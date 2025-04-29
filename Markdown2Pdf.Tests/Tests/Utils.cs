using System.Text.RegularExpressions;
using PuppeteerSharp;
using PuppeteerSharp.BrowserData;
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
      Headless = true,
      Args = ["--no-sandbox"], // needed for running inside docker
    };

    var browserFetcher = new BrowserFetcher();
    var installed = browserFetcher.GetInstalledBrowsers();
    var hasDefaultRevisionInstalled = installed.Any(installedBrowser => installedBrowser.BuildId == Chrome.DefaultBuildId);

    if (!hasDefaultRevisionInstalled) {
      // Uninstall old revisions
      foreach (var oldBrowser in installed) {
        Console.WriteLine($"Uninstalling old Chrome version {oldBrowser.BuildId} from {browserFetcher.CacheDir}...");
        browserFetcher.Uninstall(oldBrowser.BuildId);
      }

      Console.WriteLine($"Path to Chrome was not specified & default build is not installed. Downloading Chrome version {Chrome.DefaultBuildId} to {browserFetcher.CacheDir}...");
      _ = await browserFetcher.DownloadAsync(Chrome.DefaultBuildId);
    }

    return await Puppeteer.LaunchAsync(launchOptions);
  }

  internal static void CopyTestFiles() {
    foreach (var file in Directory.EnumerateFiles(testFilePath)) {
      var markdownFile = Path.Combine(tempDir.FullName, Path.GetFileName(file));
      File.Copy(file, markdownFile, true);
    }
  }

  internal static bool PdfContains(string pdfPath, string searchText) => PdfContainsSum(pdfPath, searchText) > 0;

  /// <summary>
  /// Searches for the amount of occurences of the given text in the PDF.
  /// </summary>
  /// <param name="fileName">The PDF to search in.</param>
  /// <param name="searchText">The text to search with.</param>
  /// <returns>The amount of occurences.</returns>
  internal static int PdfContainsSum(string fileName, string searchText) {
    var sum = 0;

    if (!File.Exists(fileName))
      return sum;

    using (var pdf = PdfDocument.Open(fileName)) {
      foreach (var page in pdf.GetPages()) {
        var text = ContentOrderTextExtractor.GetText(page);

        if (text.Contains(searchText))
          ++sum;
      }
    }

    return sum;
  }

  [GeneratedRegex("\r\n?|\n")]
  internal static partial Regex LineBreakRegex();

}
