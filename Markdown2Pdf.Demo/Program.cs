using System.Diagnostics;
using Markdown2Pdf;
using Markdown2Pdf.Options;

var options = new Markdown2PdfOptions {
  HeaderHtml = File.ReadAllText("header.html"),
  FooterHtml = File.ReadAllText("footer.html"),
  DocumentTitle = "Example PDF",

  MarginOptions = new MarginOptions {
    Top = "80px",
    Bottom = "50px",
    Left = "50px",
    Right = "50px"
  },
  KeepHtml = true,
  TableOfContents = new TableOfContents(isOrdered: true, maxDepthLevel: 5)
};

var converter = new Markdown2PdfConverter(options);
var resultPath = await converter.Convert("README.md");

Process.Start(new ProcessStartInfo { FileName = resultPath, UseShellExecute = true });
