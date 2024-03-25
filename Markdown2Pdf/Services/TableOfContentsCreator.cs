using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Services;

internal class TableOfContentsCreator(TableOfContentsOptions options) {

  private readonly struct Link(string title, string linkAddress, int Depth) {
    public string Title { get; } = title;
    public string LinkAddress { get; } = linkAddress;
    public int Depth { get; } = Depth;

    public override readonly string ToString() => $"<a href=\"{this.LinkAddress}\">{this.Title}</a>";
  }

  private readonly bool _isOrdered = options.IsOrdered;
  private readonly int _minDepthLevel = options.MinDepthLevel;
  private readonly int _maxDepthLevel = options.MaxDepthLevel;

  private const string _IDENTIFIER = "<!--TOC-->";
  private const string _HTML_CLASS_NAME = "table-of-contents";
  private static readonly Regex _headerReg = new("^(?<hashes>#{1,6}) +(?<title>[^\r\n]*)",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);
  private static readonly Regex _emojiReg = new(":(\\w+):", RegexOptions.Compiled);

  private IEnumerable<Link> _CreateLinks(string markdownContent) {
    var matches = _headerReg.Matches(markdownContent);
    var links = new List<Link>(matches.Count);

    foreach (Match match in matches) {
      var depth = match.Groups["hashes"].Value.Length - 1;

      if (depth < this._minDepthLevel || depth > this._maxDepthLevel)
        continue;

      // build link
      var title = match.Groups["title"].Value;
      title = _htmlElementReg.Replace(title, string.Empty);
      title = _emojiReg.Replace(title, string.Empty).Trim();

      var linkAddress = LinkHelper.Urilize(title, true);
      linkAddress = "#" + linkAddress.ToLower();

      links.Add(new Link(title, linkAddress, depth));
    }

    return links;
  }

  internal string ToHtml(string markdownContent) {
    var NL = Environment.NewLine;
    var links = _CreateLinks(markdownContent);
    var lastDepth = -1; // start at -1 to open the list on first element
    var openList = this._isOrdered ? "<ol>" : "<ul>";
    var closeList = this._isOrdered ? "</ol>" : "</ul>";
    var tocBuilder = new StringBuilder();

    tocBuilder.Append($"<nav class=\"{_HTML_CLASS_NAME}\">");

    foreach (var link in links) {

      switch (link.Depth) {
        case var depth when depth > lastDepth: // nested element
          var difference = link.Depth - lastDepth;

          // open nestings
          for (var i = 0; i < difference; ++i)
            tocBuilder.Append(NL + openList + NL + "<li>");
          break;

        case var depth when depth == lastDepth: // same height
          // close previous element
          tocBuilder.AppendLine("</li>");
          tocBuilder.Append("<li>");
          break;

        default: // depth < lastDepth
          difference = lastDepth - link.Depth;

          // close previous elements
          for (var i = 0; i < difference; ++i)
            tocBuilder.Append(NL + "</li>" + NL + closeList);

          tocBuilder.Append(NL + "<li>");
          break;
      }

      lastDepth = link.Depth;
      tocBuilder.Append(link);
    }

    for (var i = 0; i <= lastDepth; ++i)
      tocBuilder.Append(NL + "</li>" + NL + closeList);

    tocBuilder.Append(NL + "</nav>");

    return tocBuilder.ToString();
  }

  internal void InsertInto(ref string htmlContent, string tocHtml)
    => htmlContent = htmlContent.Replace(_IDENTIFIER, tocHtml);

}
