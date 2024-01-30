using System;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;

namespace Markdown2Pdf.Options;

/// <inheritdoc cref="TableOfContents(bool, int)"/>
public class TableOfContents {

  private readonly int _maxIndentation;
  private readonly bool _isOrdered;

  private const string _IDENTIFIER = "<!--TOC-->";
  private static readonly Regex _headerReg = new("^(?<depth>#{1,6}) +(?<title>.*)$",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);

  /// <summary>
  /// Inserts a Table of Contents into the generated PDF. 
  /// The TOC will be inserted into all <!--TOC--> comments within the markdown document. 
  /// </summary>
  /// <param name="isOrdered"></param>
  /// <param name="maxIndentation"></param>
  public TableOfContents(bool isOrdered = true, int maxIndentation = 3) {
    if (maxIndentation is < 1 or > 6)
      throw new ArgumentOutOfRangeException();

    this._isOrdered = isOrdered;
    this._maxIndentation = maxIndentation;
  }

  internal void InsertInto(ref string markdownContent) {
    var matches = _headerReg.Matches(markdownContent);
    var tocBuilder = new StringBuilder();
    var delimiter = this._isOrdered ? "1. " : "* ";

    foreach (Match match in matches) {
      var depth = match.Groups["depth"].Value.Length;

      if (depth > this._maxIndentation)
        continue;

      // build link
      var title = match.Groups["title"].Value;
      title = _htmlElementReg.Replace(title, string.Empty);

      var linkAddress = LinkHelper.Urilize(title, true);
      linkAddress = "#" + linkAddress.ToLower();

      var link = $"[{title}]({linkAddress})";

      _ = tocBuilder.Append(new string(' ', (depth - 1) * 4)); // indent 4 spaces per level
      _ = tocBuilder.Append(delimiter);
      _ = tocBuilder.AppendLine(link);
    }
    markdownContent = markdownContent.Replace(_IDENTIFIER, tocBuilder.ToString());
  }

}
