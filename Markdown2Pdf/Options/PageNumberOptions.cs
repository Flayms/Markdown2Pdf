namespace Markdown2Pdf.Options;

/// <summary>
/// Options for the page numbers in the Table of Contents.
/// </summary>
public class PageNumberOptions {

  /// <inheritdoc cref="Leader"/>
  /// <value>Default: <see cref="Leader.Dots"/>.</value>
  public Leader TabLeader { get; set; } = Leader.Dots;
}
