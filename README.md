<p align="center">
  <a href="https://www.nuget.org/packages/Markdown2Pdf" target="_blank">
    <img alt="Nuget" src="https://img.shields.io/nuget/v/Markdown2Pdf">
  </a>
  
  <a href="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml" target="_blank">
    <img src="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml/badge.svg?event=workflow_dispatch" alt="Build and Release" />
  </a>
</p>

<h1 align="center"> Markdown2Pdf</h1>

<p align="center">
  <img src="./assets/md2pdf.svg" alt="Logo" Width=128px/>
  <br>
</p>

.NET library for converting Markdown to PDF. Uses [Markdig](https://github.com/xoofx/markdig) for converting markdown to html and then [Puppeteer Sharp](https://github.com/hardkoded/puppeteer-sharp) to convert that output to PDF. For a cross-platform console-application checkout [Markdown2Pdf.Console](https://github.com/Flayms/Markdown2Pdf.Console).

A demo can be found [here!](./assets/demo.pdf)

## Usage

```cs
var converter = new Markdown2PdfConverter();
var resultPath = await converter.Convert("README.md");
```

### Combine several markdown files to one PDF

A list of markdown files can be passed to the converter. This concatenates the markdown file content.

```cs
var converter = new Markdown2PdfConverter();
var markdownFiles = new List<string>() { "file1.md", "file2.md" };
var resultPath = await converter.Convert(markdownFiles); // "file1.pdf"
```

## Options

To further specify the conversion process you can pass `Markdown2PdfOptions` to the converter:

```cs
var options = new Markdown2PdfOptions {
  HeaderUrl = "header.html",
  FooterUrl = "footer.html",
  DocumentTitle = "Example PDF",
};
var converter = new Markdown2PdfConverter(options);
```

| Option | Description |
| --- | --- |
| `ModuleOptions` | Options that decide from where to load additional modules. Default: `ModuleOptions.Remote`. |
| `Theme` |The styling to apply to the document. Default: `Theme.Github`. |
| `CodeHighlightTheme` | The theme to use for highlighting code blocks. Default: `CodeHighlightTheme.Github`. |
| `HeaderUrl` | Path to an html-file to use as the document header. [More Information](#header-and-footer). |
| `FooterUrl` | Path to an html-file to use as the document footer. [More Information](#header-and-footer). |
| `DocumentTitle` | The title of this document. Can be injected into the header / footer by adding the class `document-title` to the element. |
| `ChromePath` | Path to chrome or chromium executable or self-downloads it if `null`. |
| `KeepHtml` | `true` if the created html should not be deleted. |
| `MarginOptions` | Css-margins for the sides of the document. |
| `IsLandscape` | Paper orientation. |
| `Format` | The paper format for the PDF. |

## Header and Footer

With the `MarkdownPdfOptions.HeaderUrl` and `MarkdownPdfOptions.FooterUrl` options a path to a local file containing html for the Header / Footer can be set.  
Html-elements with the classes `date`, `title`, `document-title`, `url`, `pageNumber` will get their content replaced based on the information. Note that `document-title` can be set with the option `MarkdownPdfOptions.DocumentTitle`.

## Modules

This library uses node_modules packages.
By default they're loaded over the CDN they're hosted on e.g. https://cdn.jsdelivr.net.

You can also use a local installation of them by running the script `Init.ps1` and setting `Markdown2PdfOptions.ModuleOptions` to `ModuleOptions.Global`.

> **Note:** For this you need to have *npm* installed and added to `PATH`.

Alternatively you can also install the packages from the script manually and configure a custom installation path with `ModuleOptions.FromLocalPath()`.

### Further modification

To get more control over the HTML generation (e.g. to add your own JS-Scripts), modify the `converter.ContentTemplate`.

### Used Modules

* [MathJax](https://github.com/mathjax/MathJax): Latex-Math rendering
* [Mermaid](https://github.com/mermaid-js/mermaid): Diagrams
* [Highlight.js](https://github.com/highlightjs/highlight.js): Syntax highlighting
* [github-markdown-css](https://github.com/sindresorhus/github-markdown-css): Github-Theme
* [latex-css](https://github.com/vincentdoerig/latex-css): Latex-Theme
