using System;

namespace Markdown2Pdf.Options;

/// <summary>
/// Options to create a Table of Contents for the PDF, generated from all headers. 
/// The TOC will be inserted into all <c>[TOC]</c>, <c>[[_TOC_]]</c> or <c>&lt;!-- toc --&gt;</c> comments within the markdown document.<br/>
/// <example>
/// Example Markdown document with a TOC:
/// <code language="markdown">
/// # My Document
/// [TOC]
/// ...
/// </code>
/// </example>
/// <example>
/// </example>
/// Example creating a TOC:
/// <code>
/// options.TableOfContents = new TableOfContentsOptions {
/// ListStyle = ListStyle.Decimal,
/// 
/// // Include all heading levels from 2 to 4.
/// MinDepthLevel = 2,
/// MaxDepthLevel = 4
/// };
/// </code>
/// <example>
/// A header can be omitted from the toc by ending it with <c>&#60;!-- omit from toc --&#62;</c>:
/// <code language="markdown">
/// ## This header won't be displayed in the TOC &#60;!-- omit from toc --&#62;
/// </code>
/// </example>
/// </summary>
/// <remarks>
/// The TOC gets generated within a <c>&#60;nav class="table-of-contents"&#62;</c>. This can be used to apply extra custom styles.
/// </remarks>
public class TableOfContentsOptions {

  private const int _MIN_DEPTH_LEVEL = 1;
  private const int _MAX_DEPTH_LEVEL = 6;

  /// <inheritdoc cref="Options.ListStyle"/>
  public ListStyle ListStyle { get; set; } = ListStyle.OrderedDefault;

  /// <summary>
  /// Determines if the TOC links should have the default link color (instead of looking like normal text).
  /// <value>Default: <see langword="false"/>.</value>
  /// </summary>
  public bool HasColoredLinks { get; set; }

  private int? _minDepthLevel;

  /// <summary>
  /// The minimum level of heading depth to include in the TOC
  /// (e.g. <c>1</c> will only include headings greater than or equal to <c>&lt;h1&gt;</c>).
  /// Range: <c>1</c> to <c>6</c>.
  /// <value>Default: <c>1</c>.</value>
  /// </summary>
  public int MinDepthLevel {
    get => this._minDepthLevel ?? _MIN_DEPTH_LEVEL;
    set {
      if (value is < _MIN_DEPTH_LEVEL or > _MAX_DEPTH_LEVEL)
        throw new ArgumentOutOfRangeException($"Value must be between {_MIN_DEPTH_LEVEL} and {_MAX_DEPTH_LEVEL}.");

      if (this._maxDepthLevel != null && value > this._maxDepthLevel)
        throw new ArgumentOutOfRangeException($"{nameof(this.MinDepthLevel)} cannot be greater than {nameof(this.MaxDepthLevel)}");

      this._minDepthLevel = value;
    }
  }

  private int? _maxDepthLevel;

  /// <summary>
  /// The maximum level of heading depth to include in the TOC 
  /// (e.g. <c>3</c> will include headings less than or equal to <c>&lt;h3&gt;</c>).
  /// Range: <c>1</c> to <c>6</c>.
  /// <value>Default: <c>6</c>.</value>
  /// </summary>
  public int MaxDepthLevel {
    get => this._maxDepthLevel ?? _MAX_DEPTH_LEVEL;
    set {
      if (value is < _MIN_DEPTH_LEVEL or > _MAX_DEPTH_LEVEL)
        throw new ArgumentOutOfRangeException($"Value must be between {_MIN_DEPTH_LEVEL} and {_MAX_DEPTH_LEVEL}.");

      if (this._minDepthLevel != null && value < this._minDepthLevel)
        throw new ArgumentOutOfRangeException($"{nameof(this.MaxDepthLevel)} cannot be less than {nameof(this.MinDepthLevel)}");

      this._maxDepthLevel = value;
    }
  }

  /// <summary>
  /// If set, the TOC will be generated with page numbers.
  /// <value>Default: <see langword="null"/>.</value>
  /// </summary>
  /// <remarks>
  /// This causes the PDF to be generated twice to calculate the page numbers.
  /// </remarks>
  public PageNumberOptions? PageNumberOptions { get; set; }

}
