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
$SolutionFilterFile = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'F0.Analyzers.Core.slnf'

dotnet test $SolutionFilterFile --configuration $BuildConfiguration
