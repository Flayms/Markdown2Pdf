namespace Markdown2Pdf.Options;

public class Theme {

  public ThemeType Type { get; }
  public string? CssPath { get; } //todo: hide this by inheritance instead of null

  public static Theme None => new (ThemeType.None);

  /// <summary>
  /// Githubs markdown theme.
  /// </summary>
  /// <remarks>If the option <see cref="ModuleOptions.Global "/> or <see cref="ModuleOptions.FromLocalPath(string)"/> 
  /// is being used, the npm-package <c>github-markdown-css</c> needs to be installed in the corresponding location.</remarks>
  public static Theme Github => new (ThemeType.Github);

  /// <summary>
  /// Latex like document styling.
  /// </summary>
  /// <remarks>If the option <see cref="ModuleOptions.Global "/> or <see cref="ModuleOptions.FromLocalPath(string)"/> 
  /// is being used, the npm-package <c>latex.css</c> needs to be installed in the corresponding location.</remarks>
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