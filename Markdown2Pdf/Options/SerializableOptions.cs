using Markdown2Pdf.Services;
using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

/// <summary>
/// The <see cref="Markdown2PdfOptions"/> in a serializable format.
/// </summary>
public class SerializableOptions {

  /// <inheritdoc cref="Markdown2PdfOptions.ModuleOptions"/>
  public string? ModuleOptions { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.Theme"/>
  public string? Theme { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.CodeHighlightTheme"/>
  public string? CodeHighlightTheme { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.EnableAutoLanguageDetection"/>
  public bool? EnableAutoLanguageDetection { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.HeaderHtml"/>
  public string? HeaderHtml { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.FooterHtml"/>
  public string? FooterHtml { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.DocumentTitle"/>
  public string? DocumentTitle { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.MetadataTitle"/>
  public string? MetadataTitle { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.CustomHeadContent"/>
  public string? CustomHeadContent { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.ChromePath"/>
  public string? ChromePath { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.KeepHtml"/>
  public bool? KeepHtml { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.MarginOptions"/>
  public MarginOptions? MarginOptions { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.IsLandscape"/>
  public bool? IsLandscape { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.Format"/>
  public string? Format { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.Scale"/>
  public decimal? Scale { get; set; }
  /// <inheritdoc cref="Markdown2PdfOptions.TableOfContents"/>
  public TableOfContentsOptions? TableOfContents { get; set; } = null;

  /// <summary>
  /// Converts this serializable options into proper <see cref="Markdown2PdfOptions"/>.
  /// </summary>
  /// <returns>The deserialized <see cref="Markdown2PdfOptions"/>.</returns>
  public Markdown2PdfOptions ToMarkdown2PdfOptions() {
    var options = new Markdown2PdfOptions();

    if (this.ModuleOptions != null) {
      options.ModuleOptions = PropertyService.TryGetPropertyValue<ModuleOptions>(this.ModuleOptions, out var moduleOptions)
        ? moduleOptions
        : Options.ModuleOptions.FromLocalPath(this.ModuleOptions);
    }

    if (this.Theme != null) {
      options.Theme = PropertyService.TryGetPropertyValue<Theme>(this.Theme, out var theme)
        ? theme
        : Options.Theme.Custom(this.Theme);
    }

    if (this.CodeHighlightTheme != null
      && PropertyService.TryGetPropertyValue<CodeHighlightTheme>(this.CodeHighlightTheme, out var codeHighlightTheme))
      options.CodeHighlightTheme = codeHighlightTheme;

    if (this.EnableAutoLanguageDetection != null)
      options.EnableAutoLanguageDetection = this.EnableAutoLanguageDetection.Value;

    if (this.HeaderHtml != null)
      options.HeaderHtml = this.HeaderHtml;

    if (this.FooterHtml != null)
      options.FooterHtml = this.FooterHtml;

    if (this.DocumentTitle != null)
      options.DocumentTitle = this.DocumentTitle;

    if (this.MetadataTitle != null)
      options.MetadataTitle = this.MetadataTitle;

    if (this.CustomHeadContent != null)
      options.CustomHeadContent = this.CustomHeadContent;

    if (this.ChromePath != null)
      options.ChromePath = this.ChromePath;

    if (this.KeepHtml != null)
      options.KeepHtml = this.KeepHtml.Value;

    if (this.MarginOptions != null)
      options.MarginOptions = this.MarginOptions;

    if (this.IsLandscape != null)
      options.IsLandscape = this.IsLandscape.Value;

    if (this.Format != null && PropertyService.TryGetPropertyValue<PaperFormat>(this.Format, out var format))
      options.Format = format;

    if (this.Scale != null)
      options.Scale = this.Scale.Value;

    if (this.TableOfContents != null)
      options.TableOfContents = this.TableOfContents;

    return options;
  }

}
