#!/bin/bash
# buildEffects
# Compiles all .fx files found in the project's Content directory.
# Intended for usage with VS Code Build Tasks tooling.
# You may need to change the path to fxc.exe depending on your installation.

Write-Output "Starting T4 processing..."

Set-Location $PSScriptRoot
Set-Location ..\RoguelikeFNA

# create our output directory
if ((Test-Path("T4Templates\Output")) -eq 0) { New-Item  -ItemType "directory" -Path "T4Templates\Output" }

$files = Get-ChildItem ".\T4Templates\*" -Include *.tt
$t4Path = "C:/Program Files/Microsoft Visual Studio/2022/Community/Common7/IDE/TextTransform.exe"

foreach ($file in $files)
{
    $fileName = $file.BaseName
    Write-Output "Building ${fileName}.cs from ${fileName}.tt"

    # Build the template
    & $t4Path -r System.dll -r mscorlib.dll -r netstandard.dll -r System.IO.FileSystem.dll -r System.Linq.dll -r System.Text.RegularExpressions -out "T4Templates\Output\${fileName}.cs" "T4Templates\${fileName}.tt"
    
    Write-Output "Built ${fileName}.cs from ${fileName}.tt"
}




