using Markdig;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;

namespace MarkdownToPdf;

public class MarkdownToPdfConverter {

  public MarkdownToPdfSettings Settings { get; }

  public MarkdownToPdfConverter(MarkdownToPdfSettings? settings = null) {

    this.Settings = settings ?? new MarkdownToPdfSettings();
  }

  public FileInfo Convert(string markdownFilePath, string outputFilePath) {
    if (!File.Exists(markdownFilePath))
      throw new FileNotFoundException();

    return this.Convert(new FileInfo(markdownFilePath), new FileInfo(outputFilePath));
  }

  public FileInfo Convert(FileInfo markdownFile, FileInfo outputFile) {
    var markdownContent = File.ReadAllText(markdownFile.FullName);

    var htmlFile = this._GenerateHtml(markdownContent);
    var task = this._GeneratePdf(htmlFile, outputFile, Path.GetFileNameWithoutExtension(markdownFile.Name));
    task.Wait();

    return outputFile;
  } 

  private FileInfo _GenerateHtml(string markdownContent) {
    //todo: decide on how to handle pipeline better
    var pipelineBuilder = new MarkdownPipelineBuilder()
      .UseDiagrams();
      //.UseSyntaxHighlighting();
    var pipeline = pipelineBuilder.Build();
    var htmlContent = Markdown.ToHtml(markdownContent, pipeline);

    //todo: make this offline available
    //todo: support more plugins
    //todo: code-color markup
    var scripts = new[] {
      "<script src='https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.5/MathJax.js?config=TeX-MML-AM_CHTML'></script>",
      "<script src='https://cdn.jsdelivr.net/npm/mermaid@10.2.3/dist/mermaid.min.js'></script>",
    };

    var wrappedContent = $"<!DOCTYPE html><html><head>{string.Join("\r\n", scripts)}</head><body>{htmlContent}</body></html>";

    //todo: only for debug
    var htmlFile = new FileInfo("converted.html");
    File.WriteAllText(htmlFile.FullName, wrappedContent);

    return htmlFile;
  }

  //todo: just work with paths instead of fileInfos
  private async Task _GeneratePdf(FileInfo htmlFile, FileInfo outputFile, string title) {
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

    var browser = await Puppeteer.LaunchAsync(launchOptions);

    var page = await browser.NewPageAsync();

    //todo: take this as parameter
    await page.GoToAsync(htmlFile.FullName);
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
    await page.PdfAsync(outputFile.FullName, pdfOptions);
  }

}