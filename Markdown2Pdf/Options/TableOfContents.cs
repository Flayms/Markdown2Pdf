using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Markdown2Pdf.Options;
public class TableOfContents(ListType listType) {
  private readonly ListType _listType = listType;
  private const string _IDENTIFIER = "<!--TOC-->";
  private static readonly Regex _headerReg = new("^(?<depth>#{1,6}) +(?<title>.*)$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
  private static readonly Regex _htmlElementReg = new("<[^>]*>[^>]*</[^>]*>|<[^>]*/>", RegexOptions.Compiled);
  private static readonly Regex _punctutationReg = new("[\\p{P} ]", RegexOptions.Compiled);
  private static readonly Regex _hyphenReg = new("-+", RegexOptions.Compiled);

  public static TableOfContents BulletList => new(ListType.Bullet);
  // TODO: public static TableOfContents NumberList => new(ListType.Number);

  // TODO: tests
  internal void InsertInto(ref string markdownContent) {
    var matches = _headerReg.Matches(markdownContent);
    var tocBuilder = new StringBuilder();

    foreach (Match match in matches) {
      var depth = match.Groups["depth"].Value.Length - 1; //subtract 1 because level 1 is at 0 depth
      var title = match.Groups["title"].Value;

      // convert title to link
      title = _htmlElementReg.Replace(title, string.Empty); // remove html
      var linkAddress = _punctutationReg.Replace(title, "-").Replace(' ', '-'); // replace special chars by hyphens
      linkAddress = _hyphenReg.Replace(linkAddress, "-"); // only allow single hyphens
      linkAddress = "#" + linkAddress.ToLower();
      var link = $"[{title}]({linkAddress})";

      tocBuilder.Append(new string(' ', depth * 2));
      tocBuilder.Append("* ");
      tocBuilder.AppendLine(link);
    }
    markdownContent = markdownContent.Replace(_IDENTIFIER, tocBuilder.ToString());
  }

}

public enum ListType {
  Bullet,
  Number,
}
