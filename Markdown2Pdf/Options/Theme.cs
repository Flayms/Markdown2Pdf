namespace Markdown2Pdf.Options;

public class Theme {

  public ThemeType Type { get; }
  public string? CssPath { get; } //todo: hide this by inheritance instead of null

  public static Theme None => new (ThemeType.None);
  public static Theme Github => new (ThemeType.Github);
  public static Theme Latex => new (ThemeType.Latex);
  public static Theme Custom(string cssPath) => new (ThemeType.Custom, cssPath);


  private Theme(ThemeType type, string? cssPath = null) {
    this.Type = type;
    this.CssPath = cssPath;
  }
  
}

public enum ThemeType {
  None,
  Github,
  Latex,
  Custom
}