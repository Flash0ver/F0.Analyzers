#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [switch]$OpenReport
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$SolutionFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.sln'
$SolutionFilterFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.Core.slnf'
$TestProjectFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'test', 'F0.Analyzers.Tests', 'F0.Analyzers.Tests.csproj'
$NuGetConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'nuget.config'
$StrykerConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'test', 'stryker-config.json'

dotnet clean $SolutionFilterFile

dotnet tool restore --configfile $NuGetConfigurationFile

$TestProjectFile = $($TestProjectFile.Replace('\', '/'))
dotnet tool run dotnet-stryker --solution-path $SolutionFile --test-projects "['$TestProjectFile']" --config-file-path $StrykerConfigurationFile

if ($OpenReport) {
    $ResultsDirectory = Join-Path -Path $PSScriptRoot -ChildPath 'StrykerOutput'
    $LatestReportDirectory = Get-ChildItem -Path $ResultsDirectory -Directory | Sort-Object -Property Name -Descending | Select-Object -First 1
    $ReportFile = Join-Path -Path $LatestReportDirectory -ChildPath 'reports' -AdditionalChildPath 'mutation-report.html'
    Invoke-Item -Path $ReportFile
}
