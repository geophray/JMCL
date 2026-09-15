<#
.SYNOPSIS
    Export a solution from an environment and unpack it into source control.

.DESCRIPTION
    Exports both managed and unmanaged in one pass and unpacks them to the same
    folder, so the two never drift apart in source control.

    The exported .zip is a build artifact and is not committed. The unpacked
    folder is the source of truth, because a zip produces a one-line diff on
    every change and tells a reviewer nothing.

.PARAMETER Environment
    Key from settings/environments.json. Connect first with Connect-Environment.ps1.

.EXAMPLE
    ./Connect-Environment.ps1 -Environment dev
    ./Export-Solution.ps1 -Environment dev
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Environment
)

. "$PSScriptRoot/_common.ps1"
Assert-PacCli

$settings = Get-SolutionSettings
$root     = Get-RepoRoot
$url      = Get-EnvironmentUrl -Environment $Environment

$artifacts = New-DirectoryIfMissing (Join-Path $root $settings.paths.artifacts)
$source    = New-DirectoryIfMissing (Join-Path $root $settings.paths.solutionSource)
$zip       = Join-Path $artifacts "$($settings.solutionName).zip"
$zipManaged = Join-Path $artifacts "$($settings.solutionName)_managed.zip"

Write-Host "Exporting $($settings.solutionName) from $Environment." -ForegroundColor Cyan

Invoke-Pac solution export --name $settings.solutionName --path $zip `
    --environment $url --include $settings.export.includeSettings --overwrite

Invoke-Pac solution export --name $settings.solutionName --path $zipManaged `
    --environment $url --include $settings.export.includeSettings --managed --overwrite

# Unpack unmanaged and managed into one tree. --allowDelete lets components
# removed from the solution disappear from source control; without it a deleted
# component lingers forever and eventually gets re-imported by accident.
$unpack = @(
    'solution', 'unpack',
    '--zipfile', $zip,
    '--folder', $source,
    '--packagetype', $settings.export.packageType,
    '--allowDelete', '--allowWrite', '--clobber'
)
$mapping = Join-Path $root $settings.paths.mappingFile
if (Test-Path $mapping) { $unpack += @('--map', $mapping) }

Invoke-Pac @unpack

Write-Host "Unpacked to $source. Review with git diff before committing." -ForegroundColor Green
