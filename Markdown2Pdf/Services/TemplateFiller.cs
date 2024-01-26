using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown2Pdf.Services;

internal class TemplateFiller {

  // matches groups like @(myToken)
  private static readonly Regex _tokenRegex = new(@"(?<token>@\(.*?\))",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

  public static string FillTemplate(string template, Dictionary<string, string> model) {
    var matches = _tokenRegex.Matches(template);

    var filled = template;

    foreach (Match match in matches) {
      var token = match.Groups["token"].Value;
      var keyName = token.Replace("@", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

      if (!model.TryGetValue(keyName, out var value))
        throw new Exception($"The given model has no value provided for the templatekey \"{keyName}\".");

      filled = filled.Replace(token, value);
    }

    return filled;
  }
}
