# Run this to generate the documentation files for the Github_Wiki

# Cleanup markdown files for Github Wiki
function CleanMarkdown($markdownContent) {
    $cleanedContent = FixLinkExtensions $markdownContent
    $cleanedContent = FixXrefs $cleanedContent
    $cleanedContent = FixCodeBlocks $cleanedContent
    
    return $cleanedContent
}

function FixLinkExtensions($markdownContent) {
    [regex]$removeLinkExtensionRegex = "(?<=\[.*\]\(.*).md(?=\))"
    return $markdownContent -replace $removeLinkExtensionRegex, ""
}

function FixXrefs($markdownContent) {
    [regex]$removeStartOfXrefRegex = '<xref href="(.+\.)*(?=\w+)'
    [regex]$removeEndOfXrefRegex = '(?<=.+)" .*></xref>'

    return $markdownContent -replace $removeStartOfXrefRegex, "" -replace $removeEndOfXrefRegex, ""
}

function FixCodeBlocks($markdownContent) {
    [regex]$codeBlockRegex = '(?ms)<pre><code class="lang-(?<LangName>.+?)">(?<Code>.+?)<\/code><\/pre>'
    $foundMatches = $codeBlockRegex.Matches($markdownContent)
    foreach ($match in $foundMatches) {
        $replacedValue = "`n``````$($match.Groups["LangName"].Value)`n$($match.Groups["Code"].Value)`n```````n"
        $markdownContent = $markdownContent.replace($match.Groups[0].Value, $replacedValue)
    }

    return $markdownContent
}

# main
docfx metadata docfx.json

$markdownFiles = Get-ChildItem -Path "../Markdown2Pdf.wiki" -Filter "*.md" -Recurse -File

foreach ($file in $markdownFiles) {
    $markdownContent = Get-Content $file.FullName -Raw
    $cleanedMarkdown = CleanMarkdown $markdownContent
    Set-Content -Path $file.FullName -Value $cleanedMarkdown
}

Write-Host "Updated markdown files for the Github Wiki."