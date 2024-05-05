using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Tests.Tests;
internal class UtilsTest {


  [SetUp]
  public void Setup() => _Setup();

  private static void _Setup() {
    Utils.tempDir.Create();
    Utils.CopyTestFiles();
  }


  [Test]
  public async Task TestUtilsPdfProperties() {
    // arrange

    var options = new Markdown2PdfOptions {
      FilePropertiesTitle = "fileTitle",
    };

    var converter = new Markdown2PdfConverter(options);


    // act
    var pdfPath = await converter.Convert(Utils.helloWorldFile);

    // assert
    Assert.Multiple(() => {
      Assert.That(File.Exists(pdfPath));
      Assert.That(Utils.PdfProperties(pdfPath + 1, "title"), Is.EqualTo("file not found"));
      Assert.That(Utils.PdfProperties(pdfPath, "title"), Is.EqualTo("fileTitle"));
      Assert.That(Utils.PdfProperties(pdfPath, "author"), Is.EqualTo(null));
      Assert.That(Utils.PdfProperties(pdfPath, "title1"), Is.EqualTo("Property title1 not given"));
    });
  }

  [TearDown]
  public void Teardown() => Utils.tempDir.Delete(true);
}
