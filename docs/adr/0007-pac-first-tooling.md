# ADR 0007: pac-first tooling, and what a large private estate teaches

**Status:** Accepted
**Date:** 2026-09-15

## Context

A prior assessment of the private Power Platform estate (ADR-0001 in that
organization's `deployment_template`, dated 2026-09-01, authored by this fork's
maintainer) inventoried 24 solution repositories plus a shared library repo and
found ADAL, retired December 2022, structurally embedded in all of them.

That assessment is about migrating an existing estate. This fork is new, so the
useful move is not to replay the migration but to start at its destination. The
findings below are recorded as facts observed about the upstream ecosystem;
no scripts, schema files or code from those repositories are used here.

## What the assessment found

**ADAL sits in four places**, not one: the `Adoxio.Dynamics.DevOps` PowerShell
chain (unmaintained since November 2022, funnelling through `Get-CrmConnection`
into an ADAL-backed `CrmServiceClient`); a compiled cmdlet that bundles the ADAL
DLL directly; the Package Deployer path via
`Microsoft.CrmSdk.XrmTooling.PackageDeployment`; and the CI pipeline template,
which rebuilds the whole ADAL-era toolchain on every agent run.

**Version drift is the symptom that proves it is structural**, not a stale pin:
ADAL appears at 3.19.8, 5.2.8, 5.2.9 and 5.3.0, and
`Microsoft.CrmSdk.XrmTooling.CoreAssembly` at seven distinct versions from
9.1.0.46 to 9.1.1.65. "Just bump the package" would mean 20+ baselines with no
compatibility guarantee at any of them.

**`pac` already works in that codebase.** The Power Pages step in `Export.ps1`
authenticates through `pac` with MSAL today, which is existence proof rather
than speculation.

**Exactly one operation has no CLI equivalent**: importing document templates.
Everything else maps onto `pac solution` and `pac data`.

## Decisions for this fork

1. **The deployment and solution templates are pac-first from the first line.**
   No ADAL, no `Adoxio.Dynamics.DevOps`, no `Microsoft.Xrm.Data.PowerShell`, no
   `CrmServiceClient`. Where a client library is needed, it is
   `Microsoft.PowerPlatform.Dataverse.Client`, the MSAL-based successor.

2. **This retroactively justifies two exclusions.** `Dataverse.ProxyGenerator`
   and `Dataverse.DevOps.PowerShell` were left unported because they carry
   deprecated ADAL and XrmTooling dependencies. The assessment confirms those
   are not incidental dependencies to be bumped; they are the rot itself.

3. **`JMCL.Dataverse.DevOps.PowerShell`, if it is ever built, is small.**
   Document template import is the only operation `pac` does not cover. A
   component scoped to that, built on `Microsoft.PowerPlatform.Dataverse.Client`,
   is a fraction of the 448 lines the upstream carries.

4. **Package Deployer is a candidate for elimination, not a requirement.** Once
   solutions and configuration data both move through `pac`, the compiled
   `DeploymentPackage.csproj` and its `ImportExtension` subclass may have no job
   left. Treat it as a stretch goal and design the template so it is not
   load-bearing.

5. **Service principal auth is a Phase 4 prerequisite, not an afterthought.**
   `pac auth create` against a pipeline needs an Entra app registration and an
   Application User with an appropriate role in every target environment. That
   is environment-owner coordination, so it has a lead time that script work
   does not.

## Two smaller observations from the upstream build files

**`AddXmlLanguage.targets` is deliberately not carried forward.** It is a
`CodeTaskFactory` task that rewrites generated XML documentation to inject
`xml:lang="en"`, and its own comment admits the requirement's origin is unknown.
It only runs on Windows under full MSBuild, which is why it needed a fix when
the upstream moved to the dotnet SDK. This fork builds clean without it.

**Code analysis was dropped without a replacement, and that is a real gap.**
The upstream sets `RunCodeAnalysis=true` with a `CustomDictionary.xml`, which is
legacy FxCop. The modern successor is the built-in .NET analyzers
(`EnableNETAnalyzers`, `AnalysisLevel`). Enabling them on 20,912 lines of ported
code will produce a large warning count, so this is deferred rather than
forgotten: do it alongside the unit test work, where the warnings can be
triaged against tests that prove behavior rather than guessed at.
