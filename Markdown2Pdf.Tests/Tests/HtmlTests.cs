using System.Reflection;
using Markdown2Pdf.Options;
using PuppeteerSharp;

namespace Markdown2Pdf.Tests.Tests;

public class Tests {

  private readonly DirectoryInfo _tempDir = new("tempFiles");
  private readonly string _testFilePath = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))!
    .Replace("bin", "TestFiles");

  [SetUp]
  public void Setup() => this._tempDir.Create();

  [Test]
  public async Task TestGeneralFunctionality() {
    // arrange
    var content = "*Hello* **World!**";
    var markdownFile = Path.Combine(this._tempDir.FullName, "markdown.md");

    File.WriteAllText(markdownFile, content);

    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(markdownFile);

    // assert
    Assert.That(File.Exists(pdfPath));
  }

  [Test]
  [TestCase("", "class=\"markdown-body\"")]
  [TestCase("", "<script id=")]
  [TestCase("", "<link rel=\"stylesheet\" href=")]
  [TestCase("*Hello* **World!**", "<p><em>Hello</em> <strong>World!</strong></p>")]
  public void TestConversionToHtml(string markdown, string expectedHtmlPart) {
    // arrange
    var converter = new Markdown2PdfConverter();

    // act
    var html = converter.GenerateHtml(markdown);

    // assert
    Assert.That(html, Does.Contain(expectedHtmlPart));
  }

  [Test]
  public async Task TestModuleFunctionality([Values(
    "<mjx-math class=\"MJX-TEX\" aria-hidden=\"true\">",
    "<div class=\"mermaid\" data-processed=\"true\">",
    "<span class=\"hljs-keyword\">public</span>"
    )] string expectedHtmlContent, [Values(true, false)] bool runLocally) {
    // arrange
    var markdownFile = this._CopyTestFile("README.md");

    var options = new Markdown2PdfOptions {
      ModuleOptions = ModuleOptions.Remote
    };

    if (runLocally) {
      // load modules from local dir
      var nodeModuleLocation = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!, "node_modules");

      Assert.That(Directory.Exists(nodeModuleLocation),
        $"'{nodeModuleLocation}' Could not be found. " +
        "Try running 'npm install' within the executing directory.");

      options.ModuleOptions = ModuleOptions.FromLocalPath(nodeModuleLocation);
    }

    var converter = new Markdown2PdfConverter(options);

    // act
    var html = converter.GenerateHtml(File.ReadAllText(markdownFile));

    // render html
    var tempHtmlPath = Path.Combine(this._tempDir.FullName + "temp.html");
    File.WriteAllText(tempHtmlPath, html);
    var renderedHtml = await _RenderHtmlAsync(tempHtmlPath);

    // assert
    Assert.That(renderedHtml, Does.Contain(expectedHtmlContent));
  }

  private static async Task<string> _RenderHtmlAsync(string htmlFilePath) {
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

  [TearDown]
  public void Teardown() => this._tempDir.Delete(true);

  private string _CopyTestFile(string filename) {
    var testFile = Path.Combine(this._testFilePath, filename);
    var markdownFile = Path.Combine(this._tempDir.FullName, filename);
    File.Copy(testFile, markdownFile);
    return markdownFile;
  }
}
