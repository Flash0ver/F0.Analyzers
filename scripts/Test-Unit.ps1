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
$SolutionFilterFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.Core.slnf'

dotnet test $SolutionFilterFile --configuration $BuildConfiguration
