using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Markdown2Pdf;

//todo: refac
internal class TemplateFiller {

  //matches groups like @(myToken)
  private static readonly Regex _TOKEN_REGEX = new (@"(?<token>@\(.*\))",
    RegexOptions.Compiled |RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

  public static string FillTemplate(string template, Dictionary<string, string> model) {
    var matches =  _TOKEN_REGEX.Matches(template);

    var filled = template;

    foreach (Match match in matches) {
      var token = match.Groups["token"].Value;
      var keyName = token.Replace("@", string.Empty).Replace("(", string.Empty).Replace(")", string.Empty);

      //todo: better exception in fail case
      var value = model[keyName];

      filled = filled.Replace(token, value);
    }

    return filled;
  }
}