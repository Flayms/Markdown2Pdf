using Markdown2Pdf.Options;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace Markdown2Pdf.Tests.Tests;

public class PdfTests {

  private readonly DirectoryInfo _tempDir = new("tempFiles");
  private readonly string _testFilePath = Path.GetDirectoryName(Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory))!
   .Replace("bin", "TestFiles");

  [SetUp]
  public void Setup() => this._tempDir.Create();

  [Test]
  public async Task TestGeneralOutput() {
    //setup
    var markdownFile = this._CopyTestFile("helloworld.md");
    var converter = new Markdown2PdfConverter();

    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));
    var result = _SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeader() {
    //setup
    var markdownFile = this._CopyTestFile("helloworld.md");
    var headerFile = this._CopyTestFile("header.html");

    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(headerFile),
      DocumentTitle = "Example PDF"
    };
    var converter = new Markdown2PdfConverter(options);
    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));

    var result = _SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeaderPages() {
    //setup
    var markdownFile = this._CopyTestFile("README.md");
    var headerFile = this._CopyTestFile("header.html");

    _ = this._CopyTestFile("md2pdf.png");

    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(headerFile),
      DocumentTitle = "Example PDF"
    };
    var converter = new Markdown2PdfConverter(options);
    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));

    var result = _SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(4));
  }
  [Test]
  public async Task TestFooter() {
    //setup
    var markdownFile = this._CopyTestFile("helloworld.md");
    var footerFile = this._CopyTestFile("footer.html");

    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(footerFile)
    };
    var converter = new Markdown2PdfConverter(options);
    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    Assert.That(File.Exists(pdfPath));

    var result = _SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Page 1/1");
    Assert.That(result, Has.Count.EqualTo(1));

  }

  [Test]
  public async Task TestFooterPages() {
    //setup
    var markdownFile = this._CopyTestFile("README.md");
    var footerFile = this._CopyTestFile("footer.html");

    _ = this._CopyTestFile("md2pdf.png");

    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(footerFile),
    };
    var converter = new Markdown2PdfConverter(options);
    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    var result = _SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(4));

    result = _SearchPdfFile(pdfPath, "Page 1/4");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Page 4/4");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeaderFooterPages() {
    //setup
    var markdownFile = this._CopyTestFile("README.md");
    var headerFile = this._CopyTestFile("header.html");
    var footerFile = this._CopyTestFile("footer.html");

    _ = this._CopyTestFile("md2pdf.png");

    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(headerFile),
      DocumentTitle = "Example PDF",
      FooterHtml = File.ReadAllText(footerFile)
    };
    var converter = new Markdown2PdfConverter(options);
    //execute
    var pdfPath = await converter.Convert(markdownFile);

    //assert
    var result = _SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(4));

    result = _SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(4));

    result = _SearchPdfFile(pdfPath, "Page 1/4");
    Assert.That(result, Has.Count.EqualTo(1));

    result = _SearchPdfFile(pdfPath, "Page 4/4");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestPDFTwoFiles() {
    //setup
    var markdownFile = this._CopyTestFile("helloworld.md");
    var markdownFile1 = this._CopyTestFile("README.md");
    var logoFile = this._CopyTestFile("md2pdf.png");
    var markdownList = new List<string>() { markdownFile, markdownFile1 };
    var converter = new Markdown2PdfConverter();

    //execute
    var pdfPath = await converter.Convert(markdownList);

    //assert
    Assert.That(File.Exists(pdfPath));
    var result = _SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(2));
    Assert.Multiple(() => {
      Assert.That(result[0], Is.EqualTo(1));
      Assert.That(result[1], Is.EqualTo(2));
    });
  }

  [Test]
  public async Task TestPDFTwoFilesSwitched() {
    //setup
    var markdownFile = this._CopyTestFile("helloworld.md");
    var markdownFile1 = this._CopyTestFile("README.md");
    var logoFile = this._CopyTestFile("md2pdf.png");
    var markdownList = new List<string>() { markdownFile1, markdownFile };
    var converter = new Markdown2PdfConverter();

    //execute
    var pdfPath = await converter.Convert(markdownList);

    //assert
    Assert.That(File.Exists(pdfPath));
    var result = _SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(2));
    Assert.Multiple(() => {
      Assert.That(result[0], Is.EqualTo(2));
      Assert.That(result[1], Is.EqualTo(4));
    });
  }

  [TearDown]
  public void Teardown() => this._tempDir.Delete(true);

  private string _CopyTestFile(string filename) {
    var testFile = Path.Combine(this._testFilePath, filename);
    var markdownFile = Path.Combine(this._tempDir.FullName, filename);
    File.Copy(testFile, markdownFile);
    return markdownFile;
  }

  /// <summary>
  /// Searches a PDF file for the given text.
  /// </summary>
  /// <param name="fileName">The PDF to search.</param>
  /// <param name="searchText">The text to search with.</param>
  /// <returns>A list of the page numbers containing the text.</returns>
  private static List<int> _SearchPdfFile(string fileName, string searchText) {
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
