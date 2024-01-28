using PuppeteerSharp;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;

namespace Markdown2Pdf.Tests.Tests;
internal class Utils {

  internal static readonly DirectoryInfo tempDir = new("tempFiles");
  internal static readonly string testFilePath = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))!
    .Replace("bin", "TestFiles");

  internal static readonly FileInfo helloWorldFile = new(Path.Combine(tempDir.FullName, "helloworld.md"));
  internal static readonly FileInfo readmeFile = new(Path.Combine(tempDir.FullName, "README.md"));
  internal static readonly FileInfo headerFile = new(Path.Combine(tempDir.FullName, "header.html"));
  internal static readonly FileInfo footerFile = new(Path.Combine(tempDir.FullName, "footer.html"));
  internal static readonly FileInfo logoFile = new(Path.Combine(tempDir.FullName, "md2pdf.png"));

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
      File.Copy(file, markdownFile);
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

}
