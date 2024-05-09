using System.Reflection;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Tests.Tests;

public partial class HtmlTests {

  [SetUp]
  public void Setup() {
    Utils.tempDir.Create();
    Utils.CopyTestFiles();
  }

  [Test]
  public async Task TestGeneralFunctionality() {
    // arrange
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

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
    var options = new Markdown2PdfOptions {
      ModuleOptions = ModuleOptions.Remote
    };

    if (runLocally) {
      // load modules from local dir
      var nodeModuleLocation = Path.Combine(Path.GetDirectoryName(
        Assembly.GetExecutingAssembly().Location)!, "node_modules");

      Assert.That(Directory.Exists(nodeModuleLocation),
        $"'{nodeModuleLocation}' Could not be found. Try running 'setup.ps1'.");

      options.ModuleOptions = ModuleOptions.FromLocalPath(nodeModuleLocation);
    }

    var converter = new Markdown2PdfConverter(options);

    // act
    var html = converter.GenerateHtml(File.ReadAllText(Utils.readmeFile));

    // render html
    var tempHtmlPath = Path.Combine(Utils.tempDir.FullName + "temp.html");
    File.WriteAllText(tempHtmlPath, html);
    var renderedHtml = await Utils.RenderHtmlAsync(tempHtmlPath);

    // assert
    Assert.That(renderedHtml, Does.Contain(expectedHtmlContent));
  }

  [Test]
  [TestCase("""</a></li><li><a href="#h2-heading">h2 Heading</a><ol>""")]
  [TestCase("""</a></li></ol></li></ol><li><a href="#horizontal-rules">Horizontal Rules</a></li>""")]
  [TestCase("""<a href="#this-is-a-heading_with.and">""")]
  [TestCase("""<a href="#emojis">""")]
  [TestCase("""<a href="#первый-заголовок">Первый заголовок</a>""")]
  public void TableOfContents_Should_Contain(string content) {
    // arrange
    var options = new Markdown2PdfOptions {
      TableOfContents = new TableOfContentsOptions {
        ListStyle = ListStyle.OrderedDefault,
        MinDepthLevel = 2,
        MaxDepthLevel = 5
      }
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var html = converter.GenerateHtml(File.ReadAllText(Utils.readmeFile));

    // remove line endings for easier comparison
    html = Utils.LineBreakRegex().Replace(html, string.Empty);

    // assert
    Assert.That(html, Does.Contain(content));
  }

  [Test]
  [TestCase("""<a href="#h1-heading">""")] // smaller than MinDepth
  [TestCase("""<a href="#h6-heading">""")] // bigger than MaxDepth
  [TestCase("""<a href="#h3-heading">""")] // omitted with comment
  public void TableOfContents_Should_Not_Contain(string content) {
    // arrange
    var options = new Markdown2PdfOptions {
      TableOfContents = new TableOfContentsOptions {
        ListStyle = ListStyle.OrderedDefault,
        MinDepthLevel = 2,
        MaxDepthLevel = 5
      }
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var html = converter.GenerateHtml(File.ReadAllText(Utils.readmeFile));

    // remove line endings for easier comparison
    html = Utils.LineBreakRegex().Replace(html, string.Empty);

    // assert
    Assert.That(html, Does.Not.Contain(content));
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);
}
