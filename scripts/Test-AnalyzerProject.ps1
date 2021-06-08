#Requires -Version 6.0

[CmdletBinding()]
param ()

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$ProjectName = 'F0.Analyzers.csproj'
$ProjectDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'production', 'F0.Analyzers'
$ProjectFile = Join-Path -Path $ProjectDirectory -ChildPath $ProjectName
$ExampleDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'example'
$ExampleProjects =  Get-ChildItem -Path $ExampleDirectory -Filter '*.csproj' -Recurse -Depth 1 -File
$ReferenceXPath = "/Project/ItemGroup/ProjectReference[contains(@Include,'$ProjectName')]"

function Add-Reference {
    Write-Host 'dotnet add reference' -ForegroundColor Blue

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet add $ExampleProject reference $ProjectFile

        Write-Host '   - ' -NoNewline
        Write-Host $output

        [xml]$Xml = Get-Content -Path $ExampleProject

        $ProjectReference = $Xml.SelectSingleNode($ReferenceXPath)
        $ProjectReference.SetAttribute('PrivateAssets', 'all')
        $ProjectReference.SetAttribute('ReferenceOutputAssembly', 'false')
        $ProjectReference.SetAttribute('OutputItemType', 'Analyzer')
        $ProjectReference.SetAttribute('SetTargetFramework', 'TargetFramework=netstandard2.0')

        $Xml.Save($ExampleProject)
    }
}

function Remove-Reference {
    Write-Host 'dotnet remove reference' -ForegroundColor Blue

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet remove $ExampleProject reference $ProjectFile

        Write-Host '   - ' -NoNewline
        Write-Host $output
    }
}

function Wait-ManualTest {
    Write-Host 'Setup completed successfully. You may now test the Analyzers locally via the Examples. When finished, press Enter/Return to continue with Teardown . . .' -ForegroundColor Yellow

    $Input = Read-Host
}

function Write-TestInvoke {
    Write-Host 'Manual local test invoked.' -ForegroundColor Magenta
}

function Write-TestComplete {
    Write-Host 'Manual local test completed.' -ForegroundColor Green
}

function Write-Version {
    $output = dotnet --version

    Write-Host "     .NET SDK $output"
}

Write-TestInvoke
Write-Version
Add-Reference
Wait-ManualTest
Remove-Reference
Write-TestComplete
