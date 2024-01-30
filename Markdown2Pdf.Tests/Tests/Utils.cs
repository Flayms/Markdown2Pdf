using System.Text.RegularExpressions;
using PuppeteerSharp;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Markdown2Pdf.Tests.Tests;
internal partial class Utils {

  internal static readonly DirectoryInfo tempDir = new("tempFiles");
  internal static readonly string testFilePath = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))!
    .Replace("bin", "TestFiles");

  internal static readonly string helloWorldFile = Path.Combine(tempDir.FullName, "helloworld.md");
  internal static readonly string readmeFile = Path.Combine(tempDir.FullName, "README.md");
  internal static readonly string headerFile = Path.Combine(tempDir.FullName, "header.html");
  internal static readonly string footerFile = Path.Combine(tempDir.FullName, "footer.html");
  internal static readonly string logoFile = Path.Combine(tempDir.FullName, "md2pdf.png");

  internal static async Task<string> RenderHtmlAsync(string htmlFilePath) {
    using var browser = await _CreateBrowserAsync();
    var page = await browser.NewPageAsync();

    _ = await page.GoToAsync("file:///" + htmlFilePath, WaitUntilNavigation.Networkidle2);
    return await page.GetContentAsync();
  }

  private static async Task<IBrowser> _CreateBrowserAsync() {
    var launchOptions = new LaunchOptions {
      Headless = true
    };

    using var browserFetcher = new BrowserFetcher();
    var localRevs = browserFetcher.LocalRevisions();

    if (!localRevs.Contains(BrowserFetcher.DefaultChromiumRevision)) {
      _ = await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    }

    return await Puppeteer.LaunchAsync(launchOptions);
  }

  internal static void CopyTestFiles() {
    foreach (var file in Directory.EnumerateFiles(testFilePath)) {
      var markdownFile = Path.Combine(tempDir.FullName, Path.GetFileName(file));
      File.Copy(file, markdownFile, true);
    }
  }

  /// <summary>
  /// Searches in a PDF file for the given text.
  /// </summary>
  /// <param name="fileName">The PDF to search in.</param>
  /// <param name="searchText">The text to search with.</param>
  /// <returns>A list of all page numbers containing the text.</returns>
  internal static List<int> SearchPdfFile(string fileName, string searchText) {
    var pages = new List<int>();

    if (!File.Exists(fileName))
      return pages;

    using (var pdf = PdfDocument.Open(fileName)) {
      foreach (var page in pdf.GetPages()) {
        var text = ContentOrderTextExtractor.GetText(page);

        if (text.Contains(searchText))
          pages.Add(page.Number);
      }
    }

    return pages;
  }

  [GeneratedRegex("\r\n?|\n")]
  internal static partial Regex LineBreakRegex();

}
