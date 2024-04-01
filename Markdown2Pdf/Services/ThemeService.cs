using System.Collections.Generic;
using Markdown2Pdf.Models;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Services;

internal class ThemeService {

  private const string _STYLE_KEY = "stylePath";

  private readonly IReadOnlyDictionary<ThemeType, ModuleInformation> _themeSourceMapping = new Dictionary<ThemeType, ModuleInformation>() {
    {ThemeType.Github, new("https://cdnjs.cloudflare.com/ajax/libs/github-markdown-css/5.5.1/github-markdown-light.min.css", "github-markdown-css/github-markdown-light.css") },
    {ThemeType.Latex, new("https://latex.now.sh/style.css", "latex.css/style.min.css") }
  };

  private readonly Theme _theme;
  private readonly ModuleOptions _options;

  public ThemeService(Theme theme, ModuleOptions options, IConvertionEvents events) {
    this._theme = theme;
    this._options = options;

    // adjust local dictionary paths
    if (options is NodeModuleOptions nodeModuleOptions) {
      var path = nodeModuleOptions.ModulePath;

      this._themeSourceMapping = ModuleInformation.UpdateDic(this._themeSourceMapping, path);
    }

    events.OnTemplateModelCreating += this._AddThemeToTemplate;
  }

  private void _AddThemeToTemplate(object sender, TemplateModelArgs e) {
    switch (this._theme) {
      case PredefinedTheme predefinedTheme when predefinedTheme.Type != ThemeType.None: {
        var value = this._themeSourceMapping[predefinedTheme.Type];
        e.TemplateModel.Add(_STYLE_KEY, this._options.IsRemote ? value.RemotePath : value.NodePath);
        break;
      }

      case CustomTheme customTheme:
        e.TemplateModel.Add(_STYLE_KEY, customTheme.CssPath);
        break;
    }
  }
}
