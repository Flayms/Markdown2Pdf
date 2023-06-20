using Markdig;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;

namespace Markdown2Pdf;

public class Markdown2PdfConverter {

  public Markdown2PdfSettings Settings { get; }

  public Markdown2PdfConverter(Markdown2PdfSettings? settings = null) {
    this.Settings = settings ?? new Markdown2PdfSettings();
  }

  public FileInfo Convert(FileInfo markdownFile) => new FileInfo(this.Convert(markdownFile.FullName));

  public void Convert(FileInfo markdownFile, FileInfo outputFile) => this.Convert(markdownFile.FullName, outputFile.FullName);

  public string Convert(string markdownFilePath) {
    var outputFilePath = Path.GetFileNameWithoutExtension(markdownFilePath) + ".pdf";
    this.Convert(markdownFilePath, outputFilePath);

    return outputFilePath;
  }

  public void Convert(string markdownFilePath, string outputFilePath) {
    var markdownContent = File.ReadAllText(markdownFilePath);

    var htmlFile = this._GenerateHtml(markdownContent);
    var task = this._GeneratePdfAsync(htmlFile, outputFilePath, Path.GetFileNameWithoutExtension(markdownFilePath));
    task.Wait();
  }

  private string _GenerateHtml(string markdownContent) {
    //todo: decide on how to handle pipeline better
    var pipelineBuilder = new MarkdownPipelineBuilder()
      .UseDiagrams();
      //.UseSyntaxHighlighting();
    var pipeline = pipelineBuilder.Build();
    var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

    //todo: support more plugins
    //todo: code-color markup

    //working with node-modules in c# is quite messy..
    var currentLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Markdown2PdfConverter)).Location);
    var templateHtml = File.ReadAllText(Path.Combine(currentLocation, "wwwroot/ContentTemplate.html"));
    //todo: create cleaner solution
    var filledHtml = templateHtml.Replace("TEMP", htmlContent);
    //todo: also not that great
    filledHtml = filledHtml.Replace("../node_modules", Path.Combine(currentLocation, "node_modules"));

    //todo: only for debug
    var htmlPath = Path.GetFullPath("converted.html");
    File.WriteAllText(htmlPath, filledHtml);

    return htmlPath;
  }

  //todo: just work with paths instead of fileInfos
  private async Task _GeneratePdfAsync(string htmlFilePath, string outputFilePath, string title) {
    var browser = await _CreateBrowserAsync();
    var page = await browser.NewPageAsync();

    //todo: take this as parameter
    await page.GoToAsync(htmlFilePath);
    //todo: wait for event instead
    await Task.Delay(3000);

    var marginOptions = new PuppeteerSharp.Media.MarginOptions();
    if (this.Settings.MarginOptions != null) {
      //todo: remove double initialization
      marginOptions = new PuppeteerSharp.Media.MarginOptions {
        Top = this.Settings.MarginOptions.Top,
        Bottom = this.Settings.MarginOptions.Bottom,
        Left = this.Settings.MarginOptions.Left,
        Right = this.Settings.MarginOptions.Right,
      };
    }

    var pdfOptions = new PdfOptions {
      //todo: make this settable
      Format = PaperFormat.A4,
      PrintBackground = true,
      MarginOptions = marginOptions
    };

    //todo: error handling
    //todo: default header is super small
    if (this.Settings.HeaderUrl != null) {
      var headerContent = File.ReadAllText(this.Settings.HeaderUrl);

      //todo: super hacky, rather replace class content
      //todo: create setting and only use fileName as fallback
      headerContent = headerContent.Replace("title", title);
      pdfOptions.HeaderTemplate = headerContent;
      pdfOptions.DisplayHeaderFooter = true;
    }

    if (this.Settings.FooterUrl != null) {
      var footerContent = File.ReadAllText(this.Settings.FooterUrl);
      footerContent = footerContent.Replace("title", title);
      pdfOptions.FooterTemplate = footerContent;
      pdfOptions.DisplayHeaderFooter = true;
    }

    await page.EmulateMediaTypeAsync(MediaType.Screen);
    await page.PdfAsync(outputFilePath, pdfOptions);
  }

  private async Task<IBrowser> _CreateBrowserAsync() {
    var launchOptions = new LaunchOptions {
      Headless = true,
      Args = new[] {
        "--no-sandbox" //todo: check why this is needed
      },
    };

    if (this.Settings.ChromePath == null) {
      using var browserFetcher = new BrowserFetcher();
      Console.WriteLine("Downloading chromium...");
      await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    } else
      launchOptions.ExecutablePath = this.Settings.ChromePath;

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}