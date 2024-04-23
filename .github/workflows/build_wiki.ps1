# Run this to generate the documentation files for the Github_Wiki
dotnet tool update -g docfx
docfx docfx.json

# Cleanup markdown files for Github Wiki
$removeLinkExtensionRegex = "(?<=\[.*\]\(.*).md(?=\))"
$removeStartOfXrefRegex = "<xref href=""(\w+\.)*(?=\w+)"
$removeEndOfXrefRegex = "(?<=\w+)"" .*></xref>"

$markdownFiles = Get-ChildItem -Path "../Markdown2Pdf.wiki" -Filter "*.md" -Recurse -File

foreach ($file in $markdownFiles) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content -replace $removeLinkExtensionRegex, "" -replace $removeStartOfXrefRegex, "" -replace $removeEndOfXrefRegex, ""
    Set-Content -Path $file.FullName -Value $newContent
}

Write-Output "Updated markdown files for the Github Wiki."
