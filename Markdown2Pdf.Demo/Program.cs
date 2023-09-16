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
  IsLandscape = true,
};

var converter = new Markdown2PdfConverter(options);
var resultPath = await converter.Convert("README.md");

//todo: make this work on linux too
Process.Start("cmd", $"/c start {resultPath}");
