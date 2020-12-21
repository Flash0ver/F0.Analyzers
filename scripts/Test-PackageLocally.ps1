#Requires -Version 7.0

[CmdletBinding()]
param (
    [Parameter()]
    [switch]$ReleaseConfiguration
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$BuildConfiguration = $ReleaseConfiguration ? 'Release' : 'Debug'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$ProjectName = 'F0.Analyzers'
$ProjectDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'package', 'F0.Analyzers.Package'
$ProjectFile = Join-Path -Path $ProjectDirectory -ChildPath 'F0.Analyzers.Package.csproj'
$PackageFile = Join-Path -Path $ProjectDirectory -ChildPath 'bin' -AdditionalChildPath $BuildConfiguration, '*.nupkg'
$PropertiesFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'Release.props'
$VersionXPath = '/Project/PropertyGroup/F0Version'
$VersionSuffix = 'local'
$LocalFeedName = 'local-feed'
$LocalFeedDirectory = Join-Path -Path $RepositoryRootPath -ChildPath $LocalFeedName
$ExampleDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'example'
$ExampleProjects = @(
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example' -AdditionalChildPath 'F0.Analyzers.Example.csproj'
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example.CSharp2' -AdditionalChildPath 'F0.Analyzers.Example.CSharp2.csproj'
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example.CSharp7' -AdditionalChildPath 'F0.Analyzers.Example.CSharp7.csproj'
)

function Clear-LocalFeed {
    Write-Host 'clear directory' -ForegroundColor Blue

    $GitignoreFileName = '.gitignore'
    Get-ChildItem -Path $LocalFeedDirectory -Exclude $GitignoreFileName | Remove-Item -Recurse

    Write-Host "     $LocalFeedDirectory"
}

function Clean-Output {
    Write-Host 'dotnet clean' -ForegroundColor Blue

    $output = dotnet clean $ProjectFile --configuration $BuildConfiguration

    $output | Select-Object -Last 7 | Select-Object -First 1 | Write-Host
}

function Build-Package {
    Write-Host 'dotnet pack' -ForegroundColor Blue

    $output = dotnet pack $ProjectFile --configuration $BuildConfiguration --version-suffix $VersionSuffix

    Write-Host '   ' -NoNewline
    $output | Select-Object -Last 1 | Write-Host
}

function Deploy-PackageLocally {
    Write-Host 'dotnet nuget push' -ForegroundColor Blue

    $output = dotnet nuget push $PackageFile --source $LocalFeedDirectory

    Write-Host '     ' -NoNewline
    $output | Select-Object -Last 1 | Write-Host
}

function Install-Package {
    Write-Host 'dotnet add package' -ForegroundColor Blue

    $VersionPrefix = Select-Xml -Path $PropertiesFile -XPath $VersionXPath | Select-Object -ExpandProperty Node | Select-Object -ExpandProperty InnerText
    $Version = "$VersionPrefix-$VersionSuffix"

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet add $ExampleProject package $ProjectName --package-directory $LocalFeedDirectory --source $LocalFeedDirectory --version $Version

        Write-Host '   - ' -NoNewline
        $output | Select-Object -Last 4 | Select-Object -First 1 | Write-Host
    }
}

function Uninstall-Package {
    Write-Host 'dotnet remove package' -ForegroundColor Blue

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet remove $ExampleProject package $ProjectName

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

Write-TestInvoke
Clean-Output
Build-Package
Deploy-PackageLocally
Install-Package
Wait-ManualTest
Uninstall-Package
Clear-LocalFeed
Write-TestComplete
