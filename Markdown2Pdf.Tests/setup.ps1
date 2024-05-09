# Run this before running the tests

$targetDirectory = ".\bin\Debug\net8.0"

New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null
Copy-Item -Path "..\Markdown2Pdf\package.json" -Destination $targetDirectory
Push-Location -Path $targetDirectory
Invoke-Expression -Command "npm install"
Pop-Location