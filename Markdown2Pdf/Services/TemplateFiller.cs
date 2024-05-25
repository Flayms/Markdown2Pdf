using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown2Pdf.Services;

/// <summary>
/// Simple templating service.
/// </summary>
public class TemplateFiller {

  /// <summary>
  /// matches groups like @(myToken).
  /// </summary>
  private static readonly Regex _tokenRegex = new(@"(?<token>@\(.*?\))",
    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

  /// <summary>
  /// Replaces all tokens of the form <i>@(key)</i> with the given values in the <paramref name="model"/>.
  /// </summary>
  /// <param name="template">The template to replace in.</param>
  /// <param name="model">The model, containg the keys and values.</param>
  /// <returns>The filled template.</returns>
  public static string FillTemplate(string template, Dictionary<string, string> model) {
    var matches = _tokenRegex.Matches(template);

    var filled = template;

    foreach (Match match in matches) {
      var token = match.Groups["token"].Value;
      var keyName = token.Replace("@", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

      if (!model.TryGetValue(keyName, out var value))
        value = string.Empty;

      filled = filled.Replace(token, value);
    }

    return filled;
  }
}
