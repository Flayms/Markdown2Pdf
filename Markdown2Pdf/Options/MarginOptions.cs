namespace Markdown2Pdf.Options;

/// <summary>
/// Margin values with units.
/// </summary>
public class MarginOptions {

  /// <inheritdoc cref="PuppeteerSharp.Media.MarginOptions.Top"/>
  public string? Top { get; set; }

  /// <inheritdoc cref="PuppeteerSharp.Media.MarginOptions.Right"/>
  public string? Right { get; set; }

  /// <inheritdoc cref="PuppeteerSharp.Media.MarginOptions.Bottom"/>
  public string? Bottom { get; set; }

  /// <inheritdoc cref="PuppeteerSharp.Media.MarginOptions.Left"/>
  public string? Left { get; set; }
}
