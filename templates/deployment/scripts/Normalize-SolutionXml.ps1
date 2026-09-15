<#
.SYNOPSIS
    Put unpacked solution XML into a deterministic order so diffs are reviewable.

.DESCRIPTION
    Dataverse exports the children of several collections in a non-deterministic
    order. Export twice with no changes and git still reports a large diff. That
    makes "review the diff before committing" advice nobody can follow, and it
    removes most of the value of source-controlling a solution at all.

    This sorts the children of an ALLOWLIST of containers, configured in
    settings/normalization.json. The allowlist matters: some solution XML
    ordering is positional and meaningful. Form layout, ribbon definitions and
    sitemap ordering all change behaviour if reordered. Anything not named in
    the configuration is left exactly as exported.

    Whitespace is preserved. Element children are reordered within the slots
    they already occupy, so indentation and line endings survive and the diff
    shows only the reordering.

    The operation is idempotent: running it twice changes nothing the second
    time, which is what -Verify relies on.

.PARAMETER Path
    Folder of unpacked solution XML. Defaults to the solutionSource path in
    settings/solution.settings.json.

.PARAMETER Verify
    Do not write. Exit non-zero if any file would change. Use in CI to catch
    un-normalized XML before it reaches main, rather than discovering it in the
    next unreadable pull request.

.EXAMPLE
    ./Normalize-SolutionXml.ps1

.EXAMPLE
    ./Normalize-SolutionXml.ps1 -Verify
#>
[CmdletBinding()]
param(
    [string] $Path,
    [switch] $Verify
)

. "$PSScriptRoot/_common.ps1"

$root = Get-RepoRoot
if (-not $Path) {
    $settings = Get-SolutionSettings
    $Path = Join-Path $root $settings.paths.solutionSource
}
if (-not (Test-Path $Path)) { throw "No solution source at $Path." }

$configPath = Join-Path $root 'settings/normalization.json'
if (-not (Test-Path $configPath)) { throw "Missing $configPath." }
$config = Get-Content $configPath -Raw | ConvertFrom-Json

function Get-SortKey {
    <#
        Build a comparable string for one element. Keys are tried in order;
        the first that resolves wins. An element matching no key falls back to
        its own XML, which is arbitrary but stable, so the result is still
        deterministic rather than random.
    #>
    param($Element, [string[]] $Keys)

    foreach ($key in $Keys) {
        if ($key.StartsWith('@')) {
            $value = $Element.GetAttribute($key.Substring(1))
        }
        else {
            $child = $Element.SelectSingleNode($key)
            $value = if ($child) { $child.InnerText } else { $null }
        }
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            # Lowercase so ordering does not depend on how the platform cased
            # a name this time.
            return $value.ToLowerInvariant()
        }
    }
    return $Element.OuterXml
}

function Test-InsideProtectedNode {
    <#
        Walk up from a container to make sure it is not inside a region whose
        ordering is meaningful. A rule can legitimately match a container
        nested inside FormXml, and sorting there would change the form.
    #>
    param($Node, [string[]] $Protected)

    $current = $Node.ParentNode
    while ($current -and $current.NodeType -eq 'Element') {
        if ($Protected -contains $current.Name) { return $true }
        $current = $current.ParentNode
    }
    return $false
}

$changed = @()
$files = Get-ChildItem -Path $Path -Filter *.xml -Recurse -File

foreach ($file in $files) {
    $document = New-Object System.Xml.XmlDocument
    $document.PreserveWhitespace = $true
    try { $document.Load($file.FullName) }
    catch { Write-Warning "Skipping $($file.FullName): $($_.Exception.Message)"; continue }

    $before = $document.OuterXml
    $sortedContainers = 0

    foreach ($rule in $config.rules) {
        foreach ($container in @($document.SelectNodes($rule.container))) {
            if (Test-InsideProtectedNode -Node $container -Protected $config.neverDescendInto) { continue }

            $elements = @($container.ChildNodes | Where-Object {
                $_.NodeType -eq 'Element' -and $_.Name -eq $rule.child
            })
            if ($elements.Count -lt 2) { continue }

            $ordered = $elements | Sort-Object -Property @{ Expression = { Get-SortKey -Element $_ -Keys $rule.keys } }

            # Detach copies first, then swap each original for the copy that
            # belongs in its position. Reordering in place this way leaves the
            # surrounding whitespace nodes untouched.
            $clones = @($ordered | ForEach-Object { $_.CloneNode($true) })
            for ($i = 0; $i -lt $elements.Count; $i++) {
                [void]$container.ReplaceChild($clones[$i], $elements[$i])
            }
            $sortedContainers++
        }
    }

    if ($document.OuterXml -ne $before) {
        $changed += $file.FullName.Substring($root.Length + 1)
        if (-not $Verify) {
            $document.Save($file.FullName)
        }
    }
}

Write-Host "Scanned $($files.Count) XML file(s)."

if ($changed.Count -eq 0) {
    Write-Host "Already normalized." -ForegroundColor Green
    exit 0
}

foreach ($f in $changed) { Write-Host "  $f" -ForegroundColor Yellow }

if ($Verify) {
    Write-Host ""
    Write-Host "$($changed.Count) file(s) are not normalized. Run Normalize-SolutionXml.ps1 and commit the result." -ForegroundColor Red
    exit 1
}

Write-Host "Normalized $($changed.Count) file(s)." -ForegroundColor Green
