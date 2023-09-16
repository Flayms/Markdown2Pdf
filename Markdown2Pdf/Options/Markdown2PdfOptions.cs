using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

/// <summary>
/// All the options for the conversion.
/// </summary>
public class Markdown2PdfOptions {
  //todo: font-name
  //todo: font-size
  //todo: option for generating table of contents
  //todo: light theme, dark theme

  /// <summary>
  /// Options that decide from where to load additional modules. Default: <see cref="ModuleOptions.Remote"/>.
  /// </summary>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;

  /// <summary>
  /// The styling to apply to the document. Default: <see cref="Theme.Github"/>.
  /// </summary>
  public Theme Theme { get; set; } = Theme.Github;

  /// <summary>
  /// An html string to use as the document-header.
  /// </summary>
  public string? HeaderHtml { get; set; }

  /// <summary>
  /// An html string to use as the document-footer.
  /// </summary>
  public string? FooterHtml { get; set; }

  /// <summary>
  /// The title of this document. Can be injected into the header / footer by adding the class <c>document-title</c> to the element.
  /// </summary>
  public string? DocumentTitle { get; set; }

  /// <summary>
  /// Path to chrome or chromium executable or self-downloads it if <see langword="null"/>.
  /// </summary>
  public string? ChromePath { get; set; }

  /// <summary>
  /// Doesn't delete the html-file used for the PDF if this is set to <see langword="true"/>. Default: <see langword="false"/>.
  /// </summary>
  public bool KeepHtml { get; set; }

  /// <summary>
  /// Css-margins for the sides of the document.
  /// </summary>
  public MarginOptions? MarginOptions { get; set; }

  /// <summary>
  /// Paper orientation. Default: <see langword="false"/>.
  /// </summary>
  public bool IsLandscape { get; set; }


  /// <summary>
  /// The paper format for the PDF.
  /// </summary>
  public PaperFormat Format { get; set; } = PaperFormat.A4;
}
