<#
.SYNOPSIS
    Pack a solution from source control and import it into an environment.

.DESCRIPTION
    Packs from the unpacked source, then imports. There is no Package Deployer
    step: solutions and configuration data both move through pac, so the
    compiled deployment package that the older tooling required has no job.
    See docs/adr/0007 in the framework repository.

.PARAMETER Environment
    Key from settings/environments.json. Connect first with Connect-Environment.ps1.

.PARAMETER Managed
    Import the managed build. Use for test and production; leave off for dev.

.PARAMETER Upgrade
    Stage and upgrade rather than a plain import. The managed upgrade path,
    which removes components deleted since the previous version.

.EXAMPLE
    ./Connect-Environment.ps1 -Environment test
    ./Import-Solution.ps1 -Environment test -Managed
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Environment,
    [switch] $Managed,
    [switch] $Upgrade
)

. "$PSScriptRoot/_common.ps1"
Assert-PacCli

$settings = Get-SolutionSettings
$root     = Get-RepoRoot
$url      = Get-EnvironmentUrl -Environment $Environment

$artifacts = New-DirectoryIfMissing (Join-Path $root $settings.paths.artifacts)
$source    = Join-Path $root $settings.paths.solutionSource
if (-not (Test-Path $source)) { throw "No unpacked solution at $source. Run Export-Solution.ps1 first." }

$type = if ($Managed) { 'Managed' } else { 'Unmanaged' }
$zip  = Join-Path $artifacts "$($settings.solutionName)_$($type.ToLower()).zip"

Write-Host "Packing $type from $source." -ForegroundColor Cyan
$pack = @('solution', 'pack', '--zipfile', $zip, '--folder', $source, '--packagetype', $type)
$mapping = Join-Path $root $settings.paths.mappingFile
if (Test-Path $mapping) { $pack += @('--map', $mapping) }
Invoke-Pac @pack

Write-Host "Importing to $Environment." -ForegroundColor Cyan
$import = @(
    'solution', 'import',
    '--path', $zip,
    '--environment', $url,
    '--max-async-wait-time', $settings.import.maxAsyncWaitMinutes,
    '--async'
)
if ($settings.import.activatePlugins) { $import += '--activate-plugins' }
if ($Upgrade)  { $import += '--stage-and-upgrade' }
# Publishing applies to unmanaged only; a managed import publishes itself.
if (-not $Managed -and $settings.import.publishChanges) { $import += '--publish-changes' }

Invoke-Pac @import

Write-Host "Imported $($settings.solutionName) ($type) to $Environment." -ForegroundColor Green
