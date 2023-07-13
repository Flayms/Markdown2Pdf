namespace Markdown2Pdf.Tests;

public class Tests {

  private readonly DirectoryInfo _tempDir = new DirectoryInfo("tempFiles");

  [SetUp]
  public void Setup() => _tempDir.Create();

  [Test]
  public void TestGenerallFunctionality() {
    //setup
    var content = "*Hello* **World!**";
    var markdownFile = Path.Combine(_tempDir.FullName, "markdown.md");

    File.WriteAllText(markdownFile, content);

    var converter = new Markdown2PdfConverter();

    //execute
    var pdfPath = converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));
  }

  [TearDown]
  public void Teardown() => _tempDir.Delete(true);
}