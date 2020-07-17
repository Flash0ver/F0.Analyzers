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
$ProjectDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'production', 'F0.Analyzers'
$ProjectFile = Join-Path -Path $ProjectDirectory -ChildPath 'F0.Analyzers.csproj'
$PackageFile = Join-Path -Path $ProjectDirectory -ChildPath 'bin' -AdditionalChildPath $BuildConfiguration, '*.nupkg'
$LocalFeedName = 'local-feed'
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

function Clean-Output {
    Write-Host 'dotnet clean' -ForegroundColor Blue

    $output = dotnet clean $ProjectFile --configuration $BuildConfiguration

    $output | Select-Object -Last 7 | Select-Object -First 1 | Write-Host
}

function Build-Package {
    Write-Host 'dotnet pack' -ForegroundColor Blue

    $output = dotnet pack $ProjectFile --configuration $BuildConfiguration

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

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet add $ExampleProject package F0.Analyzers --source $LocalFeedDirectory --package-directory $LocalFeedDirectory

        Write-Host '   - ' -NoNewline
        $output | Select-Object -Last 4 | Select-Object -First 1 | Write-Host
    }
}

function Uninstall-Package {
    Write-Host 'dotnet remove package' -ForegroundColor Blue

    foreach ($ExampleProject in $ExampleProjects) {
        $output = dotnet remove $ExampleProject package F0.Analyzers

        Write-Host '   - ' -NoNewline
        Write-Host $output
    }
}

function Wait-ManualTest {
    Write-Host 'Setup completed successfully. You may now test the Analyzers locally via the Examples. When finished, press Enter/Return to continue with Teardown . . .' -ForegroundColor Yellow

    $Input = Read-Host
}

function Write-TestComplete {
    Write-Host 'Manual local test completed.' -ForegroundColor Green
}

Clean-Output
Build-Package
Add-LocalFeed
New-GitignoreFile
Deploy-PackageLocally
Install-Package
Wait-ManualTest
Uninstall-Package
Remove-LocalFeed
Write-TestComplete
