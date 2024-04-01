using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Services;

internal class TableOfContentsCreator {

  private readonly struct Link(string title, string linkAddress, int Depth) {
    public string Title { get; } = title;
    public string LinkAddress { get; } = linkAddress;
    public int Depth { get; } = Depth;

    public override readonly string ToString() => $"<a href=\"{this.LinkAddress}\">{this.Title}</a>";
  }

  private readonly ListStyle _listStyle;
  private readonly bool _isOrdered;

  // Substract 1 to adjust to 0 based values
  private readonly int _minDepthLevel;
  private readonly int _maxDepthLevel;

  private readonly EmbeddedResourceService _embeddedResourceService;

  private const string _OMIT_IN_TOC_IDENTIFIER = "<!-- omit from toc -->";
  private const string _HTML_CLASS_NAME = "table-of-contents";

  private const string _TOC_LIST_STYLE_KEY = "tocListStyle";
  private const string _TOC_DECIMAL_STYLE_FILE_NAME = "TableOfContentsDecimalStyle.css";
  private const string _TOC_LIST_STYLE_NONE = ".table-of-contents ul { list-style: none; }";

  private static readonly Regex _headerReg = new("^(?<hashes>#{1,6}) +(?<title>[^\r\n]*)",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);
  private static readonly Regex _emojiReg = new(":(\\w+):", RegexOptions.Compiled);
  private static readonly Regex _insertionRegex = new("""^(\[TOC]|\[\[_TOC_]]|<!-- toc -->)\r?$""",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);

  public TableOfContentsCreator(TableOfContentsOptions options, IConvertionEvents convertionEvents, EmbeddedResourceService embeddedResourceService) {
    this._listStyle = options.ListStyle;
    this._isOrdered = options.ListStyle == ListStyle.OrderedDefault
    || options.ListStyle == ListStyle.Decimal;
    this._minDepthLevel = options.MinDepthLevel - 1;
    this._maxDepthLevel = options.MaxDepthLevel - 1;
    this._embeddedResourceService = embeddedResourceService;

    convertionEvents.BeforeMarkdownConversion += this._AddToMarkdown;
    convertionEvents.OnTemplateModelCreating += this._AddListStylesToTemplate;
  }

  private void _AddToMarkdown(object sender, MarkdownArgs e) {
    var tocHtml = this._ToHtml(e.MarkdownContent);
    e.MarkdownContent = this._InsertInto(e.MarkdownContent, tocHtml);
  }

  private void _AddListStylesToTemplate(object _, TemplateModelArgs e) {
    var tableOfContentsDecimalStyle = this._listStyle switch {
      ListStyle.None => _TOC_LIST_STYLE_NONE,
      ListStyle.Decimal => this._embeddedResourceService.GetResourceContent(_TOC_DECIMAL_STYLE_FILE_NAME),
      _ => string.Empty,
    };
    e.TemplateModel.Add(_TOC_LIST_STYLE_KEY, tableOfContentsDecimalStyle);
  }

  private IEnumerable<Link> _CreateLinks(string markdownContent) {
    var matches = _headerReg.Matches(markdownContent);
    var links = new List<Link>(matches.Count);

    foreach (Match match in matches) {
      var depth = match.Groups["hashes"].Value.Length - 1;
      var title = match.Groups["title"].Value;

      if (depth < this._minDepthLevel
        || depth > this._maxDepthLevel
        || title.ToLower().EndsWith(_OMIT_IN_TOC_IDENTIFIER))
        continue;

      // build link
      title = _htmlElementReg.Replace(title, string.Empty);
      title = _emojiReg.Replace(title, string.Empty).Trim();

      var linkAddress = LinkHelper.Urilize(title, true);
      linkAddress = "#" + linkAddress.ToLower();

      links.Add(new Link(title, linkAddress, depth));
    }

    return links;
  }

  private string _ToHtml(string markdownContent) {
    var NL = Environment.NewLine;
    var links = _CreateLinks(markdownContent);
    var minLinkDepth = links.Min(l => l.Depth);
    var minDepth = Math.Max(this._minDepthLevel, minLinkDepth); // ensure that there's no unneeded nesting

    var lastDepth = -1; // start at -1 to open the list on first element

    var openList = this._isOrdered ? "<ol>" : "<ul>";
    var closeList = this._isOrdered ? "</ol>" : "</ul>";
    var tocBuilder = new StringBuilder();

    tocBuilder.Append($"<nav class=\"{_HTML_CLASS_NAME}\">");

    foreach (var link in links) {
      var fixedLinkDepth = link.Depth - minDepth; // Reduce nesting by minDepth
      if (fixedLinkDepth < 0)
        continue;

      switch (fixedLinkDepth) {
        case var depth when depth > lastDepth: // nested element
          var difference = fixedLinkDepth - lastDepth;

          // open nestings
          for (var i = 0; i < difference; ++i) {

            // only provide ListStyle for elements that actually have text
            var extraStyle = difference > 1 && i != difference - 1
              ? " style='list-style:none'"
              : string.Empty;

            tocBuilder.Append(NL + openList + NL + $"<li{extraStyle}>");
          }
          break;

        case var depth when depth == lastDepth: // same height
          // close previous element
          tocBuilder.AppendLine("</li>");
          tocBuilder.Append("<li>");
          break;

        default: // depth < lastDepth
          difference = lastDepth - fixedLinkDepth;

          // close previous elements
          for (var i = 0; i < difference; ++i)
            tocBuilder.Append(NL + "</li>" + NL + closeList);

          tocBuilder.Append(NL + "<li>");
          break;
      }

      lastDepth = fixedLinkDepth;
      tocBuilder.Append(link);
    }

    for (var i = 0; i <= lastDepth; ++i)
      tocBuilder.Append(NL + "</li>" + NL + closeList);

    tocBuilder.Append(NL + "</nav>");

    return tocBuilder.ToString();
  }

  private string _InsertInto(string content, string tocHtml)
    => _insertionRegex.Replace(content, tocHtml);
}
