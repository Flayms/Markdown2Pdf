using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using Markdown2Pdf.Options;
using Markdown2Pdf.Services;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Markdown2Pdf;

/// <summary>
/// Use this to parse markdown to PDF.
/// </summary>
public class Markdown2PdfConverter : IConvertionEvents {

  /// <summary>
  /// All the options this converter uses for generating the PDF.
  /// </summary>
  public Markdown2PdfOptions Options { get; }

  /// <summary>
  /// The template used for generating the html which then gets converted into PDF.
  /// </summary>
  public string ContentTemplate { get; set; }

  /// <summary>
  /// Pdf file name without extension.
  /// </summary>
  public string OutputFileName { get; private set; }

  private readonly EmbeddedResourceService _embeddedResourceService = new();

  private const string _CUSTOM_HEAD_KEY = "customHeadContent";
  private const string _BODY_KEY = "body";
  private const string _CODE_HIGHLIGHT_THEME_NAME_KEY = "highlightjs_theme_name";
  private const string _DISABLE_AUTO_LANGUAGE_DETECTION_KEY = "disableAutoLanguageDetection";
  private const string _DISABLE_AUTO_LANGUAGE_DETECTION_VALUE = "hljs.configure({ languages: [] });";

  private const string _DOCUMENT_TITLE_CLASS = "document-title";
  private const string _TEMPLATE_WITH_SCRIPTS_FILE_NAME = "ContentTemplate.html";
  private const string _TEMPLATE_NO_SCRIPTS_FILE_NAME = "ContentTemplate_NoScripts.html";
  private const string _HEADER_FOOTER_STYLES_FILE_NAME = "Header-Footer-Styles.html";

  /// <summary>
  /// Instantiate a new <see cref="Markdown2PdfConverter"/>.
  /// </summary>
  /// <param name="options">Optional options to specify how to convert the markdown.</param>
  public Markdown2PdfConverter(Markdown2PdfOptions? options = null) {
    this.Options = options ?? new Markdown2PdfOptions();
    var moduleOptions = this.Options.ModuleOptions;

    var templateName = this.Options.ModuleOptions == ModuleOptions.None
      ? _TEMPLATE_NO_SCRIPTS_FILE_NAME
      : _TEMPLATE_WITH_SCRIPTS_FILE_NAME;
    this.ContentTemplate = this._embeddedResourceService.GetResourceContent(templateName);

    // Services can be discarded because they stay alive through event attaching.
    if (this.Options.TableOfContents != null)
      _ = new TableOfContentsCreator(this.Options.TableOfContents, this, this._embeddedResourceService);

    _ = new ThemeService(this.Options.Theme, moduleOptions, this);
    _ = new ModuleService(this.Options.ModuleOptions, this);
    _ = new MetadataService(this.Options, this);
  }

  private event EventHandler<MarkdownArgs>? _beforeMarkdownConversion;
  event EventHandler<MarkdownArgs> IConvertionEvents.BeforeMarkdownConversion {
    add => _beforeMarkdownConversion += value;
    remove => _beforeMarkdownConversion -= value;
  }

  private event EventHandler<TemplateModelArgs>? _onTemplateModelCreating;
  event EventHandler<TemplateModelArgs>? IConvertionEvents.OnTemplateModelCreating {
    add => _onTemplateModelCreating += value;
    remove => _onTemplateModelCreating -= value;
  }

  private event EventHandler<PdfArgs>? _onPdfCreatedEvent;
  event EventHandler<PdfArgs>? IConvertionEvents.OnPdfCreatedEvent {
    add => _onPdfCreatedEvent += value;
    remove => _onPdfCreatedEvent -= value;
  }

  /// <inheritdoc cref="Convert(FileInfo, FileInfo)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>The newly created PDF-file.</returns>
  public async Task<FileInfo> Convert(FileInfo markdownFile) => new(await this.Convert(markdownFile.FullName));

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
    var markdownDir = Path.GetDirectoryName(Path.GetFullPath(markdownFilePath));
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
  /// <remarks>The PDF will be saved at the path specified in <paramref name="outputFilePath"/>.</remarks>
  public async Task<string> Convert(string markdownFilePath, string outputFilePath) {
    markdownFilePath = Path.GetFullPath(markdownFilePath);
    outputFilePath = Path.GetFullPath(outputFilePath);
    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

    var markdownContent = File.ReadAllText(markdownFilePath);

    await this._Convert(outputFilePath, markdownContent, markdownFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given enumerable of markdown-files to PDF.
  /// </summary>
  /// <param name="markdownFilePaths">Enumerable with paths to the markdown files.</param>
  /// <remarks>The PDF will be saved in the same location of the first markdown file with the naming convention "markdownFileName.pdf".</remarks>
  public async Task<string> Convert(IEnumerable<string> markdownFilePaths) {
    var first = markdownFilePaths.First();
    var markdownDir = Path.GetDirectoryName(first);
    var outputFileName = Path.GetFileNameWithoutExtension(first) + ".pdf";
    var outputFilePath = Path.Combine(markdownDir, outputFileName);
    await this.Convert(markdownFilePaths, outputFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given enumerable of markdown-files to PDF.
  /// </summary>
  /// <param name="markdownFilePaths">Enumerable with paths to the markdown files.</param>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  public async Task<string> Convert(IEnumerable<string> markdownFilePaths, string outputFilePath) {
    var markdownContent = string.Join(Environment.NewLine, markdownFilePaths.Select(File.ReadAllText));

    var markdownFilePath = Path.GetFullPath(markdownFilePaths.First());
    outputFilePath = Path.GetFullPath(outputFilePath);
    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

    await this._Convert(outputFilePath, markdownContent, markdownFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given list of markdown-files to PDF.
  /// </summary>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <param name="markdownContent">String holding all markdown data.</param>
  /// <param name="markdownFilePath">Path to the first markdown file.</param>
  private async Task _Convert(string outputFilePath, string markdownContent, string markdownFilePath) {
    // Rerun logic
    await this._ConvertInternal(outputFilePath, markdownContent, markdownFilePath);
    var args = new PdfArgs(outputFilePath);
    this._onPdfCreatedEvent?.Invoke(this, args);

    if (args.NeedsRerun)
      await this._ConvertInternal(outputFilePath, markdownContent, markdownFilePath);
  }

  private async Task _ConvertInternal(string outputFilePath, string markdownContent, string markdownFilePath) {
    this.OutputFileName = Path.GetFileNameWithoutExtension(outputFilePath);

    // generate html
    var html = this.GenerateHtml(markdownContent);

    var markdownDir = Path.GetDirectoryName(markdownFilePath);
    var htmlFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".html";
    var htmlPath = Path.Combine(markdownDir, htmlFileName);
    File.WriteAllText(htmlPath, html);

    // generate pdf
    await this._GeneratePdfAsync(htmlPath, outputFilePath);

    if (!this.Options.KeepHtml)
      File.Delete(htmlPath);
  }

  internal string GenerateHtml(string markdownContent) {
    var markdownArgs = new MarkdownArgs(ref markdownContent);
    this._beforeMarkdownConversion?.Invoke(this, markdownArgs);
    markdownContent = markdownArgs.MarkdownContent;

    var pipelineBuilder = new MarkdownPipelineBuilder()
      .UseAdvancedExtensions()
      .UseEmojiAndSmiley();

    // Switch to AutoLink Option to allow non-ASCII characters
    pipelineBuilder.Extensions.Remove(pipelineBuilder.Extensions.Find<AutoIdentifierExtension>()!);
    pipelineBuilder.UseAutoIdentifiers(AutoIdentifierOptions.AutoLink);

    var pipeline = pipelineBuilder.Build();

    var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

    var templateModel = this._CreateTemplateModel(htmlContent);

    return TemplateFiller.FillTemplate(this.ContentTemplate, templateModel);
  }

  private Dictionary<string, string> _CreateTemplateModel(string htmlContent) {
    var templateModel = new Dictionary<string, string>();

    var languageDetectionValue = this.Options.EnableAutoLanguageDetection
      ? string.Empty
      : _DISABLE_AUTO_LANGUAGE_DETECTION_VALUE;
    templateModel.Add(_DISABLE_AUTO_LANGUAGE_DETECTION_KEY, languageDetectionValue);
    templateModel.Add(_CODE_HIGHLIGHT_THEME_NAME_KEY, this.Options.CodeHighlightTheme.ToString());
    templateModel.Add(_CUSTOM_HEAD_KEY, this.Options.CustomHeadContent ?? string.Empty);
    templateModel.Add(_BODY_KEY, htmlContent);

    this._onTemplateModelCreating?.Invoke(this, new TemplateModelArgs(templateModel));

    return templateModel;
  }

  private async Task _GeneratePdfAsync(string htmlFilePath, string outputFilePath) {
    using var browser = await this._CreateBrowserAsync();
    var page = await browser.NewPageAsync();
    var options = this.Options;
    var margins = options.MarginOptions;

    _ = await page.GoToAsync("file:///" + htmlFilePath, WaitUntilNavigation.Networkidle2);

    var puppeteerMargins = margins != null
      ? new PuppeteerSharp.Media.MarginOptions {
        Top = margins.Top,
        Bottom = margins.Bottom,
        Left = margins.Left,
        Right = margins.Right,
      }
      : new PuppeteerSharp.Media.MarginOptions();

    var pdfOptions = new PdfOptions {
      Format = options.Format,
      Landscape = options.IsLandscape,
      PrintBackground = true, // TODO: background doesnt work for margins
      MarginOptions = puppeteerMargins,
      Scale = options.Scale
    };

    var hasHeaderFooterStylesAdded = false;

    // TODO: default header is super small
    if (options.HeaderHtml != null) {
      pdfOptions.DisplayHeaderFooter = true;
      var html = this._FillHeaderFooterDocumentTitleClass(options.HeaderHtml);
      pdfOptions.HeaderTemplate = this._AddHeaderFooterStylesToHtml(html);
      hasHeaderFooterStylesAdded = true;
    }

    if (options.FooterHtml != null) {
      pdfOptions.DisplayHeaderFooter = true;
      var html = this._FillHeaderFooterDocumentTitleClass(options.FooterHtml);
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
  private string _AddHeaderFooterStylesToHtml(string html)
    => this._embeddedResourceService.GetResourceContent(_HEADER_FOOTER_STYLES_FILE_NAME) + html;

  /// <summary>
  /// Inserts the document title into all elements containing the document-title class.
  /// </summary>
  /// <param name="html">Template html.</param>
  /// <returns>The html with inserted document-title.</returns>
  private string _FillHeaderFooterDocumentTitleClass(string html) {
    if (this.Options.DocumentTitle == null)
      return html;

    // need to wrap bc html could have multiple roots
    var htmlWrapped = $"<root>{html}</root>";
    var xDocument = XDocument.Parse(htmlWrapped);
    var titleElements = xDocument.XPathSelectElements($"//*[contains(@class, '{_DOCUMENT_TITLE_CLASS}')]");

    foreach (var titleElement in titleElements)
      titleElement.Value = this.Options.DocumentTitle;

    var resultHtml = xDocument.ToString();

    // remove helper wrap
    var lines = resultHtml.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
    resultHtml = string.Join(Environment.NewLine, lines.Take(lines.Length - 1).Skip(1));

    return resultHtml;
  }

  private async Task<IBrowser> _CreateBrowserAsync() {
    var launchOptions = new LaunchOptions {
      Headless = true,
      Args = ["--no-sandbox"], // needed for running inside docker
    };

    if (this.Options.ChromePath != null) {
      launchOptions.ExecutablePath = this.Options.ChromePath;
      return await Puppeteer.LaunchAsync(launchOptions);
    }

    using var browserFetcher = new BrowserFetcher();
    var installed = browserFetcher.GetInstalledBrowsers();

    if (!installed.Any()) {
      Console.WriteLine("Path to chrome was not specified. Downloading chrome...");
      _ = await browserFetcher.DownloadAsync();
    }

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}
