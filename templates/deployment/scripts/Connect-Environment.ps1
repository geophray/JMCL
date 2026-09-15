<#
.SYNOPSIS
    Authenticate to a Dataverse environment with the Power Platform CLI.

.DESCRIPTION
    Two paths, deliberately:

      Interactive (developers)   device code in a browser
      Service principal (CI)     application id, secret, tenant

    Both use MSAL. Nothing here touches ADAL, Microsoft.Xrm.Data.PowerShell,
    or CrmServiceClient. See docs/adr/0007 in the framework repository.

.PARAMETER Environment
    Key from settings/environments.json, for example dev, test, prod.

.PARAMETER ApplicationId
    Service principal application (client) id. Omit for interactive sign-in.

.PARAMETER ClientSecret
    Service principal secret. Pass from a secret store, never from a file in
    source control.

.EXAMPLE
    ./Connect-Environment.ps1 -Environment dev

.EXAMPLE
    ./Connect-Environment.ps1 -Environment prod `
        -ApplicationId $env:SP_APP_ID -ClientSecret $env:SP_SECRET -TenantId $env:SP_TENANT
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $Environment,
    [string] $ApplicationId,
    [string] $ClientSecret,
    [string] $TenantId
)

. "$PSScriptRoot/_common.ps1"
Assert-PacCli

$url         = Get-EnvironmentUrl -Environment $Environment
$profileName = "jmcl-$Environment"

# Recreate rather than reuse. A stale profile pointing at a previous URL is a
# genuinely dangerous failure mode: every later command silently targets the
# wrong environment.
pac auth delete --name $profileName 2>$null | Out-Null

if ($ApplicationId) {
    if (-not $ClientSecret) { throw "-ClientSecret is required when -ApplicationId is supplied." }
    if (-not $TenantId)     {
        $cfg = Get-Content (Join-Path (Get-RepoRoot) 'settings/environments.json') -Raw | ConvertFrom-Json
        $TenantId = $cfg.tenantId
    }
    if (-not $TenantId) { throw "No tenant id supplied and none found in settings/environments.json." }

    Write-Host "Authenticating to $Environment as a service principal." -ForegroundColor Cyan
    Invoke-Pac auth create --name $profileName --environment $url `
        --applicationId $ApplicationId --clientSecret $ClientSecret --tenant $TenantId
}
else {
    Write-Host "Authenticating to $Environment interactively. A browser window will open." -ForegroundColor Cyan
    Invoke-Pac auth create --name $profileName --environment $url --deviceCode
}

Invoke-Pac auth select --name $profileName
Write-Host "Connected to $Environment ($url)." -ForegroundColor Green
