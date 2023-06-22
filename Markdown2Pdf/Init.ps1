$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Set-Location $dir

npm i mermaid@10.2.3
npm i katex@0.16.7