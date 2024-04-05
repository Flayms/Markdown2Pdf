namespace Markdown2Pdf.Options;

/// <summary>
/// The character to use to lead from the TOC title to the page number.
/// </summary>
public enum Leader {
  ///<summary>
  /// No leader.
  /// </summary>
  None,
  /// <summary>
  /// Use dots for the leader.
  /// </summary>
  Dot,
  /// <summary>
  /// Use underscores for the leader.
  /// </summary>
  Underscore
}
