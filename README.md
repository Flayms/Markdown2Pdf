<p align="center">
  <a href="https://www.nuget.org/packages/Markdown2Pdf" target="_blank">
    <img alt="Nuget" src="https://img.shields.io/nuget/v/Markdown2Pdf" />
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

.NET library for converting Markdown to PDF. Uses [Markdig](https://github.com/xoofx/markdig) for converting markdown to html and then [Puppeteer Sharp](https://github.com/hardkoded/puppeteer-sharp) to convert that output to PDF.

A created demo PDF can be found [here!](./assets/demo.pdf)

For a cross-platform cli-application using this package checkout [Markdown2Pdf.Console](https://github.com/Flayms/Markdown2Pdf.Console).

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

| Option                        | Description                                                                                                                                              | Default                     |
| ----------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- |
| `ModuleOptions`               | Options that decide from where to load additional modules.                                                                                               | `ModuleOptions.Remote`      |
| `Theme`                       | The styling to apply to the document.                                                                                                                    | `Theme.Github`              |
| `CodeHighlightTheme`          | The theme to use for highlighting code blocks.                                                                                                           | `CodeHighlightTheme.Github` |
| `EnableAutoLanguageDetection` | Auto detect the language for code blocks without specfied language.                                                                                      | `false`                     |
| `HeaderUrl`                   | Path to an html-file to use as the document header. [More Information](#header-and-footer).                                                              | `null`                      |
| `FooterUrl`                   | Path to an html-file to use as the document footer. [More Information](#header-and-footer).                                                              | `null`                      |
| `DocumentTitle`               | The title of this document. Can be injected into the header / footer by adding the class `document-title` to the element.                                | `null`                      |
| `CustomHeadContent`           | A `string` containing any content valid inside a html `<head>` to apply extra scripting / styling to the document.. [More Information](#customization).  | `null`                      |
| `ChromePath`                  | Path to chrome or chromium executable or self-downloads it if `null`.                                                                                    | `null`                      |
| `KeepHtml`                    | `true` if the created html should not be deleted.                                                                                                        | `false`                     |
| `MarginOptions`               | Css-margins for the sides of the document.                                                                                                               | `null`                      |
| `IsLandscape`                 | Paper orientation.                                                                                                                                       | `false`                     |
| `Format`                      | The paper format for the PDF.                                                                                                                            | `A4`                        |
| `Scale`                       | Scale of the content. Must be between 0.1 and 2.                                                                                                         | `1`                         |
| `TableOfContents`             | Creates a TOC from the markdown headers. [More Information](#table-of-contents).                                                                         | `null`                      |

## Header and Footer

With the `Markdown2PdfOptions.HeaderUrl` and `Markdown2PdfOptions.FooterUrl` options a path to a local file containing html for the Header / Footer can be set.  
Html-elements with the classes `date`, `title`, `document-title`, `url`, `pageNumber` will get their content replaced based on the information. Note that `document-title` can be set with the option `Markdown2PdfOptions.DocumentTitle`.

## Customization

Custom head content can be set with the `Markdown2PdfOptions.CustomHeadContent` option.
Example adding PDF pagebreaks:
```cs
options.CustomHeadContent = "<style>h1, h2, h3 { page-break-before: always; }</style>";
```

## Table of contents

To add a table of contents insert
* `[TOC]` (Gitlab Syntax)
* `[[_TOC_]]` (Gitlab Syntax)
* or `<!-- toc -->` (Comment)

into the markdown document and use the `Markdown2PdfOptions.TableOfContents` option:

```md
# My Document

[TOC]
...
```

Example creating a TOC:

```cs
options.TableOfContents = new TableOfContentsOptions {
  ListStyle = ListStyle.Decimal,

  // Include all heading levels from 2 to 4.
  MinDepthLevel = 2,
  MaxDepthLevel = 4
};
```

A header can be omitted from the toc by ending it with `<!-- omit from toc -->`:
```md
## This header won't be displayed in the TOC <!-- omit from toc -->
```

The TOC gets generated within a `<nav class="table-of-contents">`. This can be used to apply extra custom styles.

### Further Options

| Option | Description |
| --- | --- |
| `TableOfContentsOptions.HasColoredLinks` | If set, Headers in TOC get default link markup. |
| `TableOfContentsOptions.HasPageNumbers` | Determines if the TOC should include Page-Numbers. |

## Modules

This library uses node_modules packages.
By default they're loaded over the CDN they're hosted on e.g. https://cdn.jsdelivr.net.

You can also use a local installation by installing the following packages and setting `Markdown2PdfOptions.ModuleOptions` to `ModuleOptions.FromLocalPath()`:

```bash
npm i mathjax@3
npm i mermaid@10
npm i font-awesome
npm i @highlightjs/cdn-assets@11
npm i github-markdown-css
npm i latex.css
```

> **Note:** For this you need to have *npm* installed and added to `PATH`.

| Module                                                                     | Description                               |
| -------------------------------------------------------------------------- | ----------------------------------------- |
| [MathJax](https://github.com/mathjax/MathJax)                              | Latex-Math rendering                      |
| [Mermaid](https://github.com/mermaid-js/mermaid)                           | Diagrams                                  |
| [Font-Awesome](https://fontawesome.com/)                                   | Icons (Supported within mermaid diagrams) |
| [Highlight.js](https://github.com/highlightjs/highlight.js)                | Syntax highlighting                       |
| [github-markdown-css](https://github.com/sindresorhus/github-markdown-css) | Github-Theme                              |
| [latex-css](https://github.com/vincentdoerig/latex-css)                    | Latex-Theme                               |

### Further modification

To get more control over the HTML generation (e.g. to add your own JS-Scripts), modify the `converter.ContentTemplate`.

## Running in Docker

The bundled Chromium that get's installed by Puppeteer doesn't ship with all necessary dependencies (See [Running Puppeteer in Docker](https://github.com/puppeteer/puppeteer/blob/main/docs/troubleshooting.md#running-puppeteer-in-docker)).

To resolve this install them in your `.dockerfile`:

```dockerfile
RUN apt-get update \
    && apt-get install -y wget gnupg \
    && wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
    && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
    && apt-get update \
    && apt-get install -y google-chrome-stable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst fonts-freefont-ttf libxss1 \
      --no-install-recommends \
    && rm -rf /var/lib/apt/lists/*
```

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md).