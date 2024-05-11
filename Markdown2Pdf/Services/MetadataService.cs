using Markdown2Pdf.Options;

namespace Markdown2Pdf.Services;
internal class MetadataService {
  private readonly Markdown2PdfOptions _options;
  private readonly IConvertionEvents _events;

  public MetadataService(Markdown2PdfOptions options, IConvertionEvents events) {
    events.OnTemplateModelCreating += this._AddTitleToTemplate;
    this._options = options;
    this._events = events;
  }

  private void _AddTitleToTemplate(object _, TemplateModelArgs e) {
    var title = this._options.MetadataTitle ?? this._options.DocumentTitle ?? _events.OutputFileName!;
    e.TemplateModel.Add("title", title);
  }

}
