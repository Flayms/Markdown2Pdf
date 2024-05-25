namespace Markdown2Pdf.Options;

/// <summary>
/// A theme from a CSS file.
/// </summary>
/// <param name="cssPath">Path to the CSS file to use as the theme.</param>
public class CustomTheme(string cssPath) : Theme {

  /// <summary>
  /// The path to the CSS file.
  /// </summary>
  public string CssPath { get; } = cssPath;
}
