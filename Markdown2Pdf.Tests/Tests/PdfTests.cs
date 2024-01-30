using Markdown2Pdf.Options;

namespace Markdown2Pdf.Tests.Tests;

public class PdfTests {

  [SetUp]
  public void Setup() {
    Utils.tempDir.Create();
    Utils.CopyTestFiles();
  }

  [Test]
  public async Task TestGeneralOutput() {
    // arrange
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.That(File.Exists(pdfPath));
    var result = Utils.SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeader() {
    // arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(Utils.headerFile),
      DocumentTitle = "Example PDF"
    };

    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.That(File.Exists(pdfPath));

    var result = Utils.SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeaderPages() {
    // arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(Utils.headerFile),
      DocumentTitle = "Example PDF"
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // assert
    Assert.That(File.Exists(pdfPath));

    var result = Utils.SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(4));
  }

  [Test]
  public async Task TestFooter() {
    // arrange

    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(Utils.footerFile)
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.That(File.Exists(pdfPath));

    var result = Utils.SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Page 1/1");
    Assert.That(result, Has.Count.EqualTo(1));

  }

  [Test]
  public async Task TestFooterPages() {
    // arrange
    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(Utils.footerFile),
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // assert
    var result = Utils.SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(4));

    result = Utils.SearchPdfFile(pdfPath, "Page 1/4");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Page 4/4");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestHeaderFooterPages() {
    // arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(Utils.headerFile),
      DocumentTitle = "Example PDF",
      FooterHtml = File.ReadAllText(Utils.footerFile)
    };
    var converter = new Markdown2PdfConverter(options);
    // act

    var pdfPath = await converter.Convert(Utils.readmeFile);

    // assert
    var result = Utils.SearchPdfFile(pdfPath, "Common Markdown Functionality");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Example PDF");
    Assert.That(result, Has.Count.EqualTo(4));

    result = Utils.SearchPdfFile(pdfPath, "Page");
    Assert.That(result, Has.Count.EqualTo(4));

    result = Utils.SearchPdfFile(pdfPath, "Page 1/4");
    Assert.That(result, Has.Count.EqualTo(1));

    result = Utils.SearchPdfFile(pdfPath, "Page 4/4");
    Assert.That(result, Has.Count.EqualTo(1));
  }

  [Test]
  public async Task TestPDFTwoFiles() {
    // arrange
    var markdownList = new List<string>() { Utils.helloWorldFile, Utils.readmeFile };
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(markdownList);

    // assert
    Assert.That(File.Exists(pdfPath));
    var result = Utils.SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(2));
    Assert.Multiple(() => {
      Assert.That(result[0], Is.EqualTo(1));
      Assert.That(result[1], Is.EqualTo(2));
    });
  }

  [Test]
  public async Task TestPDFTwoFilesSwitched() {
    // arrange
    var markdownList = new List<string>() { Utils.readmeFile, Utils.helloWorldFile };
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(markdownList);

    // assert
    Assert.That(File.Exists(pdfPath));
    var result = Utils.SearchPdfFile(pdfPath, "Hello World!");
    Assert.That(result, Has.Count.EqualTo(2));
    Assert.Multiple(() => {
      Assert.That(result[0], Is.EqualTo(2));
      Assert.That(result[1], Is.EqualTo(4));
    });
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);

}
