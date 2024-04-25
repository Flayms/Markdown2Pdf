using System.ComponentModel;

namespace Markdown2Pdf.Options;

/// <summary>
/// The character to use to lead from the TOC title to the page number.
/// </summary>
public enum Leader {
  ///<summary>
  /// No leader.
  /// <example>
  /// <code language="text">
  /// My Heading           1
  /// </code>
  /// </example>
  /// </summary>
  [Description("")]
  None,

  /// <summary>
  /// Use dots for the leader.
  /// <example>
  /// <code language="text">
  /// My Heading ......... 1
  /// </code>
  /// </example>
  /// </summary>
  [Description("dots")]
  Dots,

  /// <summary>
  /// Use an underline for the leader.
  /// <example>
  /// <code language="text">
  /// My Heading _________ 1
  /// </code>
  /// </example>
  /// </summary>
  [Description("underline")]
  Underline,

  /// <summary>
  /// Use dashes for the leader.
  /// <example>
  /// <code language="text">
  /// My Heading --------- 1
  /// </code>
  /// </example>
  /// </summary>
  [Description("dashes")]
  Dashes,
}
