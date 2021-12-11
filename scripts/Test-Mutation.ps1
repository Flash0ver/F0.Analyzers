#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [switch]$OpenReport
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$SolutionFilterFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.Core.slnf'
$TestProjectDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'test', 'F0.Analyzers.Tests'
$NuGetConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'nuget.config'
$StrykerConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'test', 'stryker-config.json'

dotnet clean $SolutionFilterFile

dotnet tool restore --configfile $NuGetConfigurationFile

Start-Process -FilePath 'dotnet' -ArgumentList "tool run dotnet-stryker --config-file $StrykerConfigurationFile" -WorkingDirectory $TestProjectDirectory -NoNewWindow -Wait

if ($OpenReport) {
    $ResultsDirectory = Join-Path -Path $TestProjectDirectory -ChildPath 'StrykerOutput'
    $LatestReportDirectory = Get-ChildItem -Path $ResultsDirectory -Directory | Sort-Object -Property Name -Descending | Select-Object -First 1
    $ReportFile = Join-Path -Path $LatestReportDirectory -ChildPath 'reports' -AdditionalChildPath 'mutation-report.html'
    Invoke-Item -Path $ReportFile
}
