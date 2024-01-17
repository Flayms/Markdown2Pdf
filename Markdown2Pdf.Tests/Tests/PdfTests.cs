using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using Markdown2Pdf.Options;

namespace Markdown2Pdf.Tests.Tests;

public class PdfTests
{

    private readonly DirectoryInfo _tempDir = new("tempFiles");
    private readonly string testFilePath = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(TestContext.CurrentContext.TestDirectory)).Replace("bin", "TestFiles");
    [SetUp]
    public void Setup()
    {
         _tempDir.Create();

    } 


    [Test]
    public async Task TestGeneralOutput()
    {
        //setup
        var markdownFile = CopyTestFile("helloworld.md");
        var converter = new Markdown2PdfConverter();

        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        Assert.That(File.Exists(pdfPath));
        List<int> result = ReadPdfFile(pdfPath, "Hello World!");
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task TestHeader()
    {
        //setup
        var markdownFile = CopyTestFile("helloworld.md");
        var headerFile = CopyTestFile("header.html");
        

         var options = new Markdown2PdfOptions
         {
            HeaderHtml = File.ReadAllText(headerFile),
            DocumentTitle = "Example PDF"
        };
        var converter = new Markdown2PdfConverter(options);
        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        Assert.That(File.Exists(pdfPath));

        List<int> result = ReadPdfFile(pdfPath, "Hello World!");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Example PDF");
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task TestHeaderPages()
    {
        //setup
        var markdownFile = CopyTestFile("README.md");
        var headerFile = CopyTestFile("header.html");
        var logoFile = CopyTestFile("md2pdf.png");

        var options = new Markdown2PdfOptions
        {
            HeaderHtml = File.ReadAllText(headerFile),
            DocumentTitle = "Example PDF"
        };
        var converter = new Markdown2PdfConverter(options);
        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        Assert.That(File.Exists(pdfPath));

        List<int> result = ReadPdfFile(pdfPath, "Common Markdown Functionality");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Example PDF");
        Assert.That(result.Count, Is.EqualTo(4));
    }
    [Test]
    public async Task TestFooter()
    {
        //setup
        var markdownFile = CopyTestFile("helloworld.md");
        var footerFile = CopyTestFile("footer.html");


        var options = new Markdown2PdfOptions
        {
            FooterHtml = File.ReadAllText(footerFile)
        };
        var converter = new Markdown2PdfConverter(options);
        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        Assert.That(File.Exists(pdfPath));

        List<int> result = ReadPdfFile(pdfPath, "Hello World!");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Page");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Page 1/1");
        Assert.That(result.Count, Is.EqualTo(1));

    }

    [Test]
    public async Task TestFooterPages()
    {
        //setup
        var markdownFile = CopyTestFile("README.md");
        var footerFile = CopyTestFile("footer.html");
        var logoFile = CopyTestFile("md2pdf.png");

        var options = new Markdown2PdfOptions
        {
            FooterHtml = File.ReadAllText(footerFile),
        };
        var converter = new Markdown2PdfConverter(options);
        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        List<int> result = ReadPdfFile(pdfPath, "Common Markdown Functionality");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Page");
        Assert.That(result.Count, Is.EqualTo(4));

        result = ReadPdfFile(pdfPath, "Page 1/4");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Page 4/4");
        Assert.That(result.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task TestHeaderFooterPages()
    {
        //setup
        var markdownFile = CopyTestFile("README.md");
        var headerFile = CopyTestFile("header.html");
        var footerFile = CopyTestFile("footer.html");
        var logoFile = CopyTestFile("md2pdf.png");

        var options = new Markdown2PdfOptions
        {
            HeaderHtml = File.ReadAllText(headerFile),
            DocumentTitle = "Example PDF",
            FooterHtml = File.ReadAllText(footerFile)
        };
        var converter = new Markdown2PdfConverter(options);
        //execute
        var pdfPath = await converter.Convert(markdownFile);

        //assert
        List<int> result = ReadPdfFile(pdfPath, "Common Markdown Functionality");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Example PDF");
        Assert.That(result.Count, Is.EqualTo(4));

        result = ReadPdfFile(pdfPath, "Page");
        Assert.That(result.Count, Is.EqualTo(4));

        result = ReadPdfFile(pdfPath, "Page 1/4");
        Assert.That(result.Count, Is.EqualTo(1));

        result = ReadPdfFile(pdfPath, "Page 4/4");
        Assert.That(result.Count, Is.EqualTo(1));
    }
    [TearDown]
    public void Teardown() => _tempDir.Delete(true);

    private String CopyTestFile(String filename)
    {
        var testFile = System.IO.Path.Combine(testFilePath, filename);
        var markdownFile = System.IO.Path.Combine(_tempDir.FullName, filename);
        File.Copy(testFile, markdownFile);
        return markdownFile;
     }

    public List<int> ReadPdfFile(string fileName, String searthText)
    {
        List<int> pages = new List<int>();
        if (File.Exists(fileName))
        {
            PdfReader pdfReader = new PdfReader(fileName);
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                if (currentPageText.Contains(searthText))
                {
                    pages.Add(page);
                }
            }
            pdfReader.Close();
        }
        return pages;
    }
}
