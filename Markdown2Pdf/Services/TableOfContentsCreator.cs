using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Markdig.Helpers;
using Markdown2Pdf.Options;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;

namespace Markdown2Pdf.Services;

internal class TableOfContentsCreator {

  private readonly struct Link(string title, string linkAddress, int Depth) {
    public string Title { get; } = title;
    public string LinkAddress { get; } = linkAddress;
    public int Depth { get; } = Depth;

    public readonly string ToHtml() => $"<a href=\"{this.LinkAddress}\">{this.Title}</a>";
    public readonly string ToHtml(int pageNumber) => $"<a href=\"{this.LinkAddress}\"><span>{this.Title}</span><span>{pageNumber}</span></a>";
  }

  private readonly TableOfContentsOptions _options;
  private readonly bool _isOrdered;

  // Substract 1 to adjust to 0 based values
  private readonly int _minDepthLevel;
  private readonly int _maxDepthLevel;

  private readonly EmbeddedResourceService _embeddedResourceService;

  private Link[] _links;
  private Dictionary<Link, int>? _pageNumbers;

  private const string _OMIT_IN_TOC_IDENTIFIER = "<!-- omit from toc -->";
  private const string _HTML_CLASS_NAME = "table-of-contents";

  private const string _TOC_STYLE_KEY = "tocStyle";
  private const string _DECIMAL_STYLE_FILE_NAME = "TableOfContentsDecimalStyle.css";
  private const string _LIST_STYLE_NONE = ".table-of-contents ul { list-style: none; }";

  private static readonly Regex _headerReg = new("^(?<hashes>#{1,6}) +(?<title>[^\r\n]*)",
    RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);
  private static readonly Regex _emojiReg = new(":(\\w+):", RegexOptions.Compiled);
  private static readonly Regex _insertionRegex = new("""^(\[TOC]|\[\[_TOC_]]|<!-- toc -->)\r?$""",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline);
  private static readonly Regex _lineBreakRegex = new("\r\n?|\n", RegexOptions.Compiled);

  public TableOfContentsCreator(TableOfContentsOptions options, IConvertionEvents convertionEvents, EmbeddedResourceService embeddedResourceService) {
    this._options = options;
    this._isOrdered = options.ListStyle == ListStyle.OrderedDefault
      || options.ListStyle == ListStyle.Decimal;
    this._minDepthLevel = options.MinDepthLevel - 1;
    this._maxDepthLevel = options.MaxDepthLevel - 1;
    this._embeddedResourceService = embeddedResourceService;

    convertionEvents.BeforeMarkdownConversion += this._AddToMarkdown;
    convertionEvents.OnTemplateModelCreating += this._AddStylesToTemplate;

    if (options.HasPageNumbers)
      convertionEvents.OnPdfCreatedEvent += this._ReadPageNumbers;
  }

  private void _AddToMarkdown(object sender, MarkdownArgs e) {
    var tocHtml = this._ToHtml(e.MarkdownContent);
    e.MarkdownContent = this._InsertInto(e.MarkdownContent, tocHtml);
  }

  private void _AddStylesToTemplate(object _, TemplateModelArgs e) {
    var tableOfContentsDecimalStyle = this._options.ListStyle switch {
      ListStyle.None => _LIST_STYLE_NONE,
      ListStyle.Decimal => this._embeddedResourceService.GetResourceContent(_DECIMAL_STYLE_FILE_NAME),
      _ => string.Empty,
    };

    if (this._options.HasColoredLinks)
      tableOfContentsDecimalStyle += Environment.NewLine + ".table-of-contents a { all: unset; }";

    if (this._options.HasPageNumbers)
      tableOfContentsDecimalStyle += Environment.NewLine + ".table-of-contents a { display: flex; justify-content: space-between; }";

    e.TemplateModel.Add(_TOC_STYLE_KEY, tableOfContentsDecimalStyle);
  }

  private void _ReadPageNumbers(object _, PdfArgs e) { //todo: what if link not found
    e.NeedsRerun = true;

    using (var pdf = PdfDocument.Open(e.PdfPath)) {
      var structure = pdf.Structure;
      var resut = pdf.TryGetBookmarks(out var bookmarks);
      var pageNumbers = new Dictionary<Link, int>();

      foreach (var page in pdf.GetPages()) {
        var text = ContentOrderTextExtractor.GetText(page);
        var lines = _lineBreakRegex.Split(text);

        //todo: optimize
        foreach (var line in lines) {
          foreach (var link in this._links) {
            if (link.Title == line)
              pageNumbers[link] = page.Number;
          }
        }
      }

      this._pageNumbers = pageNumbers;
    }
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
    var links = this._links = _CreateLinks(markdownContent).ToArray();
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

      //todo: optimize
      string linkText;
      if (this._options.HasPageNumbers) {
        if (this._pageNumbers != null && this._pageNumbers.TryGetValue(link, out var pageNumber)) {
          linkText = link.ToHtml(pageNumber);
        } else
          linkText = link.ToHtml(-1); //todo: placeholder
      } else
        linkText = link.ToHtml();

      tocBuilder.Append(linkText);
    }

    for (var i = 0; i <= lastDepth; ++i)
      tocBuilder.Append(NL + "</li>" + NL + closeList);

    tocBuilder.Append(NL + "</nav>");

    return tocBuilder.ToString();
  }

  private string _InsertInto(string content, string tocHtml)
    => _insertionRegex.Replace(content, tocHtml);

}
