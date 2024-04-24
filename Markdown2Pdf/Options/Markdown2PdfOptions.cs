using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

/// <summary>
/// All the options for the conversion.
/// </summary>
public class Markdown2PdfOptions {

  /// <summary>
  /// Options that decide from where to load additional modules.
  /// <value>Default: <see cref="ModuleOptions.Remote"/>.</value>
  /// </summary>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;

  /// <summary>
  /// The styling to apply to the document. Default: <see cref="Theme.Github"/>.
  /// </summary>
  public Theme Theme { get; set; } = Theme.Github;

  /// <summary>
  /// The theme to use for highlighting code blocks.
  /// <value>Default: <see cref="CodeHighlightTheme.Github"/>.</value>
  /// </summary>
  public CodeHighlightTheme CodeHighlightTheme { get; set; } = CodeHighlightTheme.Github;

  /// <summary>
  /// Auto detect the language for code blocks without specfied language.
  /// <value>Default: <see langword="false"/>.</value>
  /// </summary>
  public bool EnableAutoLanguageDetection { get; set; }

  /// <summary>
  /// An HTML string to use as the document-header.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  /// <remarks>
  /// Html-elements with the classes <c>date</c>, <c>title</c>, <c>document-title</c>, <c>url</c>, <c>pageNumber</c> will get their content replaced based on the information.
  /// Note that <c>document-title</c> can be set with the option <see cref="DocumentTitle"/>.
  /// </remarks>
  public string? HeaderHtml { get; set; }

  ///<inheritdoc cref="HeaderHtml"/>
  /// <summary>
  /// An HTML string to use as the document-footer.
  /// </summary>
  public string? FooterHtml { get; set; }

  /// <summary>
  /// The title of this document. Can be injected into the header / footer by adding the class <c>document-title</c> to the element.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  public string? DocumentTitle { get; set; }

  /// <summary>
  /// A <see langword="string"/> containing any content valid inside a HTML <c>&lt;head&gt;</c> 
  /// to apply extra scripting / styling to the document.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  /// <example>
  /// Example adding PDF pagebreaks:
  /// <code>
  /// options.CustomHeadContent = "<style>h1, h2, h3 { page-break-before: always; }</style>";
  /// </code>
  /// </example>
  public string? CustomHeadContent { get; set; }

  /// <summary>
  /// Path to chrome or chromium executable. If set to <see langword="null"/> downloads chromium by itself.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  public string? ChromePath { get; set; }

  /// <summary>
  /// Doesn't delete the HTML-file used for generating the PDF if set to <see langword="true"/>.
  /// <value>Default: <see langword="false"/>.</value>
  /// </summary>
  public bool KeepHtml { get; set; }

  /// <summary>
  /// Css-margins for the sides of the document.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  public MarginOptions? MarginOptions { get; set; }

  /// <summary>
  /// Paper orientation.
  /// <value>Default: <see langword="false"/>.</value>
  /// </summary>
  public bool IsLandscape { get; set; }

  /// <summary>
  /// The paper format for the PDF.
  /// <value>Default: <see cref="PaperFormat.A4"/>.</value>
  /// </summary>
  public PaperFormat Format { get; set; } = PaperFormat.A4;

  /// <inheritdoc cref="PuppeteerSharp.PdfOptions.Scale"/>
  public decimal Scale { get; set; } = 1;

  /// <inheritdoc cref="TableOfContentsOptions"/>
  /// <value>Default: <see langword="null"/>.</value>
  public TableOfContentsOptions? TableOfContents { get; set; } = null;
}
