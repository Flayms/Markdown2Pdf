namespace Markdown2Pdf.Tests.Tests;

public class Tests {

  private readonly DirectoryInfo _tempDir = new("tempFiles");

  [SetUp]
  public void Setup() => this._tempDir.Create();

  [Test]
  public async Task TestGeneralFunctionality() {
    //setup
    var content = "*Hello* **World!**";
    var markdownFile = Path.Combine(this._tempDir.FullName, "markdown.md");

    File.WriteAllText(markdownFile, content);

    var converter = new Markdown2PdfConverter();

    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));
  }

  [Test]
  [TestCase("", "class=\"markdown-body\"")]
  [TestCase("", "<script id=")]
  [TestCase("", "<link rel=\"stylesheet\" href=")]
  [TestCase("*Hello* **World!**", "<p><em>Hello</em> <strong>World!</strong></p>")]
  public void TestConversionToHtml(string markdown, string expectedHtmlPart) {
    //setup
    var converter = new Markdown2PdfConverter();

    //execute
    var html = converter._GenerateHtml(markdown);

    //assert
    Assert.That(html, Does.Contain(expectedHtmlPart));
  }

  [TearDown]
  public void Teardown() => this._tempDir.Delete(true);
}