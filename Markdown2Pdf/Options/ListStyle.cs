namespace Markdown2Pdf.Options;
/// <summary>
/// Decides which characters to use before the TOC items.
/// </summary>
public enum ListStyle {

  /// <summary>
  /// Just display the TOC items without any preceeding characters.
  /// </summary>
  None,

  /// <summary>
  /// Use the current themes default list-style for ordered lists.
  /// </summary>
  OrderedDefault,

  /// <summary>
  /// Use the current themes default list-style for unordered lists.
  /// </summary>
  Unordered,

  /// <summary>
  /// Preceed the TOC items with numbers separated by points (e.g. <c>1.1</c>, <c>1.2</c>, <c>1.2.1</c>...).
  /// </summary>
  Decimal
}
