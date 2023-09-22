#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [string]$Filter
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$SolutionFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.Core.slnf'
$BenchmarkProjectFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'performance', 'F0.Analyzers.Benchmarks', 'F0.Analyzers.Benchmarks.csproj'

$Configuration = 'Release'
$Framework = 'net7.0'

dotnet clean $SolutionFile

if ($Filter) {
    dotnet run --project $BenchmarkProjectFile --configuration $Configuration --framework $Framework --filter $Filter
}
else {
    dotnet run --project $BenchmarkProjectFile --configuration $Configuration --framework $Framework
}
