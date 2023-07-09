namespace Markdown2Pdf.Options;

public class Markdown2PdfOptions {
  //todo: font-name
  //todo: font-size
  //todo: option for generating table of contents

  /// <summary>
  /// Path to an html-file to use as the document-header.
  /// </summary>
  public string? HeaderUrl { get; set; }

  /// <summary>
  /// Path to an html-file to use as the document-footer.
  /// </summary>
  public string? FooterUrl { get; set; }

  /// <summary>
  /// Css-margins for the sides of the document.
  /// </summary>
  public MarginOptions? MarginOptions { get; set; }

  /// <summary>
  /// Path to chrome or chromium executable or self-downloads it if <see langword="null"/>.
  /// </summary>
  public string? ChromePath { get; set; }

  /// <summary>
  /// Options that decide from where to load additional modules. Default: <see cref="ModuleOptions.Remote"/>.
  /// </summary>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;
}