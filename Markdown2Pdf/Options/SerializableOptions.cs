using System.Reflection;
using PuppeteerSharp.Media;

namespace Markdown2Pdf.Options;

internal class SerializableOptions {
  public string? ModuleOptions { get; set; }
  public string? Theme { get; set; }
  public string? CodeHighlightTheme { get; set; }
  public bool EnableAutoLanguageDetection { get; set; }
  public string? HeaderHtml { get; set; }
  public string? FooterHtml { get; set; }
  public string? DocumentTitle { get; set; }
  public string? MetadataTitle { get; set; }
  public string? CustomHeadContent { get; set; }
  public string? ChromePath { get; set; }
  public bool KeepHtml { get; set; }
  public MarginOptions? MarginOptions { get; set; }
  public bool IsLandscape { get; set; }
  public string? Format { get; set; }
  public decimal? Scale { get; set; }
  public TableOfContentsOptions? TableOfContents { get; set; } = null;

  public Markdown2PdfOptions ToMarkdown2PdfOptions() {
    var options = new Markdown2PdfOptions();

    // TODO: use _TryGetPropertyValue
    if (this.ModuleOptions != null) {
      options.ModuleOptions = this.ModuleOptions.ToLower() switch {
        "none" => Options.ModuleOptions.None,
        "remote" => Options.ModuleOptions.Remote,
        _ => Options.ModuleOptions.FromLocalPath(this.ModuleOptions)
      };
    }

    // TODO: use _TryGetPropertyValue
    if (this.Theme != null) {
      options.Theme = this.Theme.ToLower() switch {
        "none" => Options.Theme.None,
        "github" => Options.Theme.Github,
        "latex" => Options.Theme.Latex,
        _ => Options.Theme.Custom(this.Theme)
      };
    }

    if (this.CodeHighlightTheme != null && _TryGetPropertyValue<CodeHighlightTheme>(this.CodeHighlightTheme, out var codeHighlightTheme))
      options.CodeHighlightTheme = codeHighlightTheme;

    if (this.EnableAutoLanguageDetection)
      options.EnableAutoLanguageDetection = true;

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

    if (this.KeepHtml)
      options.KeepHtml = true;

    if (this.MarginOptions != null) // TODO: double check
      options.MarginOptions = this.MarginOptions;

    if (this.IsLandscape)
      options.IsLandscape = true;

    if (this.Format != null && _TryGetPropertyValue<PaperFormat>(this.Format, out var format))
      options.Format = format;

    if (this.Scale != null)
      options.Scale = this.Scale.Value;

    if (this.TableOfContents != null) // TODO: double check
      options.TableOfContents = this.TableOfContents;

    return options;
  }

  private static bool _TryGetPropertyValue<T>(string propertyName, out T propertyValue) {
    var property = typeof(T).GetProperty(propertyName,
    BindingFlags.Static | BindingFlags.Public | BindingFlags.IgnoreCase);

    if (property == null) {
      propertyValue = default!;
      return false;
    }

    propertyValue = (T)property.GetValue(null, null)!;
    return true;
  }
}
