
namespace Markdown2Pdf.Options;

/// <summary>
/// The theme to use for styling the markdown code blocks.
/// </summary>
public readonly struct CodeHighlightTheme {

  private readonly string _sheetName;

  /// <summary>
  /// Creates a new <see cref="CodeHighlightTheme"/> with the default theme.
  /// </summary>
  public CodeHighlightTheme() {
    this._sheetName = "default.css";
  }

  private CodeHighlightTheme(string theme) {
    this._sheetName = theme;
  }

  /// <summary>
  /// Returns the css file name of the theme.
  /// </summary>
  public override string ToString() => this._sheetName;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
  public static CodeHighlightTheme OneCLight => new("1c-light.css");
  public static CodeHighlightTheme A11yDark => new("a11y-dark.css");
  public static CodeHighlightTheme A11yLight => new("a11y-light.css");
  public static CodeHighlightTheme Agate => new("agate.css");
  public static CodeHighlightTheme AnOldHope => new("an-old-hope.css");
  public static CodeHighlightTheme AndroidStudio => new("androidstudio.css");
  public static CodeHighlightTheme ArduinoLight => new("arduino-light.css");
  public static CodeHighlightTheme Arta => new("arta.css");
  public static CodeHighlightTheme Ascetic => new("ascetic.css");
  public static CodeHighlightTheme AtomOneDarkReasonable => new("atom-one-dark-reasonable.css");
  public static CodeHighlightTheme AtomOneDark => new("atom-one-dark.css");
  public static CodeHighlightTheme AtomOneLight => new("atom-one-light.css");
  public static CodeHighlightTheme BrownPaper => new("brown-paper.css");
  public static CodeHighlightTheme BrownPaperSqPng => new("brown-papersq.png");
  public static CodeHighlightTheme CodepenEmbed => new("codepen-embed.css");
  public static CodeHighlightTheme ColorBrewer => new("color-brewer.css");
  public static CodeHighlightTheme Dark => new("dark.css");
  public static CodeHighlightTheme Default => new("default.css");
  public static CodeHighlightTheme Devibeans => new("devibeans.css");
  public static CodeHighlightTheme Docco => new("docco.css");
  public static CodeHighlightTheme Far => new("far.css");
  public static CodeHighlightTheme Felipec => new("felipec.css");
  public static CodeHighlightTheme Foundation => new("foundation.css");
  public static CodeHighlightTheme GithubDarkDimmed => new("github-dark-dimmed.css");
  public static CodeHighlightTheme GithubDark => new("github-dark.css");
  public static CodeHighlightTheme Github => new("github.css");
  public static CodeHighlightTheme Gml => new("gml.css");
  public static CodeHighlightTheme Googlecode => new("googlecode.css");
  public static CodeHighlightTheme GradientDark => new("gradient-dark.css");
  public static CodeHighlightTheme GradientLight => new("gradient-light.css");
  public static CodeHighlightTheme Grayscale => new("grayscale.css");
  public static CodeHighlightTheme Hybrid => new("hybrid.css");
  public static CodeHighlightTheme Idea => new("idea.css");
  public static CodeHighlightTheme IntellijLight => new("intellij-light.css");
  public static CodeHighlightTheme IrBlack => new("ir-black.css");
  public static CodeHighlightTheme IsblEditorDark => new("isbl-editor-dark.css");
  public static CodeHighlightTheme IsblEditorLight => new("isbl-editor-light.css");
  public static CodeHighlightTheme KimbieDark => new("kimbie-dark.css");
  public static CodeHighlightTheme KimbieLight => new("kimbie-light.css");
  public static CodeHighlightTheme Lightfair => new("lightfair.css");
  public static CodeHighlightTheme Lioshi => new("lioshi.css");
  public static CodeHighlightTheme Magula => new("magula.css");
  public static CodeHighlightTheme MonoBlue => new("mono-blue.css");
  public static CodeHighlightTheme MonokaiSublime => new("monokai-sublime.css");
  public static CodeHighlightTheme Monokai => new("monokai.css");
  public static CodeHighlightTheme NightOwl => new("night-owl.css");
  public static CodeHighlightTheme NnfxDark => new("nnfx-dark.css");
  public static CodeHighlightTheme NnfxLight => new("nnfx-light.css");
  public static CodeHighlightTheme Nord => new("nord.css");
  public static CodeHighlightTheme Obsidian => new("obsidian.css");
  public static CodeHighlightTheme PandaSyntaxDark => new("panda-syntax-dark.css");
  public static CodeHighlightTheme PandaSyntaxLight => new("panda-syntax-light.css");
  public static CodeHighlightTheme ParaisoDark => new("paraiso-dark.css");
  public static CodeHighlightTheme ParaisoLight => new("paraiso-light.css");
  public static CodeHighlightTheme Pojoaque => new("pojoaque.css");
  public static CodeHighlightTheme Purebasic => new("purebasic.css");
  public static CodeHighlightTheme QtcreatorDark => new("qtcreator-dark.css");
  public static CodeHighlightTheme QtcreatorLight => new("qtcreator-light.css");
  public static CodeHighlightTheme Rainbow => new("rainbow.css");
  public static CodeHighlightTheme Routeros => new("routeros.css");
  public static CodeHighlightTheme SchoolBook => new("school-book.css");
  public static CodeHighlightTheme ShadesOfPurple => new("shades-of-purple.css");
  public static CodeHighlightTheme Srcery => new("srcery.css");
  public static CodeHighlightTheme StackoverflowDark => new("stackoverflow-dark.css");
  public static CodeHighlightTheme StackoverflowLight => new("stackoverflow-light.css");
  public static CodeHighlightTheme Sunburst => new("sunburst.css");
  public static CodeHighlightTheme TokyoNightDark => new("tokyo-night-dark.css");
  public static CodeHighlightTheme TokyoNightLight => new("tokyo-night-light.css");
  public static CodeHighlightTheme TomorrowNightBlue => new("tomorrow-night-blue.css");
  public static CodeHighlightTheme TomorrowNightBright => new("tomorrow-night-bright.css");
  public static CodeHighlightTheme Vs => new("vs.css");
  public static CodeHighlightTheme Vs2015 => new("vs2015.css");
  public static CodeHighlightTheme Xcode => new("xcode.css");
  public static CodeHighlightTheme Xt256 => new("xt256.css");

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
