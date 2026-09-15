# ADR 0004: Template provenance and the path back into scope

**Status:** Proposed
**Date:** 2026-09-14
**Supersedes the exclusion rationale in** the fork plan, section 8.4.

## Context

ADR 0001 excluded `solution_template` and `deployment_template` on the grounds
that they were multi-author the hosting organization work product with no license. Direct
review of two additional copies changes half of that.

**Two lineages exist for each template.**

Personal copies, `github.com/geophray`, private, one squashed commit each dated
2022-12-15 ("Clone of empty deployment template", "Clone of empty solution
template"). Entirely CCLLC-branded: `CCLLCImportExtension.cs`, zero `the hosting organization`
strings. These are pre-the hosting organization snapshots.

ADO copies, a private Azure DevOps collection:

- `deployment_template` begins 2021-02-08 with Scott Colson's own "Initial
  Commit", a 19-file squashed import. Consistent with him bringing an existing
  codebase in. 92 commits through 2026-09-08, with substantial contribution
  from other contributors.
- `solution_template` begins 2024-04-24 with Cody Peterson's "Copy of iris
  solution_template solution" and contains **no Colson commits at all**. It was
  re-imported from a client project. The 2022 personal copy predates it by
  sixteen months and is the cleaner ancestor.

**Neither template has ever carried a LICENSE.** `git log --all
--diff-filter=A` across the full ADO `deployment_template` history returns no
license file at any point. This is conspicuous rather than accidental: Colson
placed an explicit MIT LICENSE on all eleven library repositories. The absence
here is a gap in the grant, not evidence of one.

## What the hosting organization changed

`deployment_template`, 79 files to 86:

- New capability: `DevOps/Stage-PortalChange.ps1`,
  `DevOps/Upload-StagedPortalChanges.ps1`, `Sites/_site-config/
  PortalToolkit.Config.psd1`, `New-Project.ps1`, `docs/adr/0001-pac-cli-
  migration.md`.
- Import and export scripts moved off ADAL authentication.
- Mechanical: `CCLLCImportExtension.cs` to `the hosting organizationImportExtension.cs`; vendored
  `CCLLC.Cds.DevOps` PowerShell module replaced by a rebuilt
  `the hosting organization.Dataverse.DevOps`.

`solution_template`, 207 files to 141: predominantly removals. The committed
`App_Packages/CCLLC.CDS.Sdk.1.4.13/` vendored sources and the `WebResources/`
folder are gone. Added: an empty `CdsSolution/Other/Solution.xml` skeleton, a
`NuGet.config`, `Plugins/app.config`, and tokenized project references.

## Decision

Revise the exclusion. Build Phase 4 on the **2022 personal copies** as the base,
and reimplement the later deltas rather than merging them. This removes the
multi-author entanglement, since the 2022 base contains no the hosting organization-staff
contribution, and for `solution_template` it also avoids a base derived from a
client project.

This remains **Proposed, not Accepted**, pending the one thing it does not
resolve: the base is Colson's unlicensed work regardless of which copy is used.

## Blocking item

Ask Colson to confirm the templates carry the same MIT terms as the libraries.
This is the highest-value open item in the project: a single message closes the
only unresolved provenance question, and he licensed everything adjacent to it
that way already. Until answered, the templates stay out of the repository.

## Consequences

- Phase 4 becomes substantially cheaper: reimplementing the delta is hours, not
  a ground-up design. The portal staging scripts are the only real feature.
- Do not carry the vendored Microsoft assemblies forward
  (`Microsoft.Xrm.Sdk.dll`, `System.Management.Automation.dll`,
  `DocumentFormat.OpenXml.dll`). A `JMCL.Dataverse.DevOps` build replaces the
  module they support.
- If Colson declines or does not respond, fall back to ADR 0001: design the
  templates from first principles, informed by the delta above.
