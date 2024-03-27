using System;

namespace Markdown2Pdf.Options;

/// <summary>
/// Options to create a Table of Contents for the PDF, generated from all headers. 
/// The TOC will be inserted into all <c>&lt;!--TOC--&gt;</c> comments within the markdown document. 
/// </summary>
public class TableOfContentsOptions {

  private const int _MIN_DEPTH_LEVEL = 1;
  private const int _MAX_DEPTH_LEVEL = 6;

  /// <summary>
  /// If <see langword="true"/>, will generate an Ordered List, otherwise an Unordered List.
  /// </summary>
  public bool IsOrdered { get; set; } = true;


  private int? _minDepthLevel;

  /// <summary>
  /// The minimum level of heading depth to include in the TOC
  /// (e.g. <c>1</c> will only include headings greater than or equal to <c>&lt;h1&gt;</c>).
  /// Range: <c>1</c> to <c>6</c>.
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

}
