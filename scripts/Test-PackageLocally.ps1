#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [Alias('c','configuration')]
    [ValidateSet('Debug', 'Release')]
    [string]$BuildConfiguration = 'Debug'
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$ProjectName = 'F0.Analyzers'
$NuGetConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'nuget.config'
$ProjectDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'package', 'F0.Analyzers.Package'
$ProjectFile = Join-Path -Path $ProjectDirectory -ChildPath 'F0.Analyzers.Package.csproj'
$PackageFile = Join-Path -Path $ProjectDirectory -ChildPath 'bin' -AdditionalChildPath $BuildConfiguration, '*.nupkg'
$PropertiesFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'Release.props'
$VersionXPath = '/Project/PropertyGroup/F0Version'
$VersionSuffix = 'local'
$LocalFeedName = 'local-feed'
$PackageSourcePath = "./$LocalFeedName"
$LocalFeedDirectory = Join-Path -Path $RepositoryRootPath -ChildPath $LocalFeedName
$ExampleDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'example'
$ExampleProjects = @(
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example' -AdditionalChildPath 'F0.Analyzers.Example.csproj'
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example.CSharp2' -AdditionalChildPath 'F0.Analyzers.Example.CSharp2.csproj'
    Join-Path -Path $ExampleDirectory -ChildPath 'F0.Analyzers.Example.CSharp7' -AdditionalChildPath 'F0.Analyzers.Example.CSharp7.csproj'
)

function Add-LocalFeed {
    Write-Host 'create directory' -ForegroundColor Blue

    $output = New-Item -Path $RepositoryRootPath -Name $LocalFeedName -ItemType 'directory'

    Write-Host '     ' -NoNewline
    $output | Select-Object -Last 1 | Write-Host
}

function New-GitignoreFile {
    Write-Host 'create gitignore' -ForegroundColor Blue

    $GitignoreFileName = '.gitignore'
    $GitignoreFileContent = "*$([Environment]::NewLine)!$GitignoreFileName$([Environment]::NewLine)"
    $output = New-Item -Path $LocalFeedDirectory -Name $GitignoreFileName -ItemType 'file' -Value $GitignoreFileContent

    Write-Host '     ' -NoNewline
    $output | Select-Object -Last 1 | Write-Host
}

function Remove-LocalFeed {
    Write-Host 'delete directory' -ForegroundColor Blue

    Remove-Item -Path $LocalFeedDirectory -Recurse

    Write-Host "     $LocalFeedDirectory"
}

function Add-PackageSource {
    Write-Host 'dotnet nuget add source' -ForegroundColor Blue

    $output = dotnet nuget add source $PackageSourcePath --configfile $NuGetConfigurationFile --name $LocalFeedName

    Write-Host "     $output"
}

function Remove-PackageSource {
    Write-Host 'dotnet nuget remove source' -ForegroundColor Blue
    
    $output = dotnet nuget remove source $LocalFeedName --configfile $NuGetConfigurationFile

    Write-Host "     $output"
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

function Write-Version {
    $output = dotnet --version

    Write-Host "     .NET SDK $output"
}

Write-TestInvoke
Write-Version
Clean-Output
Build-Package
Add-LocalFeed
New-GitignoreFile
Deploy-PackageLocally
Add-PackageSource
Install-Package
Wait-ManualTest
Uninstall-Package
Remove-PackageSource
Remove-LocalFeed
Write-TestComplete
