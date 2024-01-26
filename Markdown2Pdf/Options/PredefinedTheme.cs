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

/// <summary>
/// All predefined themes.
/// </summary>
public enum ThemeType {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
  None,
  Github,
  Latex
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
