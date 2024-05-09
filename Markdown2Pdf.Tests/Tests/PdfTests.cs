using Markdown2Pdf.Options;
using UglyToad.PdfPig;

namespace Markdown2Pdf.Tests.Tests;

public class PdfTests {

  static PdfTests() => _Setup();

  [SetUp]
  public void Setup() => _Setup();

  private static void _Setup() {
    Utils.tempDir.Create();
    Utils.CopyTestFiles();
  }

  [Test]
  public async Task Should_Create_Pdf_File() {
    // Arrange
    var converter = new Markdown2PdfConverter();

    // Act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // Assert
    Assert.Multiple(() => {
      Assert.That(File.Exists(pdfPath));
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
    });
  }

  private static object?[] _GetTestCasesHeaderFooter() => [
      new [] { File.ReadAllText(Utils.headerFile), null, "Header Text" },
      new [] { null, File.ReadAllText(Utils.footerFile), "Page 1/1" },
    ];

  [Test]
  [TestCaseSource(nameof(_GetTestCasesHeaderFooter))]
  public async Task Should_Generate_Header_Footer(string? headerContent, string? footerContent, string expected) {
    // Arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = headerContent,
      FooterHtml = footerContent,
      DocumentTitle = "Header Text"
    };

    var converter = new Markdown2PdfConverter(options);

    // Act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // Assert
    Assert.That(Utils.PdfContains(pdfPath, expected));
  }

  [Test]
  public async Task Should_Generate_Header_For_Every_Page() {
    // Arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = File.ReadAllText(Utils.headerFile),
      DocumentTitle = "Example PDF"
    };
    var converter = new Markdown2PdfConverter(options);

    // Act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // Assert
    var amountOfOccurences = Utils.PdfContainsSum(pdfPath, "Example PDF");
    Assert.That(amountOfOccurences, Is.EqualTo(4));
  }

  [Test]
  public async Task Should_Generate_Footer_For_Every_Page() {
    // Arrange
    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(Utils.footerFile),
    };
    var converter = new Markdown2PdfConverter(options);

    // Act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // Assert
    Assert.Multiple(() => {
      Assert.That(Utils.PdfContains(pdfPath, "Page 1/4"));
      Assert.That(Utils.PdfContains(pdfPath, "Page 4/4"));
    });
  }

  [Test]
  public async Task Should_Combine_2_Pdf_Files() {
    // Arrange
    var markdownList = new List<string>() { Utils.helloWorldFile, Utils.readmeFile };
    var converter = new Markdown2PdfConverter();

    // Act
    var pdfPath = await converter.Convert(markdownList);

    // Assert
    Assert.Multiple(() => {
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
      Assert.That(Utils.PdfContains(pdfPath, "Common Markdown Functionality"));
    });
  }

  [Test]
  public async Task Should_Generate_Table_Of_Contents() {
    // Arrange
    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(Utils.footerFile),
      TableOfContents = new TableOfContentsOptions {
        ListStyle = ListStyle.OrderedDefault,
        MaxDepthLevel = 4
      }
    };
    var converter = new Markdown2PdfConverter(options);

    // Act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // Assert
    Assert.Multiple(() => {
      // We expect that with the TOC the page count increases
      Assert.That(Utils.PdfContains(pdfPath, "Page 1/5"));
      Assert.That(Utils.PdfContains(pdfPath, "Page 5/5"));
    });
  }

  private static object[] _GetTestCasesPdfPath() => [
      new [] { Path.Combine(Utils.tempDir.FullName, "myhello.pdf") },
      new [] { Path.Combine(Utils.tempDir.FullName, "test", "myhello.pdf") },
    ];

  [Test]
  [TestCaseSource(nameof(_GetTestCasesPdfPath))]
  public async Task Should_Generate_Pdf_At(string outputFilePath) {
    // Arrange
    var converter = new Markdown2PdfConverter();

    // Act
    var pdfPath = await converter.Convert(Utils.helloWorldFile, outputFilePath);

    // Assert
    Assert.Multiple(() => {
      Assert.That(pdfPath, Is.EqualTo(outputFilePath));
      Assert.That(File.Exists(pdfPath));
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
    });
  }

  [Test]
  [TestCaseSource(nameof(_GetTestCasesPdfPath))]
  public async Task Should_Combine_2_Pdfs_At(string outputFilePath) {
    // Arrange
    var markdownList = new List<string>() { Utils.helloWorldFile, Utils.readmeFile };
    var converter = new Markdown2PdfConverter();

    // Act
    var pdfPath = await converter.Convert(markdownList, outputFilePath);

    // Assert
    Assert.Multiple(() => {
      Assert.That(pdfPath, Is.EqualTo(outputFilePath));
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
      Assert.That(Utils.PdfContains(pdfPath, "Common Markdown Functionality"));
    });
  }

  [Test]
  [TestCase(null, null, "helloworld")]
  [TestCase("testDocumentTitle", null, "testDocumentTitle")]
  [TestCase(null, "myMetadataTitle", "myMetadataTitle")]
  [TestCase("testDocumentTitle", "myMetadataTitle", "myMetadataTitle")]
  public async Task Should_Set_Pdf_Metadata(string documentTitle, string metadataTitle, string expectedTitle) {
    // Arrange

    var options = new Markdown2PdfOptions {
      DocumentTitle = documentTitle,
      MetadataTitle = metadataTitle,
    };

    var converter = new Markdown2PdfConverter(options);

    // Act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);
    using var pdf = PdfDocument.Open(pdfPath);

    // Assert
    Assert.Multiple(() => {
      Assert.That(pdf.Information.Title, Is.EqualTo(expectedTitle));
      Assert.That(pdf.Information.Author, Is.EqualTo(null));
    });
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);

}
