namespace Markdown2Pdf.Options;

public class Markdown2PdfOptions {
  //todo: font-name
  //todo: font-size
  //todo: option for generating table of contents
  //todo: light theme, dark theme

  /// <summary>
  /// Path to an html-file to use as the document-header.
  /// </summary>
  public string? HeaderUrl { get; set; }

  /// <summary>
  /// Path to an html-file to use as the document-footer.
  /// </summary>
  public string? FooterUrl { get; set; }

  /// <summary>
  /// The title of this document. Can be injected into the header / footer by adding the class <c>document-title</c> to the element.
  /// </summary>
  public string? DocumentTitle { get; set; }

  /// <summary>
  /// Path to chrome or chromium executable or self-downloads it if <see langword="null"/>.
  /// </summary>
  public string? ChromePath { get; set; }

  /// <summary>
  /// <see langword="true"/> if the created html should not be deleted.
  /// </summary>
  public bool KeepHtml { get; set; }

  /// <summary>
  /// Css-margins for the sides of the document.
  /// </summary>
  public MarginOptions? MarginOptions { get; set; }

  /// <summary>
  /// Options that decide from where to load additional modules. Default: <see cref="ModuleOptions.Remote"/>.
  /// </summary>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;

  /// <summary>
  /// The styling to apply to the document. Default: <see cref="Theme.Github"/>.
  /// </summary>
  public Theme Theme { get; set; } = Theme.Github;
}