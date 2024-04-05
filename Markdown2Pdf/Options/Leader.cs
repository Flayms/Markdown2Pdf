using System.ComponentModel;

namespace Markdown2Pdf.Options;

/// <summary>
/// The character to use to lead from the TOC title to the page number.
/// </summary>
public enum Leader {
  ///<summary>
  /// No leader.
  /// </summary>
  [Description("")]
  None,

  /// <summary>
  /// Use dots for the leader.
  /// </summary>
  [Description("dots")]
  Dots,

  /// <summary>
  /// Use an underline for the leader.
  /// </summary>
  [Description("underline")]
  Underline,

  /// <summary>
  /// Use dashes for the leader.
  /// </summary>
  [Description("dashes")]
  Dashes,
}
