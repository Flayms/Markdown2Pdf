using System.IO;
using System.Text;

namespace Markdown2Pdf.Services;
internal class InlineOptionsParser {

  public static bool TryParseYamlFrontMatter(string markdownFilePath) {
    if (!_TryReadYamlFrontMatter(markdownFilePath, out var markdownContent)) {
      return false;
    }

    return true;
  }

  private static bool _TryReadYamlFrontMatter(string markdownFilePath, out string markdownContent) {
    using var reader = File.OpenText(markdownFilePath);

    var firstLine = reader.ReadLine();
    if (firstLine != "---") { // Start found
      markdownContent = null!;
      return false;
    }
    var sb = new StringBuilder();

    string line;
    while ((line = reader.ReadLine()) != null) {
      switch (line) {
        case "---": // End found
          markdownContent = sb.ToString();
          return true;

        case "": // No empty lines allowed
          markdownContent = null!;
          return false;

        default:
          sb.AppendLine(line);
          break;
      }
    }

    // No end found
    markdownContent = null!;
    return false;
  }
}
