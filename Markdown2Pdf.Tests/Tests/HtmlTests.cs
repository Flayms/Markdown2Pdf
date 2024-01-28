using PuppeteerSharp;

namespace Markdown2Pdf.Tests.Tests;

public class Tests {

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
    var pdfPath = await converter.Convert(Utils.helloWorldFile.FullName);

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
  [TestCase("<mjx-math class=\"MJX-TEX\" aria-hidden=\"true\">", TestName = "Mathjax")]
  [TestCase("<div class=\"mermaid\" data-processed=\"true\">", TestName = "Mermaid")]
  [TestCase("<span class=\"hljs-keyword\">public</span>", TestName = "HighlightJs")]
  public async Task TestModuleFunctionality(string expectedHtmlContent) {
    // arrange
    var converter = new Markdown2PdfConverter();

    // act
    var html = converter.GenerateHtml(File.ReadAllText(Utils.readmeFile.FullName));

    // render html
    var tempHtmlPath = Path.Combine(Utils.tempDir.FullName + "temp.html");
    File.WriteAllText(tempHtmlPath, html);
    var renderedHtml = await Utils.RenderHtmlAsync(tempHtmlPath);

    // assert
    Assert.That(renderedHtml, Does.Contain(expectedHtmlContent));
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);

}
