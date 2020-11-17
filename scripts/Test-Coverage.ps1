#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter()]
    [switch]$OpenReport
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$TestResultsDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'TestResults'
$SolutionFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.sln'
$RunsettingsFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'test', 'coverlet.runsettings'
$NuGetConfigurationFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'nuget.config'
$CoverageReportsGlob = Join-Path -Path $TestResultsDirectory -ChildPath '**' -AdditionalChildPath 'coverage.cobertura.xml'
$ReportTargetDirectory = Join-Path -Path $TestResultsDirectory -ChildPath 'coveragereport'
$ReportTypes = 'HtmlInline_AzurePipelines_Dark'
$ReportTargetFile = Join-Path -Path $ReportTargetDirectory -ChildPath 'index.html'

if (Test-Path -Path $TestResultsDirectory) {
    Remove-Item -Path $TestResultsDirectory -Recurse
}

dotnet clean $SolutionFile
dotnet test $SolutionFile --collect:"XPlat Code Coverage" --results-directory $TestResultsDirectory --settings $RunsettingsFile

dotnet tool restore --configfile $NuGetConfigurationFile
dotnet tool run reportgenerator "-reports:$CoverageReportsGlob" "-targetdir:$ReportTargetDirectory" -reporttypes:$ReportTypes

if ($OpenReport) {
    Invoke-Item $ReportTargetFile
}
