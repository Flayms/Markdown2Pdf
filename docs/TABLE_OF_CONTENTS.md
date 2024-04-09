# Table of contents in detail

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

Example creating a TOC with all options. Skip options no needed.:

### C#
```cs
options.TableOfContents = new TableOfContentsOptions {
  ListStyle = ListStyle.Decimal,

  // Include all heading levels from 2 to 4.
  MinDepthLevel = 2,
  MaxDepthLevel = 4,

  HasColoredLinks = True,

  // Include page numbers with leading dots
  PageNumberOptions = new PageNumberOptions {
    TabLeader = Leader.Dots,
  }
};
```

### VB
```vb
options.TableOfContents = New TableOfContentsOptions With {
  .ListStyle = ListStyle.Decimal,

  // Include all heading levels from 2 to 4.
  .MinDepthLevel = 2,
  .MaxDepthLevel = 4,

  .HasColoredLinks = True,

  // Include page numbers with leading dots
  .PageNumberOptions = New PageNumberOptions With {
    .TabLeader = Leader.Dots,
  }
}
```

A header can be omitted from the toc by ending it with `<!-- omit from toc -->`:
```md
## This header won't be displayed in the TOC <!-- omit from toc -->
```

The TOC gets generated within a `<nav class="table-of-contents">`. This can be used to apply extra custom styles.

## Options

| Option              | Description                                               | Default                    |
| ------------------- | --------------------------------------------------------- | -------------------------- |
| `ListStyle`         | Decides which characters to use before the TOC titles.    | `ListStyle.OrderedDefault` |
| `MinDepthLevel`     | The minimum level of heading depth to include in the TOC. | `1`                        |
| `MaxDepthLevel`     | The maximum level of heading depth to include in the TOC. | `6`                        |
| `HasColoredLinks`   | If set, the titles in the TOC get default link markup.    | `false`                    |
| `PageNumberOptions` | If set, the TOC will be generated with page numbers.      | `null`                     |

### ListStyle
| Option              | Description                                                                       |
| ------------------- | --------------------------------------------------------------------------------- |
| `None`              | Just display the TOC items without any preceeding characters.                     |
| `OrderedDefault`    | Use the current themes default list-style for ordered lists.                      |
| `Unordered`         | Use the current themes default list-style for unordered lists.                    |
| `Decimal`           | Preceed the TOC items with numbers separated by points (e.g. 1.1, 1.2, 1.2.1...). |

### PageNumberOptions

| Option              | Description                      |
| ------------------- | -------------------------------- |
| `None`              | No leader.                       |
| `Dots`              | Use dots for the leader.         |
| `Underline`         | Use an underline for the leader. |
| `Dashes`            |Use dashes for the leader.        |

[back to README](..\README.md)
