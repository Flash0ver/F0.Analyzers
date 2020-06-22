#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter()]
    [switch]$ThrowOnError
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$PublicApiDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'production', 'F0.Analyzers', 'PublicApi'
$PublicApiShippedFile = Join-Path -Path $PublicApiDirectory -ChildPath 'PublicAPI.Shipped.txt'
$PublicApiUnshippedFile = Join-Path -Path $PublicApiDirectory -ChildPath 'PublicAPI.Unshipped.txt'

$Message = [System.Text.StringBuilder]::new()
$PublicApiShippedFile, $PublicApiUnshippedFile | ForEach-Object {
    $PublicApi = Get-Content -Path $_

    if ($PublicApi -ne $null) {
        [void]$Message.AppendLine($_)
        [void]$Message.AppendLine($PublicApi)
    }
}

if ($Message.Length -ne 0) {
    Write-Host $Message.ToString() -ForegroundColor Yellow

    if ($ThrowOnError) {
        throw 'Analyzers should not declare a public API.'
    }

    Write-Host 'Analyzers should not declare a public API.' -ForegroundColor Red
    exit 1
}

exit 0
