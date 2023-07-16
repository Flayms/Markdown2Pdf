using Markdown2Pdf;
using Markdown2Pdf.Options;

var options = new Markdown2PdfOptions {
  HeaderUrl = "header.html",
  FooterUrl = "footer.html",

   MarginOptions = new MarginOptions {
     Top = "80px",
     Bottom ="50px",
     Left = "50px",
     Right = "50px"
   },
   KeepHtml = true,
};

var converter = new Markdown2PdfConverter(options);
_ = converter.Convert("README.md");
