

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
  public override string ToString() {
    return this._sheetName;
  }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
  public static CodeHighlightTheme ONE_C_LIGHT => new("1c-light.css");
  public static CodeHighlightTheme A11Y_DARK => new("a11y-dark.css");
  public static CodeHighlightTheme A11Y_LIGHT => new("a11y-light.css");
  public static CodeHighlightTheme AGATE => new("agate.css");
  public static CodeHighlightTheme AN_OLD_HOPE => new("an-old-hope.css");
  public static CodeHighlightTheme ANDROID_STUDIO => new("androidstudio.css");
  public static CodeHighlightTheme ARDUINO_LIGHT => new("arduino-light.css");
  public static CodeHighlightTheme ARTA => new("arta.css");
  public static CodeHighlightTheme ASCETIC => new("ascetic.css");
  public static CodeHighlightTheme ATOM_ONE_DARK_REASONABLE => new("atom-one-dark-reasonable.css");
  public static CodeHighlightTheme ATOM_ONE_DARK => new("atom-one-dark.css");
  public static CodeHighlightTheme ATOM_ONE_LIGHT => new("atom-one-light.css");
  public static CodeHighlightTheme BROWN_PAPER => new("brown-paper.css");
  public static CodeHighlightTheme BROWN_PAPERSQ_PNG => new("brown-papersq.png");
  public static CodeHighlightTheme CODEPEN_EMBED => new("codepen-embed.css");
  public static CodeHighlightTheme COLOR_BREWER => new("color-brewer.css");
  public static CodeHighlightTheme DARK => new("dark.css");
  public static CodeHighlightTheme DEFAULT => new("default.css");
  public static CodeHighlightTheme DEVIBEANS => new("devibeans.css");
  public static CodeHighlightTheme DOCCO => new("docco.css");
  public static CodeHighlightTheme FAR => new("far.css");
  public static CodeHighlightTheme FELIPEC => new("felipec.css");
  public static CodeHighlightTheme FOUNDATION => new("foundation.css");
  public static CodeHighlightTheme GITHUB_DARK_DIMMED => new("github-dark-dimmed.css");
  public static CodeHighlightTheme GITHUB_DARK => new("github-dark.css");
  public static CodeHighlightTheme GITHUB => new("github.css");
  public static CodeHighlightTheme GML => new("gml.css");
  public static CodeHighlightTheme GOOGLECODE => new("googlecode.css");
  public static CodeHighlightTheme GRADIENT_DARK => new("gradient-dark.css");
  public static CodeHighlightTheme GRADIENT_LIGHT => new("gradient-light.css");
  public static CodeHighlightTheme GRAYSCALE => new("grayscale.css");
  public static CodeHighlightTheme HYBRID => new("hybrid.css");
  public static CodeHighlightTheme IDEA => new("idea.css");
  public static CodeHighlightTheme INTELLIJ_LIGHT => new("intellij-light.css");
  public static CodeHighlightTheme IR_BLACK => new("ir-black.css");
  public static CodeHighlightTheme ISBL_EDITOR_DARK => new("isbl-editor-dark.css");
  public static CodeHighlightTheme ISBL_EDITOR_LIGHT => new("isbl-editor-light.css");
  public static CodeHighlightTheme KIMBIE_DARK => new("kimbie-dark.css");
  public static CodeHighlightTheme KIMBIE_LIGHT => new("kimbie-light.css");
  public static CodeHighlightTheme LIGHTFAIR => new("lightfair.css");
  public static CodeHighlightTheme LIOSHI => new("lioshi.css");
  public static CodeHighlightTheme MAGULA => new("magula.css");
  public static CodeHighlightTheme MONO_BLUE => new("mono-blue.css");
  public static CodeHighlightTheme MONOKAI_SUBLIME => new("monokai-sublime.css");
  public static CodeHighlightTheme MONOKAI => new("monokai.css");
  public static CodeHighlightTheme NIGHT_OWL => new("night-owl.css");
  public static CodeHighlightTheme NNFx_DARK => new("nnfx-dark.css");
  public static CodeHighlightTheme NNFX_LIGHT => new("nnfx-light.css");
  public static CodeHighlightTheme NORD => new("nord.css");
  public static CodeHighlightTheme OBSIDIAN => new("obsidian.css");
  public static CodeHighlightTheme PANDA_SYNTAX_DARK => new("panda-syntax-dark.css");
  public static CodeHighlightTheme PANDA_SYNTAX_LIGHT => new("panda-syntax-light.css");
  public static CodeHighlightTheme PARAISO_DARK => new("paraiso-dark.css");
  public static CodeHighlightTheme PARAISO_LIGHT => new("paraiso-light.css");
  public static CodeHighlightTheme POJOAQUE => new("pojoaque.css");
  public static CodeHighlightTheme PUREBASIC => new("purebasic.css");
  public static CodeHighlightTheme QTCREATOR_DARK => new("qtcreator-dark.css");
  public static CodeHighlightTheme QTCREATOR_LIGHT => new("qtcreator-light.css");
  public static CodeHighlightTheme RAINBOW => new("rainbow.css");
  public static CodeHighlightTheme ROUTEROS => new("routeros.css");
  public static CodeHighlightTheme SCHOOL_BOOK => new("school-book.css");
  public static CodeHighlightTheme SHADES_OF_PURPLE => new("shades-of-purple.css");
  public static CodeHighlightTheme SRCERY => new("srcery.css");
  public static CodeHighlightTheme STACKOVERFLOW_DARK => new("stackoverflow-dark.css");
  public static CodeHighlightTheme STACKOVERFLOW_LIGHT => new("stackoverflow-light.css");
  public static CodeHighlightTheme SUNBURST => new("sunburst.css");
  public static CodeHighlightTheme TOKYO_NIGHT_DARK => new("tokyo-night-dark.css");
  public static CodeHighlightTheme TOKYO_NIGHT_LIGHT => new("tokyo-night-light.css");
  public static CodeHighlightTheme TOMORROW_NIGHT_BLUE => new("tomorrow-night-blue.css");
  public static CodeHighlightTheme TOMORROW_NIGHT_BRIGHT => new("tomorrow-night-bright.css");
  public static CodeHighlightTheme VS => new("vs.css");
  public static CodeHighlightTheme VS2015 => new("vs2015.css");
  public static CodeHighlightTheme XCODE => new("xcode.css");
  public static CodeHighlightTheme XT256 => new("xt256.css");

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
