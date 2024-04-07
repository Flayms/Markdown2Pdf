using System;
using System.Collections.Generic;

namespace Markdown2Pdf;
internal interface IConvertionEvents {

  internal event EventHandler<MarkdownArgs> BeforeMarkdownConversion;
  internal event EventHandler<TemplateModelArgs> OnTemplateModelCreating;
  internal event EventHandler<PdfArgs> OnPdfCreatedEvent;
}

internal class MarkdownArgs(ref string markdownContent) : EventArgs {
  public string MarkdownContent { get; set; } = markdownContent;
}

internal class TemplateModelArgs(Dictionary<string, string> templateModel) : EventArgs {
  public IDictionary<string, string> TemplateModel { get; } = templateModel;
}

internal class PdfArgs(string pdfPath) : EventArgs {
  public string PdfPath { get; set; } = pdfPath;
  public bool NeedsRerun { get; set; }
}
