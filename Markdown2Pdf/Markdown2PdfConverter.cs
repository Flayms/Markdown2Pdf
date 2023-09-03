using Markdig;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Reflection;
using System.Linq;
using Markdown2Pdf.Options;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using Markdown2Pdf.Models;
using Markdown2Pdf.Services;

namespace Markdown2Pdf;

/// <summary>
/// Use this to parse markdown to PDF.
/// </summary>
public class Markdown2PdfConverter {

  /// <summary>
  /// All the options this converter uses for generating the PDF.
  /// </summary>
  public Markdown2PdfOptions Options { get; }

  private readonly IReadOnlyDictionary<string, ModuleInformation> _packagelocationMapping = new Dictionary<string, ModuleInformation>() {
    {"mathjax", new ("https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js", "mathjax/es5/tex-mml-chtml.js") },
    {"mermaid", new ("https://cdn.jsdelivr.net/npm/mermaid@10.2.3/dist/mermaid.min.js", "mermaid/dist/mermaid.min.js") }
  };

  private readonly IReadOnlyDictionary<ThemeType, ModuleInformation> _themeSourceMapping = new Dictionary<ThemeType, ModuleInformation>() {
    {ThemeType.Github, new("https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.2.0/github-markdown.min.css", "github-markdown-css/github-markdown.css") },
    {ThemeType.Latex, new("https://latex.now.sh/style.css", "latex.css/style.min.css") },
  };

  private readonly EmbeddedResourceService _embeddedResourceService = new EmbeddedResourceService();

  private const string _STYLE_KEY = "stylePath";
  private const string _BODY_KEY = "body";
  private const string _DOCUMENT_TITLE_CLASS = "document-title";
  private const string _TEMPLATE_WITH_SCRIPTS_FILE_NAME = "ContentTemplate.html";
  private const string _TEMPLATE_NO_SCRIPTS_FILE_NAME = "ContentTemplate_NoScripts.html";
  private const string _HEADER_FOOTER_STYLES_FILE_NAME = "Header-Footer-Styles.html";

  /// <summary>
  /// Instantiate a new <see cref="Markdown2PdfConverter"/>.
  /// </summary>
  /// <param name="options">Optional options to specify how to convert the markdown.</param>
  public Markdown2PdfConverter(Markdown2PdfOptions? options = default) {
    this.Options = options ?? new Markdown2PdfOptions();

    var moduleOptions = this.Options.ModuleOptions;

    //adjust local dictionary paths
    if (moduleOptions.ModuleLocation == ModuleLocation.Custom
      || moduleOptions.ModuleLocation == ModuleLocation.Global) {
      var path = moduleOptions.ModulePath!;

      this._packagelocationMapping = this._UpdateDic(this._packagelocationMapping, path);
      this._themeSourceMapping = this._UpdateDic(this._themeSourceMapping, path);
    }
  }

  private IReadOnlyDictionary<TKey, ModuleInformation> _UpdateDic<TKey>(IReadOnlyDictionary<TKey, ModuleInformation> dicToUpdate, string path) {
    var updatedLocationMapping = new Dictionary<TKey, ModuleInformation>();

    foreach (var kvp in dicToUpdate) {
      var key = kvp.Key;
      var absoluteNodePath = Path.Combine(path, kvp.Value.NodePath);
      updatedLocationMapping[key] = new(kvp.Value.RemotePath, absoluteNodePath);
    }

    return updatedLocationMapping;
  }

  /// <inheritdoc cref="Convert(FileInfo, FileInfo)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>The newly created PDF-file.</returns>
  public async Task<FileInfo> Convert(FileInfo markdownFile) => new ( await this.Convert(markdownFile.FullName));

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFile"><see cref="FileInfo"/> containing the markdown.</param>
  /// <param name="outputFile"><see cref="FileInfo"/> for saving the generated PDF.</param>
  public async Task Convert(FileInfo markdownFile, FileInfo outputFile) => await this.Convert(markdownFile.FullName, outputFile.FullName);

  /// <inheritdoc cref="Convert(string, string)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>Filepath to the generated pdf.</returns>
  public async Task<string> Convert(string markdownFilePath) {
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var outputFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".pdf";
    var outputFilePath = Path.Combine(markdownDir, outputFileName);
    await this.Convert(markdownFilePath, outputFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFilePath">Path to the markdown file.</param>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  public async Task Convert(string markdownFilePath, string outputFilePath) {
    markdownFilePath = Path.GetFullPath(markdownFilePath);
    outputFilePath = Path.GetFullPath(outputFilePath);

    var markdownContent = File.ReadAllText(markdownFilePath);

    var html = this._GenerateHtml(markdownContent);

    //todo: make temp-file
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var markdownFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".html";
    var htmlPath = Path.Combine(markdownDir, markdownFileName);
    File.WriteAllText(htmlPath, html);

    await this._GeneratePdfAsync(htmlPath, outputFilePath);

    if (!this.Options.KeepHtml)
      File.Delete(htmlPath);
  }

  internal string _GenerateHtml(string markdownContent) {
    //todo: decide on how to handle pipeline better
    var pipeline = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UseDiagrams()
      .Build();
    //.UseSyntaxHighlighting();
    var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

    //todo: support more plugins
    //todo: code-color markup

    var assembly = Assembly.GetAssembly(typeof(Markdown2PdfConverter));
    var currentLocation = Path.GetDirectoryName(assembly.Location);

    var templateName = this.Options.ModuleOptions == ModuleOptions.None
      ? _TEMPLATE_NO_SCRIPTS_FILE_NAME
      : _TEMPLATE_WITH_SCRIPTS_FILE_NAME;

    var templateHtml = this._embeddedResourceService.GetResourceContent("ContentTemplate.html");

    //create model for templating html
    var templateModel = new Dictionary<string, string>();

    //load correct module paths
    var isRemote = this.Options.ModuleOptions.ModuleLocation == ModuleLocation.Remote;

    foreach (var kvp in this._packagelocationMapping)
      templateModel.Add(kvp.Key, isRemote ? kvp.Value.RemotePath : kvp.Value.NodePath);

    var theme = this.Options.Theme.Type;
    if (theme == ThemeType.Github || theme == ThemeType.Latex) {
      var value = this._themeSourceMapping[theme];
      templateModel.Add(_STYLE_KEY, isRemote ? value.RemotePath : value.NodePath);
    }

    templateModel.Add(_BODY_KEY, htmlContent);

    return TemplateFiller.FillTemplate(templateHtml, templateModel);
  }

  private async Task _GeneratePdfAsync(string htmlFilePath, string outputFilePath) {
    using var browser = await this._CreateBrowserAsync();
    var page = await browser.NewPageAsync();
    var options = this.Options;
    var margins = options.MarginOptions;

    await page.GoToAsync("file:///" + htmlFilePath, WaitUntilNavigation.Networkidle2);

    var puppeteerMargins = new PuppeteerSharp.Media.MarginOptions();
    if (margins != null) {
      //todo: remove double initialization
      puppeteerMargins = new PuppeteerSharp.Media.MarginOptions {
        Top = margins.Top,
        Bottom = margins.Bottom,
        Left = margins.Left,
        Right = margins.Right,
      };
    }

    var pdfOptions = new PdfOptions {
      Format = options.Format,
      Landscape = options.IsLandscape,
      PrintBackground = true, //todo: background doesnt work for margins
      MarginOptions = puppeteerMargins
    };

    var hasHeaderFooterStylesAdded = false;

    //todo: default header is super small
    if (options.HeaderUrl != null) {
      pdfOptions.DisplayHeaderFooter = true;
      var html = this._FillHeaderFooterDocumentTitleClass(File.ReadAllText(options.HeaderUrl));
      pdfOptions.HeaderTemplate = _AddHeaderFooterStylesToHtml(html);
      hasHeaderFooterStylesAdded = true;
    }

    if (options.FooterUrl != null) {
      pdfOptions.DisplayHeaderFooter = true;
      var html = this._FillHeaderFooterDocumentTitleClass(File.ReadAllText(options.FooterUrl));
      pdfOptions.FooterTemplate = !hasHeaderFooterStylesAdded ? this._AddHeaderFooterStylesToHtml(html) : html;
    }

    await page.EmulateMediaTypeAsync(MediaType.Screen);
    await page.PdfAsync(outputFilePath, pdfOptions);
  }

  /// <summary>
  /// Applies extra styles to the given header / footer html because the default ones don't look good on the pdf.
  /// </summary>
  /// <param name="html">The header / footer html to add the styles to.</param>
  /// <returns>The html with added styles.</returns>
  private string _AddHeaderFooterStylesToHtml(string html) => this._embeddedResourceService.GetResourceContent(_HEADER_FOOTER_STYLES_FILE_NAME) + html;

  private string _FillHeaderFooterDocumentTitleClass(string html) {
    if (this.Options.DocumentTitle == null)
      return html;

    //todo: document this
    //need to wrap bc html could have multiple roots
    var htmlWrapped = $"<root>{html}</root>";
    var xDocument = XDocument.Parse(htmlWrapped);
    var titleElements = xDocument.XPathSelectElements($"//*[contains(@class, '{_DOCUMENT_TITLE_CLASS}')]");

    foreach (var titleElement in titleElements)
      titleElement.Value = this.Options.DocumentTitle;

    var resultHtml = xDocument.ToString();

    //remove helper wrap
    var lines = resultHtml.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    resultHtml = string.Join(Environment.NewLine, lines.Take(lines.Length - 1).Skip(1));

    return resultHtml;
  }

  private async Task<IBrowser> _CreateBrowserAsync() {
    var launchOptions = new LaunchOptions {
      Headless = true
    };

    if (this.Options.ChromePath == null) {
      using var browserFetcher = new BrowserFetcher();
      var localRevs = browserFetcher.LocalRevisions();

      if (!localRevs.Contains(BrowserFetcher.DefaultChromiumRevision)) {
        Console.WriteLine("Downloading chromium...");
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
      }
    } else
      launchOptions.ExecutablePath = this.Options.ChromePath;

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}