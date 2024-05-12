using System.IO;
using System.Text;
using Markdown2Pdf.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Markdown2Pdf.Services;
internal class InlineOptionsParser {

  public static bool TryParseYamlFrontMatter(string markdownFilePath, out Markdown2PdfOptions options) {
    if (!_TryReadYamlFrontMatter(markdownFilePath, out var markdownContent)) {
      options = null!;
      return false;
    }

    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();
    var yamlObject = deserializer.Deserialize<SerializableOptions>(markdownContent);
    options = yamlObject.ToMarkdown2PdfOptions();
    return true;
  }

  private static bool _TryReadYamlFrontMatter(string markdownFilePath, out string markdownContent) {
    using var reader = File.OpenText(markdownFilePath);

    var firstLine = reader.ReadLine();
    if (firstLine != "---") { // Start found // TODO: allow comments
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
