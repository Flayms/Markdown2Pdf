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

  private class Link(string title, string linkAddress, int Depth) {
    public string Title { get; } = title;
    public string LinkAddress { get; } = linkAddress;
    public int Depth { get; } = Depth;

    public string ToHtml() => $"<a href=\"{this.LinkAddress}\">{this.Title}</a>";
    public string ToHtml(int pageNumber) => $"" +
      $"<a href=\"{this.LinkAddress}\">" +
      $"<span class=\"title\">{this.Title}</span>" +
      $"<span class=\"page-number\">{pageNumber}</span>" +
      $"</a>";
  }

  private class LinkWithPageNumber(Link link, int pageNumber)
    : Link(link.Title, link.LinkAddress, link.Depth) {
    public int PageNumber { get; } = pageNumber;
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
  private LinkWithPageNumber[]? _linkPages;

  private const string _OMIT_IN_TOC_IDENTIFIER = "<!-- omit from toc -->";
  private const string _HTML_CLASS_NAME = "table-of-contents";
  private const string _TOC_STYLE_KEY = "tocStyle";
  private const string _DECIMAL_STYLE_FILE_NAME = "TableOfContentsDecimalStyle.css";
  private const string _PAGE_NUMBER_STYLE_FILE_NAME = "TableOfContentsPageNumberStyle.css";
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
      tableOfContentsDecimalStyle += Environment.NewLine + this._embeddedResourceService.GetResourceContent(_PAGE_NUMBER_STYLE_FILE_NAME);

    e.TemplateModel.Add(_TOC_STYLE_KEY, tableOfContentsDecimalStyle);
  }

  private void _ReadPageNumbers(object _, PdfArgs e) { // TODO: what if link not found
    if (this._links == null)
      throw new InvalidOperationException("Links have not been created yet.");

    e.NeedsRerun = true;

    using var pdf = PdfDocument.Open(e.PdfPath);
    this._linkPages = _ParsePageNumbersFromPdf(pdf, this._links).ToArray();

    // Fill in values that could not be found
    var length = this._links.Length;
    for (var i = 0; i < length; ++i) {
      if (this._linkPages[i] != null)
        continue;

      this._linkPages[i] = i == 0
        ? new(this._links[i], 1) // Assume first page
        : new(this._links[i], this._linkPages[i - 1].PageNumber); // Assume same as previous
    }
  }

  private static IEnumerable<LinkWithPageNumber> _ParsePageNumbersFromPdf(PdfDocument pdf, Link[] links) {
    var linkPages = new LinkWithPageNumber[links.Count()];
    var linksToFind = links.ToList();

    foreach (var page in pdf.GetPages()) {
      var text = ContentOrderTextExtractor.GetText(page);
      var lines = _lineBreakRegex.Split(text);

      foreach (var line in lines)
        foreach (var link in linksToFind) {
          if (link.Title != line)
            continue;

          linkPages[Array.IndexOf(links, link)] = new(link, page.Number);
          linksToFind.Remove(link);
          if (linksToFind.Count() == 0)
            return linkPages; // All links found

          break; // Found link, continue with next line
        }
    }

    return linkPages;
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

  private string _CreateLinkText(Link link) {
    if (!this._options.HasPageNumbers)
      return link.ToHtml();

    if (this._linkPages == null)
      return link.ToHtml(-1); // Placeholder

    var pageNumber = this._linkPages.First(l => l.LinkAddress == link.LinkAddress).PageNumber;
    return link.ToHtml(pageNumber);
  }

  private string _InsertInto(string content, string tocHtml)
    => _insertionRegex.Replace(content, tocHtml);

}
