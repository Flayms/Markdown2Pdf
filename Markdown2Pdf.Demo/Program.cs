using Markdown2Pdf;

var settings = new Markdown2PdfSettings {
   HeaderUrl = "header.html",
   FooterUrl = "footer.html",

   MarginOptions = new MarginOptions {
     Top = "80px",
     Bottom ="50px",
     Left = "50px",
     Right = "50px"
   }
};

var converter = new Markdown2PdfConverter(settings);
converter.Convert("README.md");
