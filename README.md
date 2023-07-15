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

## Modules

This library uses node_modules packages.
By default they're loaded over https://cdn.jsdelivr.net.

You can also use a local installation of them by running the script `Init.ps1` and setting `Markdown2PdfOptions.ModuleOptions` to `ModuleOptions.Global`.

> **Note:** For this you need to have *npm* installed and added to `PATH`.

Alternatively you can also install the packages from the script manually and configure a custom installation path with `ModuleOptions.FromLocalPath()`.

### Used Modules

* [MathJax](https://github.com/mathjax/MathJax): Latex-Math rendering
* [Mermaid](https://github.com/mermaid-js/mermaid):  diagrams

## Options

* Custom Html-Header
* Custom Html-Footer
* Page-Margins
* Module loading from different sources
* Keeping the temp html