#Requires -Version 6.0

[CmdletBinding()]
param (
    [Parameter(Mandatory=$false)]
    [switch]$ThrowOnError
)

Set-StrictMode -Version 3.0
$ErrorActionPreference = 'Stop'

$RepositoryRootPath = (Get-Item -Path $PSScriptRoot).Parent
$ReleaseTrackingDirectory = Join-Path -Path $RepositoryRootPath -ChildPath 'source' -AdditionalChildPath 'production', 'F0.Analyzers', 'ReleaseTracking'
$ReleaseTrackingShippedFile = Join-Path -Path $ReleaseTrackingDirectory -ChildPath 'AnalyzerReleases.Shipped.md'
$ReleaseTrackingUnshippedFile = Join-Path -Path $ReleaseTrackingDirectory -ChildPath 'AnalyzerReleases.Unshipped.md'

$ShippedJob = Start-Job -Name 'Shipped' -ScriptBlock { param($Path)
    $ReleaseTracking = Get-Content -Path $Path

    if ($ReleaseTracking -eq $null) {
        throw "$Path should not be empty."
    }
} -ArgumentList $ReleaseTrackingShippedFile

$UnshippedJob = Start-Job -Name 'Unshipped' -ScriptBlock { param($Path)
    $ReleaseTracking = Select-String -Path $Path -Pattern '^; ' -NotMatch

    if ($ReleaseTracking -ne $null) {
        throw "$Path should be empty."
    }
} -ArgumentList $ReleaseTrackingUnshippedFile

$Jobs = $ShippedJob, $UnshippedJob

$Message = [System.Text.StringBuilder]::new()
$Jobs | Wait-Job | Remove-Job
$Jobs | Where-Object -Property State -EQ -Value 'Failed' | ForEach-Object -Process {
    [void]$Message.Append("$($_.Name): ")
    [void]$Message.AppendLine($_.ChildJobs[0].JobStateInfo.Reason.Message)
}

if ($Message.Length -ne 0) {
    Write-Host $Message.ToString() -ForegroundColor Yellow

    if ($ThrowOnError) {
        throw 'Analyzer releases are not tracked correctly.'
    }

    Write-Host '❌ Analyzer releases are not tracked correctly.' -ForegroundColor Red
    exit 1
}

Write-Host '✓ Analyzer releases are tracked correctly.' -ForegroundColor Green
exit 0
