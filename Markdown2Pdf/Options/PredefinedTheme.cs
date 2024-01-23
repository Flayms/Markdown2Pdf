namespace Markdown2Pdf.Options;

internal class PredefinedTheme : Theme {

  /// <summary>
  /// The type of this theme.
  /// </summary>
  public ThemeType Type { get; }

  internal PredefinedTheme(ThemeType type) {
    this.Type = type;
  }
}

public enum ThemeType {
  None,
  Github,
  Latex
}
