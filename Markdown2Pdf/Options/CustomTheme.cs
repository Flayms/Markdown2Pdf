namespace Markdown2Pdf.Options;
internal class CustomTheme(string cssPath) : Theme {

  public string CssPath { get; } = cssPath;
}
