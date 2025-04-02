﻿using System;
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
/// The main <see langword="class"/> for converting markdown to PDF.<br/>
/// </summary>
/// <example>
/// The following code example shows how to convert a file "README.md" in the current directory to PDF.
/// The output will be saved as "README.pdf" in the same directory.
/// <code>
/// var converter = new Markdown2PdfConverter();
/// var resultPath = await converter.Convert("README.md");
/// </code>
/// To further specify the conversion process, <see cref="Markdown2PdfOptions"/> can be passed to the converter.
/// <code>
/// var options = new Markdown2PdfOptions {
///   HeaderHtml = File.ReadAllText("header.html"),
///   FooterHtml = File.ReadAllText("footer.html"),
///   DocumentTitle = "Example PDF",
/// };
/// var converter = new Markdown2PdfConverter(options);
/// </code>
/// </example>
public class Markdown2PdfConverter : IConvertionEvents {

  /// <summary>
  /// Contains all options this converter uses for generating the PDF.
  /// </summary>
  /// <remarks>Can be set with the constructor <see cref="Markdown2PdfConverter(Markdown2PdfOptions)"/>.</remarks>
  public Markdown2PdfOptions Options { get; }

  /// <summary>
  /// The template used for generating the HTML which then gets converted into PDF.
  /// </summary>
  /// <remarks>Modify this to get more control over the HTML generation (e.g. to add your own JS-Scripts).</remarks>
  public string ContentTemplate { get; set; }

  /// <summary>
  /// The PDF file name without extension.
  /// </summary>
  public string? OutputFileName { get; private set; }

  /// <summary>
  /// The <see href="https://github.com/xoofx/markdig">Markdig</see> <see cref="MarkdownPipelineBuilder"/> used for the markdown to HTML conversion.
  /// </summary>
  /// <remarks>
  /// This <see cref="MarkdownPipelineBuilder"/> has the following extensions enabled by default:
  /// <br> * </br><see cref="MarkdownExtensions.UseAdvancedExtensions"/>
  /// <br> * </br><see cref="MarkdownExtensions.UseYamlFrontMatter"/>
  /// <br> * </br><see cref="MarkdownExtensions.UseEmojiAndSmiley(MarkdownPipelineBuilder, bool)"/>
  /// <br> * </br><see cref="MarkdownExtensions.UseAutoIdentifiers"/> with <see cref="AutoIdentifierOptions.AutoLink"/>
  /// </remarks>
  public MarkdownPipelineBuilder PipelineBuilder { get; } = new MarkdownPipelineBuilder()
    .UseAdvancedExtensions()
    .UseYamlFrontMatter()
    .UseEmojiAndSmiley();

  /// <inheritdoc cref="IConvertionEvents.BeforeHtmlConversion"/>
  public event EventHandler<MarkdownArgs>? BeforeHtmlConversion;
  event EventHandler<MarkdownArgs> IConvertionEvents.BeforeHtmlConversion {
    add => BeforeHtmlConversion += value;
    remove => BeforeHtmlConversion -= value;
  }

  /// <inheritdoc cref="IConvertionEvents.OnTemplateModelCreating"/>
  public event EventHandler<TemplateModelArgs>? OnTemplateModelCreating;
  event EventHandler<TemplateModelArgs>? IConvertionEvents.OnTemplateModelCreating {
    add => OnTemplateModelCreating += value;
    remove => OnTemplateModelCreating -= value;
  }

  /// <inheritdoc cref="IConvertionEvents.OnTempPdfCreatedEvent"/>
  public event EventHandler<PdfArgs>? OnTempPdfCreatedEvent;
  event EventHandler<PdfArgs>? IConvertionEvents.OnTempPdfCreatedEvent {
    add => OnTempPdfCreatedEvent += value;
    remove => OnTempPdfCreatedEvent -= value;
  }

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
  /// Instantiates a new <see cref="Markdown2PdfConverter"/>.
  /// </summary>
  /// <param name="options">Optional options to specify how to convert the markdown.</param>
  public Markdown2PdfConverter(Markdown2PdfOptions? options = null) {
    this.Options = options ?? new Markdown2PdfOptions();

    // Switch to AutoLink Option to allow non-ASCII characters
    this.PipelineBuilder.Extensions.Remove(this.PipelineBuilder.Extensions.Find<AutoIdentifierExtension>()!);
    this.PipelineBuilder.UseAutoIdentifiers(AutoIdentifierOptions.AutoLink);

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

  /// <summary>
  /// Instantiates a new <see cref="Markdown2PdfConverter"/>. 
  /// The <see cref="Markdown2PdfOptions"/> are loaded from a <i>YAML front matter block</i> 
  /// at the start of the given markdown document.
  /// </summary>
  /// <param name="markdownFilePath">Path to the markdown file containing the <i>YAML front matter</i>.</param>
  /// <returns>The new <see cref="Markdown2PdfConverter"/>.</returns>
  /// <example>
  /// Use this at the beginning of the markdown file:
  /// <code language="markdown">
  /// ---
  /// document-title: myDocumentTitle
  /// metadata-title: myMetadataTitle
  /// module-options: Remote # or None or path to node_module directory
  /// theme: Github # or Latex or None or path to css file
  /// code-highlight-theme: Github
  /// enable-auto-language-detection: true
  /// header-html: "&lt;div class='document-title' style='background-color: #5eafed; width: 100%; padding: 5px'&gt;&lt;/div&gt;"
  /// # footer-html: "&lt;div&gt;hello world&lt;/div&gt;"
  /// # custom-head-content: "&lt;style&gt;h2 { page-break-before: always; }&lt;/style&gt;"
  /// # chrome-path: "C:\Program Files\Google\Chrome\Application\chrome.exe"
  /// keep-html: false
  /// margin-options:
  ///   top: 80px
  ///   bottom: 50px
  ///   left: 50px
  ///   right: 50px
  /// is-landscape: false
  /// format: A4
  /// scale: 1
  /// table-of-contents:
  ///   list-style: decimal
  ///   min-depth-level: 2
  ///   max-depth-level: 6
  ///   page-number-options:
  ///     tab-leader: dots
  /// ---
  /// 
  /// # Here the normal markdown content starts
  /// </code>
  /// </example>
  /// <remarks>
  /// Instead of three dashes (---) an HTML comment (&lt;!-- --&gt;) can also be used to wrap the YAML.
  /// </remarks>
  public static Markdown2PdfConverter CreateWithInlineOptionsFromFile(string markdownFilePath) {
    var options = InlineOptionsParser.ParseYamlFrontMatter(markdownFilePath);
    return new Markdown2PdfConverter(options);
  }

  /// <inheritdoc cref="CreateWithInlineOptionsFromFile(string)"/>
  /// <param name="markdownFile">Markdown file containing the <i>YAML front matter</i>.</param>
  public static Markdown2PdfConverter CreateWithInlineOptionsFromFile(FileInfo markdownFile)
    => CreateWithInlineOptionsFromFile(markdownFile.FullName);

  /// <inheritdoc cref="Convert(FileInfo, FileInfo)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  /// <returns>The newly created PDF file.</returns>
  public async Task<FileInfo> Convert(FileInfo markdownFile) => new(await this.Convert(markdownFile.FullName));

  /// <summary>
  /// Converts the given markdown-file to PDF.
  /// </summary>
  /// <param name="markdownFile"><see cref="FileInfo"/> containing the markdown.</param>
  /// <param name="outputFile"><see cref="FileInfo"/> for saving the generated PDF.</param>
  public async Task Convert(FileInfo markdownFile, FileInfo outputFile) => await this.Convert(markdownFile.FullName, outputFile.FullName);

  /// <inheritdoc cref="Convert(string, string)"/>
  /// <remarks>The PDF will be saved in the same location as the markdown file with the naming convention "markdownFileName.pdf".</remarks>
  public async Task<string> Convert(string markdownFilePath) {
    var markdownDir = Path.GetDirectoryName(Path.GetFullPath(markdownFilePath));
    var outputFileName = Path.GetFileNameWithoutExtension(markdownFilePath) + ".pdf";
    var outputFilePath = Path.Combine(markdownDir, outputFileName);
    await this.Convert(markdownFilePath, outputFilePath);

    return outputFilePath;
  }

  /// <summary>
  /// Converts the given markdown file to PDF.
  /// </summary>
  /// <param name="markdownFilePath">Path to the markdown file.</param>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <remarks>The PDF will be saved at the path specified in <paramref name="outputFilePath"/>.</remarks>
  /// <returns>Filepath to the generated pdf.</returns>
  public async Task<string> Convert(string markdownFilePath, string outputFilePath) {
    markdownFilePath = Path.GetFullPath(markdownFilePath);
    outputFilePath = Path.GetFullPath(outputFilePath);
    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath));

    var markdownContent = File.ReadAllText(markdownFilePath);

    await this._Convert(outputFilePath, markdownContent, markdownFilePath);

    return outputFilePath;
  }

  /// <inheritdoc cref="Convert(IEnumerable{string}, string)"/>
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
  /// Converts the given enumerable of markdown files to PDF.
  /// </summary>
  /// <param name="markdownFilePaths">Enumerable with paths of the markdown files.</param>
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
  /// Converts the given list of markdown files to PDF.
  /// </summary>
  /// <param name="outputFilePath">File path for saving the PDF to.</param>
  /// <param name="markdownContent">String holding all markdown data.</param>
  /// <param name="markdownFilePath">Path of the first markdown file.</param>
  private async Task _Convert(string outputFilePath, string markdownContent, string markdownFilePath) {
    // Rerun logic
    await this._ConvertInternal(outputFilePath, markdownContent, markdownFilePath);
    var args = new PdfArgs(outputFilePath);
    if (Options.TableOfContents?.PageNumberOptions != null) { // If PageNumbers enabled, PDF needs to be generated twice
      var tempPath = _CreateTempFilePath(outputFilePath);
      await this._ConvertInternal(tempPath, markdownContent, markdownFilePath);
      this.OnTempPdfCreatedEvent?.Invoke(this, new PdfArgs(tempPath)); // TODO: trigger at right time
      File.Delete(tempPath);
    }

    await this._ConvertInternal(outputFilePath, markdownContent, markdownFilePath);
  }

  private static string _CreateTempFilePath(string outputFilePath) {
    var outputDir = Path.GetDirectoryName(outputFilePath)!;
    return Path.Combine(outputDir, $"~temp{DateTime.UtcNow:yyyyMMddHHmmssfff}.TEMPPDF");
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
    var markdownArgs = new MarkdownArgs(markdownContent);
    this.BeforeHtmlConversion?.Invoke(this, markdownArgs);
    markdownContent = markdownArgs.MarkdownContent;

    var pipeline = this.PipelineBuilder.Build();

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

    this.OnTemplateModelCreating?.Invoke(this, new TemplateModelArgs(templateModel));

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

    var browserFetcher = new BrowserFetcher();
    var installed = browserFetcher.GetInstalledBrowsers();

    if (!installed.Any()) {
      Console.WriteLine("Path to chrome was not specified. Downloading chrome...");
      _ = await browserFetcher.DownloadAsync();
    }

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}
