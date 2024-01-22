

namespace Markdown2Pdf.Options;

public readonly struct HighlightJsTheme {

  private readonly string sheetName;

  public HighlightJsTheme() {
    this.sheetName = "default.css";
  }

  private HighlightJsTheme(string theme) {
    this.sheetName = theme;
  }

  public override string ToString() {
    return this.sheetName;
  }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
  public static HighlightJsTheme ONE_C_LIGHT => new("1c-light.css");
  public static HighlightJsTheme A11Y_DARK => new("a11y-dark.css");
  public static HighlightJsTheme A11Y_LIGHT => new("a11y-light.css");
  public static HighlightJsTheme AGATE => new("agate.css");
  public static HighlightJsTheme AN_OLD_HOPE => new("an-old-hope.css");
  public static HighlightJsTheme ANDROID_STUDIO => new("androidstudio.css");
  public static HighlightJsTheme ARDUINO_LIGHT => new("arduino-light.css");
  public static HighlightJsTheme ARTA => new("arta.css");
  public static HighlightJsTheme ASCETIC => new("ascetic.css");
  public static HighlightJsTheme ATOM_ONE_DARK_REASONABLE => new("atom-one-dark-reasonable.css");
  public static HighlightJsTheme ATOM_ONE_DARK => new("atom-one-dark.css");
  public static HighlightJsTheme ATOM_ONE_LIGHT => new("atom-one-light.css");
  public static HighlightJsTheme BROWN_PAPER => new("brown-paper.css");
  public static HighlightJsTheme BROWN_PAPERSQ_PNG => new("brown-papersq.png");
  public static HighlightJsTheme CODEPEN_EMBED => new("codepen-embed.css");
  public static HighlightJsTheme COLOR_BREWER => new("color-brewer.css");
  public static HighlightJsTheme DARK => new("dark.css");
  public static HighlightJsTheme DEFAULT => new("default.css");
  public static HighlightJsTheme DEVIBEANS => new("devibeans.css");
  public static HighlightJsTheme DOCCO => new("docco.css");
  public static HighlightJsTheme FAR => new("far.css");
  public static HighlightJsTheme FELIPEC => new("felipec.css");
  public static HighlightJsTheme FOUNDATION => new("foundation.css");
  public static HighlightJsTheme GITHUB_DARK_DIMMED => new("github-dark-dimmed.css");
  public static HighlightJsTheme GITHUB_DARK => new("github-dark.css");
  public static HighlightJsTheme GITHUB => new("github.css");
  public static HighlightJsTheme GML => new("gml.css");
  public static HighlightJsTheme GOOGLECODE => new("googlecode.css");
  public static HighlightJsTheme GRADIENT_DARK => new("gradient-dark.css");
  public static HighlightJsTheme GRADIENT_LIGHT => new("gradient-light.css");
  public static HighlightJsTheme GRAYSCALE => new("grayscale.css");
  public static HighlightJsTheme HYBRID => new("hybrid.css");
  public static HighlightJsTheme IDEA => new("idea.css");
  public static HighlightJsTheme INTELLIJ_LIGHT => new("intellij-light.css");
  public static HighlightJsTheme IR_BLACK => new("ir-black.css");
  public static HighlightJsTheme ISBL_EDITOR_DARK => new("isbl-editor-dark.css");
  public static HighlightJsTheme ISBL_EDITOR_LIGHT => new("isbl-editor-light.css");
  public static HighlightJsTheme KIMBIE_DARK => new("kimbie-dark.css");
  public static HighlightJsTheme KIMBIE_LIGHT => new("kimbie-light.css");
  public static HighlightJsTheme LIGHTFAIR => new("lightfair.css");
  public static HighlightJsTheme LIOSHI => new("lioshi.css");
  public static HighlightJsTheme MAGULA => new("magula.css");
  public static HighlightJsTheme MONO_BLUE => new("mono-blue.css");
  public static HighlightJsTheme MONOKAI_SUBLIME => new("monokai-sublime.css");
  public static HighlightJsTheme MONOKAI => new("monokai.css");
  public static HighlightJsTheme NIGHT_OWL => new("night-owl.css");
  public static HighlightJsTheme NNFx_DARK => new("nnfx-dark.css");
  public static HighlightJsTheme NNFX_LIGHT => new("nnfx-light.css");
  public static HighlightJsTheme NORD => new("nord.css");
  public static HighlightJsTheme OBSIDIAN => new("obsidian.css");
  public static HighlightJsTheme PANDA_SYNTAX_DARK => new("panda-syntax-dark.css");
  public static HighlightJsTheme PANDA_SYNTAX_LIGHT => new("panda-syntax-light.css");
  public static HighlightJsTheme PARAISO_DARK => new("paraiso-dark.css");
  public static HighlightJsTheme PARAISO_LIGHT => new("paraiso-light.css");
  public static HighlightJsTheme POJOAQUE => new("pojoaque.css");
  public static HighlightJsTheme PUREBASIC => new("purebasic.css");
  public static HighlightJsTheme QTCREATOR_DARK => new("qtcreator-dark.css");
  public static HighlightJsTheme QTCREATOR_LIGHT => new("qtcreator-light.css");
  public static HighlightJsTheme RAINBOW => new("rainbow.css");
  public static HighlightJsTheme ROUTEROS => new("routeros.css");
  public static HighlightJsTheme SCHOOL_BOOK => new("school-book.css");
  public static HighlightJsTheme SHADES_OF_PURPLE => new("shades-of-purple.css");
  public static HighlightJsTheme SRCERY => new("srcery.css");
  public static HighlightJsTheme STACKOVERFLOW_DARK => new("stackoverflow-dark.css");
  public static HighlightJsTheme STACKOVERFLOW_LIGHT => new("stackoverflow-light.css");
  public static HighlightJsTheme SUNBURST => new("sunburst.css");
  public static HighlightJsTheme TOKYO_NIGHT_DARK => new("tokyo-night-dark.css");
  public static HighlightJsTheme TOKYO_NIGHT_LIGHT => new("tokyo-night-light.css");
  public static HighlightJsTheme TOMORROW_NIGHT_BLUE => new("tomorrow-night-blue.css");
  public static HighlightJsTheme TOMORROW_NIGHT_BRIGHT => new("tomorrow-night-bright.css");
  public static HighlightJsTheme VS => new("vs.css");
  public static HighlightJsTheme VS2015 => new("vs2015.css");
  public static HighlightJsTheme XCODE => new("xcode.css");
  public static HighlightJsTheme XT256 => new("xt256.css");

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
