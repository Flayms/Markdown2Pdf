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

  private const string _STYLE_KEY = "stylePath";
  private const string _BODY_KEY = "body";
  private const string _DOCUMENT_TITLE_CLASS = "document-title";
  private const string _HTML_FILE_NAME = "converted.html";
  private const string _TEMPLATE_WITH_SCRIPTS = "ContentTemplate.html";
  private const string _TEMPLATE_NO_SCRIPTS = "ContentTemplate_NoScripts.html";

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
  public FileInfo Convert(FileInfo markdownFile) => new (this.Convert(markdownFile.FullName));

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFile"><see cref="FileInfo"/> containing the markdown.</param>
  /// <param name="outputFile"><see cref="FileInfo"/> for saving the generated PDF.</param>
  public void Convert(FileInfo markdownFile, FileInfo outputFile) => this.Convert(markdownFile.FullName, outputFile.FullName);

  /// <inheritdoc cref="Convert(string, string)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>Filepath to the generated pdf.</returns>
  public string Convert(string markdownFilePath) {
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var outputFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".pdf";
    var outputFilePath = Path.Combine(markdownDir, outputFileName);
    this.Convert(markdownFilePath, outputFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFilePath">Path to the markdown file.</param>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  public void Convert(string markdownFilePath, string outputFilePath) {
    markdownFilePath = Path.GetFullPath(markdownFilePath);
    outputFilePath = Path.GetFullPath(outputFilePath);

    var markdownContent = File.ReadAllText(markdownFilePath);

    var html = this._GenerateHtml(markdownContent);

    //todo: make temp-file
    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var htmlPath = Path.Combine(markdownDir, _HTML_FILE_NAME);
    File.WriteAllText(htmlPath, html);

    var task = this._GeneratePdfAsync(htmlPath, outputFilePath);
    task.Wait();

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
      ? _TEMPLATE_NO_SCRIPTS
      : _TEMPLATE_WITH_SCRIPTS;

    var templateHtmlResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("ContentTemplate.html"));

    string templateHtml;

    using (var stream = assembly.GetManifestResourceStream(templateHtmlResource))
    using (var reader = new StreamReader(stream))
      templateHtml = reader.ReadToEnd();

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

    await page.GoToAsync("file:///" + htmlFilePath, WaitUntilNavigation.Networkidle2);

    var marginOptions = new PuppeteerSharp.Media.MarginOptions();
    if (this.Options.MarginOptions != null) {
      //todo: remove double initialization
      marginOptions = new PuppeteerSharp.Media.MarginOptions {
        Top = this.Options.MarginOptions.Top,
        Bottom = this.Options.MarginOptions.Bottom,
        Left = this.Options.MarginOptions.Left,
        Right = this.Options.MarginOptions.Right,
      };
    }

    var pdfOptions = new PdfOptions {
      //todo: make this settable
      Format = PaperFormat.A4,
      MarginOptions = marginOptions
    };

    //todo: default header is super small
    if (this.Options.HeaderUrl != null)
      pdfOptions.HeaderTemplate = this._SetupHeaderFooter(File.ReadAllText(this.Options.HeaderUrl), pdfOptions);

    if (this.Options.FooterUrl != null)
      pdfOptions.FooterTemplate = this._SetupHeaderFooter(File.ReadAllText(this.Options.FooterUrl), pdfOptions);

    await page.EmulateMediaTypeAsync(MediaType.Screen);
    await page.PdfAsync(outputFilePath, pdfOptions);
  }

  private string _SetupHeaderFooter(string html, PdfOptions pdfOptions) {
    pdfOptions.DisplayHeaderFooter = true;

    if (this.Options.DocumentTitle == null)
      return html;

    //todo: document this
    //need to wrap bc html could have multiple roots
    var htmlWrapped = $"<root>{html}</root>";
    var xDocument = XDocument.Parse(htmlWrapped);
    var titleElements = xDocument.XPathSelectElements($"//*[contains(@class, '{_DOCUMENT_TITLE_CLASS}')]");
    foreach ( var titleElement in titleElements )
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