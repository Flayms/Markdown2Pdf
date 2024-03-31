﻿using Markdown2Pdf.Options;

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
  public async Task TestGeneratesPdf() {
    // arrange
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.Multiple(() => {
      Assert.That(File.Exists(pdfPath));
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
    });
  }

  private static object?[] _GetTestCases() => new object?[] {
      new [] { File.ReadAllText(Utils.headerFile), null, "Header Text" },
      new [] { null, File.ReadAllText(Utils.footerFile), "Page 1/1" },
    };

  [Test]
  [TestCaseSource(nameof(_GetTestCases))]
  public async Task TestHeaderFooter2(string? headerContent, string? footerContent, string expected) {
    // arrange
    var options = new Markdown2PdfOptions {
      HeaderHtml = headerContent,
      FooterHtml = footerContent,
      DocumentTitle = "Header Text"
    };

    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.That(Utils.PdfContains(pdfPath, expected));
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
    var amountOfOccurences = Utils.PdfContainsSum(pdfPath, "Example PDF");
    Assert.That(amountOfOccurences, Is.EqualTo(4));
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
    Assert.Multiple(() => {
      Assert.That(Utils.PdfContains(pdfPath, "Page 1/4"));
      Assert.That(Utils.PdfContains(pdfPath, "Page 4/4"));
    });
  }

  [Test]
  public async Task TestCombineTwoFiles() {
    // arrange
    var markdownList = new List<string>() { Utils.helloWorldFile, Utils.readmeFile };
    var converter = new Markdown2PdfConverter();

    // act
    var pdfPath = await converter.Convert(markdownList);

    // assert
    Assert.Multiple(() => {
      Assert.That(Utils.PdfContains(pdfPath, "Hello World!"));
      Assert.That(Utils.PdfContains(pdfPath, "Common Markdown Functionality"));
    });
  }

  [Test]
  public async Task TestTableOfContents() {
    // arrange
    var options = new Markdown2PdfOptions {
      FooterHtml = File.ReadAllText(Utils.footerFile),
      TableOfContents = new TableOfContentsOptions {
        ListStyle = ListStyle.OrderedDefault,
        MaxDepthLevel = 4
      }
    };
    var converter = new Markdown2PdfConverter(options);

    // act
    var pdfPath = await converter.Convert(Utils.readmeFile);

    // assert
    Assert.Multiple(() => {
      Assert.That(Utils.PdfContains(pdfPath, "Page 1/5"));
      Assert.That(Utils.PdfContains(pdfPath, "Page 5/5"));
    });
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);

}
