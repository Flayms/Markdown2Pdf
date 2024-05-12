using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Markdown2Pdf.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Markdown2Pdf.Services;
internal class InlineOptionsParser {

  public static Markdown2PdfOptions ParseYamlFrontMatter(string markdownFilePath) {
    if (!_TryReadYamlFrontMatter(markdownFilePath, out var yamlContent))
      throw new Exception($"Could not find a YAML front matter block in '{markdownFilePath}'.");

    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(HyphenatedNamingConvention.Instance)
      .Build();

    var options = deserializer.Deserialize<SerializableOptions>(yamlContent);
    return options.ToMarkdown2PdfOptions();
  }

  private static bool _TryReadYamlFrontMatter(string markdownFilePath, out string markdownContent) {
    using var reader = File.OpenText(markdownFilePath);

    var identifiers = new Dictionary<string, string>() {
      {"---", "---" },
      {"<!--", "-->" },
    };

    var firstLine = reader.ReadLine();
    if (!identifiers.TryGetValue(firstLine, out var endIdentifier)) {
      markdownContent = null!;
      return false;
    }
    var sb = new StringBuilder();

    string line;
    while ((line = reader.ReadLine()) != null) {
      if (line == endIdentifier) {
        markdownContent = sb.ToString();
        return true;
      }

      sb.AppendLine(line);
    }

    // No end found
    markdownContent = null!;
    return false;
  }

}
