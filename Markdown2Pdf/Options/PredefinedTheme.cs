namespace Markdown2Pdf.Options;

/// <summary>
/// Use a predefined theme.
/// </summary>
/// <param name="type">The theme type to use.</param>
internal class PredefinedTheme(ThemeType type) : Theme {

  /// <summary>
  /// The type of this theme.
  /// </summary>
  public ThemeType Type { get; } = type;
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
