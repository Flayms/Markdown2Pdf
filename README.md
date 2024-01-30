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

> An enumeration of markdown files can also be passed to the converter, combining them into one PDF. 

## Options

To further specify the conversion process, pass `Markdown2PdfOptions` to the converter:

```cs
var options = new Markdown2PdfOptions {
  HeaderUrl = "header.html",
  FooterUrl = "footer.html",
  DocumentTitle = "Example PDF",
};
var converter = new Markdown2PdfConverter(options);
```

| Option | Description | Default |
| --- | --- | --- |
| `ModuleOptions` | Options that decide from where to load additional modules. | `ModuleOptions.Remote` |
| `Theme` |The styling to apply to the document. | `Theme.Github` |
| `CodeHighlightTheme` | The theme to use for highlighting code blocks. | `CodeHighlightTheme.Github` |
| `HeaderUrl` | Path to an html-file to use as the document header. [More Information](#header-and-footer). | `null` |
| `FooterUrl` | Path to an html-file to use as the document footer. [More Information](#header-and-footer). | `null` |
| `DocumentTitle` | The title of this document. Can be injected into the header / footer by adding the class `document-title` to the element. | `null` |
| `CustomCss` | A `string` containing CSS to apply extra styling to the document. [More Information](#custom-style).| `String.Empty` |
| `ChromePath` | Path to chrome or chromium executable or self-downloads it if `null`. | `null` |
| `KeepHtml` | `true` if the created html should not be deleted. | `false` |
| `MarginOptions` | Css-margins for the sides of the document. | `null` |
| `IsLandscape` | Paper orientation. | `false` |
| `Format` | The paper format for the PDF. | `A4` |
| `Scale` | Scale of the content. Must be between 0.1 and 2. | `1` |
| `TableOfContents` | Creates a TOC out of the markdown headers and writes it into a `<!--TOC-->` comment within the markdown document. | `null` |

## Header and Footer

With the `Markdown2PdfOptions.HeaderUrl` and `Markdown2PdfOptions.FooterUrl` options a path to a local file containing html for the Header / Footer can be set.  
Html-elements with the classes `date`, `title`, `document-title`, `url`, `pageNumber` will get their content replaced based on the information. Note that `document-title` can be set with the option `Markdown2PdfOptions.DocumentTitle`.

## Custom Style

Custom CSS can be set with the `Markdown2PdfOptions.CustomCss` option.
Example adding pagebreaks:
```cs
options.CustomCss = "<style>h1, h2, h3 { page-break-before: always; }</style>";
```


## Modules

This library uses node_modules packages.
By default they're loaded over the CDN they're hosted on e.g. https://cdn.jsdelivr.net.

You can also use a local installation by installing the following packages and setting `Markdown2PdfOptions.ModuleOptions` to `ModuleOptions.FromLocalPath()`:

```bash
npm i mathjax@3
npm i mermaid@10.2.3
npm i @highlightjs/cdn-assets@11.9.0
npm i github-markdown-css
npm i latex.css
```

> **Note:** For this you need to have *npm* installed and added to `PATH`.

| Module | Description |
| --- | --- |
| [MathJax](https://github.com/mathjax/MathJax) | Latex-Math rendering |
| [Mermaid](https://github.com/mermaid-js/mermaid) | Diagrams |
| [Highlight.js](https://github.com/highlightjs/highlight.js) | Syntax highlighting |
| [github-markdown-css](https://github.com/sindresorhus/github-markdown-css) | Github-Theme |
| [latex-css](https://github.com/vincentdoerig/latex-css) | Latex-Theme |

### Further modification

To get more control over the HTML generation (e.g. to add your own JS-Scripts), modify the `converter.ContentTemplate`.
