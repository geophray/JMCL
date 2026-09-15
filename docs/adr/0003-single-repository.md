# ADR 0003: One repository, flat src layout

**Status:** Accepted
**Date:** 2026-09-14

## Context

The upstream exists in two shapes, and it is worth being precise about why,
because this fork's shape reverses one of them.

The commit SHAs establish that the private Azure DevOps repositories are literal
clones of Scott Colson's own git repositories, not restructured copies:

| Component | GitHub head | ADO last Colson commit |
|---|---|---|
| CCLLCCodeLibraries / the hosting organization.Core.Libraries | b477916 | b477916 |
| CCLLC.CDS.Sdk / the hosting organization.Dataverse.Sdk | 88162a6 | 88162a6 |
| CCLLC.CDS.Sdk.Data / the hosting organization.Dataverse.Sdk.Data | 98d65ff | 98d65ff |

Git SHAs hash the full ancestry, so identical heads mean identical history. The
ADO logs also carry GitHub fingerprints: "Merge pull request #N from
ScottColson/...", a jekyll-theme-cayman commit, and a merge from
github.com/ScottColson/CCLLC.Azure.Secrets, a repository that is no longer
public. GitHub shows three repositories because that is the subset Colson
published; ADO holds the whole constellation.

So the monorepo-to-many-repos difference is Colson's own evolution, not an
organizational preference imposed later.

Why he moved, in rough order of weight:

1. **Per-repo CI/CD with independent versioning.** Each split repository carries
   its own azure-pipelines.yml triggering on release/*. The monorepo forced
   hand-managed cross-package pinning, and it visibly drifted: Core.ProcessModel
   1.1.5 pinning IocContainer.Sources 2.0.1, CDS.Sdk depending on CDS.Sdk.Data
   at 1.1.9 in one repository and 1.3.0.2 in another.
2. **Decoupled release cadence.** Core and Telemetry froze in November 2020
   while the CDS SDK kept moving through September 2023.
3. **Some components were never in the monorepo.** Azure.Secrets and
   FieldEncryption have first commits from April 2020, predating its head.

The split also abandoned the shared-project model: 14 of the 17 .shproj files
across all upstream repositories are in the monorepo. Shared projects exist only
to serve dual distribution, where SharedProjects/X is the source of truth and
LibraryProjects/X builds the DLL from it. Once a repository holds exactly one
package, that ceremony buys nothing.

## Decision

One repository, with a flat `src/<PackageName>/` layout.

## Rationale

The forces that broke the monorepo were tooling limitations that no longer
apply:

- Cross-package version skew is what `Directory.Packages.props` exists to
  prevent.
- Hand-managed inter-package pinning becomes `ProjectReference`, a conclusion
  this fork's author reached independently during the March 2026 the hosting organization pass.
- Per-package pipelines become one pipeline that packs every project, each
  carrying its own version.

The shared-project layout is explicitly not reproduced. `src/<PackageName>/` is
the split repositories' flat shape inside a single repository, which takes the
better half of each arrangement.

## Consequences

- One clone, one restore, one build. Cross-component refactoring is a single
  commit rather than a coordinated multi-repo release.
- Everything releases together unless per-project versioning is deliberately
  maintained. This is the cost Colson was escaping, and it returns here.
  Revisit if Core stays frozen while the Dataverse SDK churns.
- The vendor-branch and rename-script mechanism in the plan must map eleven
  upstream repositories into one tree. The component manifest in
  build/rename-upstream.py is where that mapping lives.
