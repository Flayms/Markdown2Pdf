using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

/// <summary>
/// All the options for the conversion.
/// </summary>
public class Markdown2PdfOptions {
  // TODO: option for generating table of contents

  /// <summary>
  /// Options that decide from where to load additional modules. Default: <see cref="ModuleOptions.Remote"/>.
  /// </summary>
  public ModuleOptions ModuleOptions { get; set; } = ModuleOptions.Remote;

  /// <summary>
  /// The styling to apply to the document. Default: <see cref="Theme.Github"/>.
  /// </summary>
  public Theme Theme { get; set; } = Theme.Github;

  /// <summary>
  /// The theme to use for highlighting code blocks. Default: <see cref="CodeHighlightTheme.Github"/>.
  /// </summary>
  public CodeHighlightTheme CodeHighlightTheme { get; set; } = CodeHighlightTheme.Github;

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
  /// A <see langword="string"/> containing CSS to apply extra styling to the document.
  /// </summary>
  public string CustomCss { get; set; } = string.Empty;

  /// <summary>
  /// Path to chrome or chromium executable. If set to <see langword="null"/> downloads chromium by itself.
  /// </summary>
  public string? ChromePath { get; set; }

  /// <summary>
  /// Doesn't delete the html-file used for generating the PDF if set to <see langword="true"/>. Default: <see langword="false"/>.
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

  /// <inheritdoc cref="PuppeteerSharp.PdfOptions.Scale"/>
  public decimal Scale { get; set; } = 1;
}
