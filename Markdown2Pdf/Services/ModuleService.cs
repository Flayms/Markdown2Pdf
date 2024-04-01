using Markdown2Pdf.Models;
using Markdown2Pdf.Options;
using System;
using System.Collections.Generic;

namespace Markdown2Pdf.Services;
internal class ModuleService {


  private readonly IReadOnlyDictionary<string, ModuleInformation> _packagelocationMapping = new Dictionary<string, ModuleInformation>() {
    {"mathjax", new ("https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js", "mathjax/es5/tex-mml-chtml.js") },
    {"mermaid", new ("https://cdn.jsdelivr.net/npm/mermaid@10/dist/mermaid.min.js", "mermaid/dist/mermaid.min.js") },
    {"highlightjs", new ("https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js", "@highlightjs/cdn-assets/highlight.min.js") },
    {"highlightjs_style", new ("https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles", "@highlightjs/cdn-assets/styles") },
    {"fontawesome", new ("https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css", "font-awesome/css/font-awesome.min.css") }
  };
  private readonly ModuleOptions _options;

  public ModuleService(ModuleOptions options, IConvertionEvents events) {
    this._options = options;

    // adjust local dictionary paths
    if (options is NodeModuleOptions nodeModuleOptions) {
      var path = nodeModuleOptions.ModulePath;

      this._packagelocationMapping = ModuleInformation.UpdateDic(this._packagelocationMapping, path);
    }

    events.OnTemplateModelCreating += this._AddModulesToTemplate;
  }

  private void _AddModulesToTemplate(object sender, TemplateModelArgs e) {
    // load correct module paths
    foreach (var kvp in this._packagelocationMapping)
      e.TemplateModel.Add(kvp.Key, this._options.IsRemote ? kvp.Value.RemotePath : kvp.Value.NodePath);
  }

}
