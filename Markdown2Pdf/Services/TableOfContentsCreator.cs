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
  private readonly string _openListElement;
  private readonly string _closeListElement;

  // Substract 1 to adjust to 0 based values
  private readonly int _minDepthLevel;
  private readonly int _maxDepthLevel;

  private readonly EmbeddedResourceService _embeddedResourceService;

  private Link[]? _links;
  private Dictionary<Link, int>? _linkPageMapping;

  private const string _OMIT_IN_TOC_IDENTIFIER = "<!-- omit from toc -->";
  private const string _HTML_CLASS_NAME = "table-of-contents";
  private const string _TOC_STYLE_KEY = "tocStyle";
  private const string _DECIMAL_STYLE_FILE_NAME = "TableOfContentsDecimalStyle.css";
  private const string _LIST_STYLE_NONE = ".table-of-contents ul { list-style: none; }";
  private static readonly string _nl = Environment.NewLine;

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
    this._openListElement = this._isOrdered ? "<ol>" : "<ul>";
    this._closeListElement = this._isOrdered ? "</ol>" : "</ul>";

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

  private void _ReadPageNumbers(object _, PdfArgs e) { // TODO: what if link not found
    if (this._links == null)
      throw new InvalidOperationException("Links have not been created yet.");

    e.NeedsRerun = true;

    using var pdf = PdfDocument.Open(e.PdfPath);
    this._linkPageMapping = _ParsePageNumbersFromPdf(pdf, this._links);
  }

  private static Dictionary<Link, int> _ParsePageNumbersFromPdf(PdfDocument pdf, IEnumerable<Link> links) {
    var linkPageNumbers = new Dictionary<Link, int>();
    var linksToFind = new List<Link>(links);

    foreach (var page in pdf.GetPages()) {
      var text = ContentOrderTextExtractor.GetText(page);
      var lines = _lineBreakRegex.Split(text);

      foreach (var line in lines)
        foreach (var link in linksToFind) {
          if (link.Title != line)
            continue;

          linkPageNumbers[link] = page.Number;
          linksToFind.Remove(link);
          if (linksToFind.Count() == 0)
            return linkPageNumbers; // All links found

          break; // Found link, continue with next line
        }
    }

    return linkPageNumbers;
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
    var links = this._links = _CreateLinks(markdownContent).ToArray();
    var minLinkDepth = links.Min(l => l.Depth);
    var minDepth = Math.Max(this._minDepthLevel, minLinkDepth); // ensure that there's no unneeded nesting

    var lastDepth = -1; // start at -1 to open the list on first element
    var tocBuilder = new StringBuilder();

    tocBuilder.Append($"<nav class=\"{_HTML_CLASS_NAME}\">");

    foreach (var link in links) {
      var fixedDepth = link.Depth - minDepth; // Start counting from minDepth
      if (fixedDepth < 0)
        continue;

      var htmlListTags = string.Empty;
      htmlListTags = fixedDepth switch {
        var depth when depth > lastDepth => this._CreateNestingTags(fixedDepth, lastDepth),
        var depth when depth == lastDepth => this._CreateSameDepthTags(),
        _ => this._CreatedDenestingTags(fixedDepth, lastDepth),
      };

      tocBuilder.Append(htmlListTags);
      lastDepth = fixedDepth;

      tocBuilder.Append(_CreateLinkText(link));
    }

    // close open tags
    for (var i = 0; i <= lastDepth; ++i)
      tocBuilder.Append(_nl + "</li>" + _nl + this._closeListElement);

    tocBuilder.Append(_nl + "</nav>");

    return tocBuilder.ToString();
  }

  private string _CreateNestingTags(int depth, int lastDepth) {
    var difference = depth - lastDepth;
    var html = string.Empty;

    // open nestings
    for (var i = 0; i < difference; ++i) {

      // only provide ListStyle for elements that actually have text
      var extraStyle = difference > 1 && i != difference - 1
        ? " style='list-style:none'"
        : string.Empty;

      html += _nl + this._openListElement + _nl + $"<li{extraStyle}>";
    }

    return html;
  }

  private string _CreateSameDepthTags() => "</li>" + _nl + "<li>";

  private string _CreatedDenestingTags(int depth, int lastDepth) {
    var difference = lastDepth - depth;
    var html = string.Empty;

    for (var i = 0; i < difference; ++i)
      html += _nl + "</li>" + _nl + this._closeListElement;
    
    return html + _nl + "<li>";
  }

  private int _lastPageNumber = -1;
  private string _CreateLinkText(Link link) {
    if (!this._options.HasPageNumbers)
      return link.ToHtml();

    if (this._linkPageMapping == null || !this._linkPageMapping.TryGetValue(link, out var pageNumber))
      return link.ToHtml(this._lastPageNumber);

    this._lastPageNumber = pageNumber;
    return link.ToHtml(pageNumber);
  }

  private string _InsertInto(string content, string tocHtml)
    => _insertionRegex.Replace(content, tocHtml);

}
