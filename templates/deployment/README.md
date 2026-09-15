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

## Solution XML normalization

Dataverse exports the children of several collections in a non-deterministic
order. Export twice with no changes and git still reports a large diff, which
makes "review before committing" advice nobody can follow.

`Export-Solution.ps1` runs `Normalize-SolutionXml.ps1` automatically after
unpack. You can also run it directly, and CI can assert it:

```powershell
./scripts/Normalize-SolutionXml.ps1            # normalize in place
./scripts/Normalize-SolutionXml.ps1 -Verify    # exit non-zero if not normalized
```

**Three rules, and they are absolute XPaths.** That is the safety property:
a rule fires only on a document whose root element matches, so
`/Entity/EntityInfo/entity/attributes` cannot stray into an `attributes`
element in some other document the way a `//attributes` wildcard would.

```
/ImportExportXml/SolutionManifest/MissingDependencies   sort MissingDependency
/EntityRelationships                                    sort EntityRelationship
/Entity/EntityInfo/entity/attributes                    sort attribute
```

Everything else is left exactly as exported, including things that merely look
sortable. Some solution XML ordering is positional: form layout, ribbon
definitions and sitemap ordering all change behaviour if reordered, and
`neverDescendInto` guards those regions as a second line of defence.

**Adding a rule is a real decision, not a configuration tweak.** Confirm
against a real export that the container exists with the expected keys, then
confirm with a round trip that the solution still imports.

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
