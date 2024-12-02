using System;
using System.Collections.Generic;

namespace Markdown2Pdf;

/// <summary>
/// Interface for events that occur during the conversion process.
/// </summary>
public interface IConvertionEvents {

  /// <summary>
  /// Name of the output file.
  /// </summary>
  public string? OutputFileName { get; }

  /// <summary>
  /// Gets invoked before the markdown to HTML conversion.
  /// </summary>
  public event EventHandler<MarkdownArgs> BeforeHtmlConversion;

  /// <summary>
  /// Gets invoked when the template model is created.
  /// </summary>
  /// <remarks>
  /// This can be used to add custom content to the html template.
  /// </remarks>
  public event EventHandler<TemplateModelArgs> OnTemplateModelCreating;

  /// <summary>
  /// Gets invoked after a temporary PDF file is created.
  /// </summary>
  /// <remarks>
  /// This only happens if a parsing of the generated PDF is needed, e.g. for generating page numbers.
  /// </remarks>
  public event EventHandler<PdfArgs> OnTempPdfCreatedEvent;
}

/// <summary>
/// <see cref="EventArgs"/> containing the markdown content before the HTML conversion.
/// </summary>
/// <param name="markdownContent">The current markdown content.</param>
public class MarkdownArgs(string markdownContent) : EventArgs {

  /// <summary>
  /// The current markdown content, available to be modified.
  /// </summary>
  public string MarkdownContent { get; set; } = markdownContent;
}

/// <summary>
/// <see cref="EventArgs"/> containing the model for the HTML template.
/// </summary>
/// <param name="templateModel">The model for the HMTml template.</param>
public class TemplateModelArgs(Dictionary<string, string> templateModel) : EventArgs {

  /// <summary>
  /// The model for the HTML template.
  /// </summary>
  public IDictionary<string, string> TemplateModel { get; } = templateModel;
}

/// <summary>
/// <see cref="EventArgs"/> containing the path to the temporary PDF file.
/// </summary>
/// <param name="pdfPath">Path to the temporary PDF.</param>
public class PdfArgs(string pdfPath) : EventArgs {

  /// <summary>
  /// Path to the temporary PDF.
  /// </summary>
  public string PdfPath { get; } = pdfPath;
}
