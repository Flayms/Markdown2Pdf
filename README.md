<p align="center">
  <a href="https://www.nuget.org/packages/Markdown2Pdf" target="_blank">
    <img alt="Nuget" src="https://img.shields.io/nuget/v/Markdown2Pdf">
  </a>
  
  <a href="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml" target="_blank">
    <img src="https://github.com/Flayms/Markdown2Pdf/actions/workflows/build-and-release.yml/badge.svg?event=workflow_dispatch" alt="Build and Release" />
  </a>
</p>

# Markdown2Pdf
.NET library for converting Markdown to PDF. Uses [Markdig](https://github.com/xoofx/markdig) for converting markdown to html and then [Puppeteer Sharp](https://github.com/hardkoded/puppeteer-sharp) to convert that output to PDF. For a cross-platform console-application checkout [Markdown2Pdf.Console](https://github.com/Flayms/Markdown2Pdf.Console).

A demo can be found [here!](./img/demo.pdf)

## Usage

```c#
var converter = new Markdown2PdfConverter();
converter.Convert("README.md");
```

## Options

To further specify the conversion process you can pass `Markdown2PdfOptions` to the converter:

```c#
var options = var options = new Markdown2PdfOptions {
  HeaderUrl = "header.html",
  FooterUrl = "footer.html",
  DocumentTitle = "Example PDF",
};
var converter = new Markdown2PdfConverter(options);
```

|Option|Description|
|---|---|
|`HeaderUrl`|Path to an html-file to use as the document-header. Allows the classes `date`, `title`, `document-title`, `url`, `pageNumber` and `totalPages` for injection.|
|`FooterUrl`|Path to an html-file to use as the document-footer. Allows the classes `date`, `title`, `document-title`, `url`, `pageNumber` and `totalPages` for injection.|
|`DocumentTitle`|The title of this document. Can be injected into the header / footer by adding the class `document-title` to the element.|
|`MarginOptions`|Css-margins for the sides of the document.|
|`ChromePath`|Path to chrome or chromium executable or self-downloads it if `null`.|
|`ModuleOptions`|Options that decide from where to load additional modules. Default: `ModuleOptions.Remote`.|
|`KeepHtml`|`true` if the created html should not be deleted.|

## Modules

This library uses node_modules packages.
By default they're loaded over https://cdn.jsdelivr.net.

You can also use a local installation of them by running the script `Init.ps1` and setting `Markdown2PdfOptions.ModuleOptions` to `ModuleOptions.Global`.

> **Note:** For this you need to have *npm* installed and added to `PATH`.

Alternatively you can also install the packages from the script manually and configure a custom installation path with `ModuleOptions.FromLocalPath()`.

### Used Modules

* [MathJax](https://github.com/mathjax/MathJax): Latex-Math rendering
* [Mermaid](https://github.com/mermaid-js/mermaid): Diagrams
* [github-markdown-css](https://github.com/sindresorhus/github-markdown-css): Styling