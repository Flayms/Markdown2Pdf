using Markdig;
using System.IO;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.Reflection;
using System.Linq;
using Markdown2Pdf.Options;
using Markdown2Pdf.Helper;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Markdown2Pdf;

public class Markdown2PdfConverter {

  public Markdown2PdfOptions Options { get; }
  private string? _globalModulePath;

  //todo: readonly dic
  //todo: better way to keep versions in sync
  //todo: implement
  private readonly Dictionary<string, string> _packageLocations = new() {
    {"katex",  "https://cdn.jsdelivr.net/npm/katex@0.16.8" },
    {"mermaid",  "https://cdn.jsdelivr.net/npm/mermaid@10.2.3" }
  };

  public Markdown2PdfConverter(Markdown2PdfOptions? options = null) {
    this.Options = options ?? new Markdown2PdfOptions();

    //todo: maybe not good to do this here
    //load global module path
    //todo: also check custom paths
    if (this.Options.ModuleOptions.ModuleLocation == ModuleLocation.Global) {
      //todo: better error handling for cmd command
      var result = CommandLineHelper.RunCommand("npm list -g");
      var globalModulePath = Path.Combine(Regex.Split(result, "\r\n|\r|\n").First(), "node_modules");

      if (!Directory.Exists(globalModulePath))
        throw new ArgumentException($"Could not locate node_modules at \"{globalModulePath}\"");

      this._globalModulePath = globalModulePath;
    }
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
    var assembly = Assembly.GetAssembly(typeof(Markdown2PdfConverter));
    var currentLocation = Path.GetDirectoryName(assembly.Location);
    var templateHtmlResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("ContentTemplate.html"));
    //var templateHtml = File.ReadAllText(Path.Combine(currentLocation, "wwwroot/ContentTemplate.html"));

    string templateHtml;

    using (Stream stream = assembly.GetManifestResourceStream(templateHtmlResource))
    using (StreamReader reader = new StreamReader(stream)) {
      templateHtml = reader.ReadToEnd();
    }

    //todo: create cleaner solution
    var filledHtml = templateHtml.Replace("TEMP", htmlContent);
    //todo: also not that great
    //todo: make project work without node as well
    //todo: option for global or setable package path...
    var nodeModulePath = Path.Combine(currentLocation, "node_modules");
    if (!Directory.Exists(nodeModulePath)) {
      Console.WriteLine($"Warning: could not locate node_modules at \"{nodeModulePath}\""); //todo: better logger

      //todo: now use either remote or no modules all together..
    }

    filledHtml = filledHtml.Replace("../node_modules", nodeModulePath);

    //todo: only for debug //todo: make temp-file
    var htmlPath = Path.GetFullPath("converted.html");
    File.WriteAllText(htmlPath, filledHtml);

    return htmlPath;
  }

  //todo: just work with paths instead of fileInfos
  private async Task _GeneratePdfAsync(string htmlFilePath, string outputFilePath, string title) {
    //todo: doesn't dispose chromium properly...
    using var browser = await _CreateBrowserAsync();
    var page = await browser.NewPageAsync();

    //todo: take this as parameter
    await page.GoToAsync(htmlFilePath);
    //todo: wait for event instead
    await Task.Delay(3000);

    var marginOptions = new PuppeteerSharp.Media.MarginOptions();
    if (this.Options.MarginOptions != null) {
      //todo: remove double initialization
      marginOptions = new PuppeteerSharp.Media.MarginOptions {
        Top = this.Options.MarginOptions.Top,
        Bottom = this.Options.MarginOptions.Bottom,
        Left = this.Options.MarginOptions.Left,
        Right = this.Options.MarginOptions.Right,
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
    if (this.Options.HeaderUrl != null) {
      var headerContent = File.ReadAllText(this.Options.HeaderUrl);

      //todo: super hacky, rather replace class content
      //todo: create setting and only use fileName as fallback
      headerContent = headerContent.Replace("title", title);
      pdfOptions.HeaderTemplate = headerContent;
      pdfOptions.DisplayHeaderFooter = true;
    }

    if (this.Options.FooterUrl != null) {
      var footerContent = File.ReadAllText(this.Options.FooterUrl);
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

    if (this.Options.ChromePath == null) {
      using var browserFetcher = new BrowserFetcher();
      Console.WriteLine("Downloading chromium...");
      await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
    } else
      launchOptions.ExecutablePath = this.Options.ChromePath;

    return await Puppeteer.LaunchAsync(launchOptions);
  }

}