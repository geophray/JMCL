# JMCL deployment template

A solution deployment setup for Dataverse, built on the Power Platform CLI.

Copy this directory into a new solution repository. The scripts are the
contract; pipelines are thin callers, so a laptop and a build agent run
identical code.

## What this deliberately does not use

No ADAL, no `Adoxio.Dynamics.DevOps`, no `Microsoft.Xrm.Data.PowerShell`, no
`CrmServiceClient`, and no Package Deployer.

ADAL left support in December 2022 and the PowerShell modules built on it have
had no release since November 2022, so there is no newer version to move to.
Package Deployer exists to bundle solutions and data into a compiled package;
once both move through `pac`, it has no job.

**That last claim is scope-dependent and worth stating honestly.** A prior
migration of a large estate could not eliminate Package Deployer, because two
operations have no `pac` equivalent: importing document templates, and
resetting an environment. If a solution needs either, Package Deployer or a
compiled cmdlet comes back. This template drops it because nothing here needs
those operations yet, not because they have gone away. Reasoning is recorded in
`docs/adr/0007-pac-first-tooling.md` in the framework repository.

## Prerequisites

```bash
dotnet tool install --global Microsoft.PowerApps.CLI.Tool
```

PowerShell 7 or later. On macOS: `brew install --cask powershell`. Preinstalled
on Windows build agents.

For pipelines, an Entra app registration added as an Application User with an
appropriate security role in each target environment. That needs environment
owner coordination, so start it before you need it.

## Setup

```bash
cp settings/environments.example.json settings/environments.json
# edit the URLs and tenant id; this file is gitignored
```

Then edit `settings/solution.settings.json` for the solution name, publisher
prefix and paths.

## Daily use

```powershell
# once per session
./scripts/Connect-Environment.ps1 -Environment dev

# pull changes made in the maker portal into source control
./scripts/Export-Solution.ps1 -Environment dev
git diff                                  # review before committing

# push source control into an environment
./scripts/Import-Solution.ps1 -Environment dev
./scripts/Import-Solution.ps1 -Environment test -Managed
./scripts/Import-Solution.ps1 -Environment prod -Managed -Upgrade
```

## In a pipeline

```powershell
./scripts/Connect-Environment.ps1 -Environment test `
    -ApplicationId $env:SP_APP_ID `
    -ClientSecret  $env:SP_SECRET `
    -TenantId      $env:SP_TENANT
./scripts/Import-Solution.ps1 -Environment test -Managed
```

Secrets come from the pipeline's secret store as parameters. They are never
written to a settings file.

## Choices worth knowing

**The unpacked folder is the source of truth, not the zip.** A committed zip
produces a one-line diff on every change and tells a reviewer nothing. The zip
is a build artifact.

**Export writes both managed and unmanaged.** Unpacking both from one export
keeps them in step; exporting them separately lets them drift.

**Unpack runs with `--allowDelete`.** Without it, a component removed from the
solution lingers in source control forever and eventually gets re-imported by
accident.

**Every `pac` call checks its exit code.** PowerShell does not throw on a
non-zero exit from a native command, so without the check a failed export would
sail on and the next step would pack a stale file.

**Auth profiles are recreated, never reused.** A stale profile pointing at a
previous URL means every later command silently targets the wrong environment.

## Not here yet

Configuration and test data (`pac data export` / `pac data import`, using the
same Configuration Migration Tool schema format), pipeline definitions, and
solution version stamping via `pac solution online-version`.
