<#
    Shared helpers. Dot-sourced by the other scripts; not run directly.

    Design note: these scripts are the contract, and the pipelines are thin
    callers. A developer on a laptop and a build agent run the same code, so a
    green pipeline is evidence about the thing being shipped rather than about
    a second implementation that happens to resemble it.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-RepoRoot {
    # Scripts live at <repo>/scripts, so the root is two levels up from here.
    return (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
}

function Get-SolutionSettings {
    $path = Join-Path (Get-RepoRoot) 'settings/solution.settings.json'
    if (-not (Test-Path $path)) {
        throw "Missing $path. Copy it from the deployment template and edit the values."
    }
    return Get-Content $path -Raw | ConvertFrom-Json
}

function Get-EnvironmentUrl {
    param([Parameter(Mandatory)][string] $Environment)

    $path = Join-Path (Get-RepoRoot) 'settings/environments.json'
    if (-not (Test-Path $path)) {
        throw "Missing $path. Copy settings/environments.example.json to settings/environments.json and fill in your environment URLs. It is gitignored."
    }

    $config = Get-Content $path -Raw | ConvertFrom-Json
    if (-not $config.environments.PSObject.Properties.Name.Contains($Environment)) {
        $known = ($config.environments.PSObject.Properties.Name) -join ', '
        throw "Unknown environment '$Environment'. Known environments: $known"
    }
    return $config.environments.$Environment.url
}

function Assert-PacCli {
    <#
        pac is a dotnet global tool. Checking here turns a confusing
        "command not found" halfway through a deployment into a clear message
        before anything has been touched.
    #>
    if (-not (Get-Command pac -ErrorAction SilentlyContinue)) {
        throw @"
The Power Platform CLI (pac) is not on PATH.

    dotnet tool install --global Microsoft.PowerApps.CLI.Tool

If it is installed, the dotnet tools directory may be missing from PATH:
    macOS/Linux  ~/.dotnet/tools
    Windows      %USERPROFILE%\.dotnet\tools
"@
    }
}

function Invoke-Pac {
    <#
        pac signals failure through the exit code. PowerShell does not throw on
        a non-zero exit code from a native command, so without this check a
        failed export would sail on and the next step would operate on a stale
        or missing file.
    #>
    param([Parameter(Mandatory, ValueFromRemainingArguments)][string[]] $Arguments)

    Write-Host "pac $($Arguments -join ' ')" -ForegroundColor DarkGray
    & pac @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "pac $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
    }
}

function New-DirectoryIfMissing {
    param([Parameter(Mandatory)][string] $Path)
    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path -Force | Out-Null
    }
    return (Resolve-Path $Path).Path
}
