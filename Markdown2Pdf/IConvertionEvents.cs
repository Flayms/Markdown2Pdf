using System;
using System.Collections.Generic;

namespace Markdown2Pdf;
internal interface IConvertionEvents {

  internal event EventHandler<TemplateModelArgs>? OnTemplateModelCreating;
}

internal class TemplateModelArgs(Dictionary<string, string> templateModel) : EventArgs {
  public IDictionary<string, string> TemplateModel { get; } = templateModel;
}
