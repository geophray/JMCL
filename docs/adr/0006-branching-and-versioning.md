# ADR 0006: Trunk-based branching with tag-driven versioning

**Status:** Accepted
**Date:** 2026-09-14

## Context

The first sixteen commits went straight onto `master` with no review surface
and no release mechanism. Two questions were entangled: how work reaches the
trunk, and where a package version comes from. The second one decides the first.

The upstream shows both answers tried. Colson's earlier `CCLLC.CDS.Sdk`
pipeline triggered on `release/*`, so a release branch governed the build. The
later the later upstream pipelines triggered on `main` and took the version from the
pipeline build counter: `name: '$(Major).$(Minor).$(rev:r)'` with
`versioningScheme: byBuildNumber`.

The build-counter approach is what produced the version skew this fork had to
reconcile. `Core.ProcessModel` 1.1.5 pinned `IocContainer.Sources` 2.0.1 while
another repository disagreed, because each pipeline owned an independent
counter and nothing in source recorded the truth. The pipelines also set
`allowPackageConflicts: true`, so re-pushing an existing version succeeded
silently instead of failing.

## Decision

**Versions come from git tags, via MinVer.** Tag `v1.2.0` and every package
builds as `1.2.0`. An untagged commit gets a deterministic prerelease such as
`1.2.1-alpha.0.5`.

**Branching is trunk-based.** Short-lived feature branches, a pull request into
`main`, `main` always green and always releasable. A tag cuts a release.

**No release branches.** A tag already fixes the point to build from, which is
the service a release branch provides. Release branches additionally create a
backporting obligation, and there is one maintainer.

`master` is renamed to `main` to match the upstream convention and the CI
configuration.

## Consequences

- A version is reproducible from any clone. `git describe` and the package
  version cannot disagree.
- The same version cannot be published twice by accident, and CI pushes without
  `--skip-duplicate` so a duplicate fails loudly rather than silently.
- MinVer requires full git history. CI checkouts must use `fetch-depth: 0`, or
  the version is silently wrong.
- `build/pack-sources.py` reads the version from MSBuild rather than a flag, so
  source packages and assembly packages cannot drift apart.
- Per-package independent versioning, which ADR 0003 flagged as the cost of a
  single repository, stays available: a scoped tag prefix per component can be
  introduced later without changing the model.
- If a client engagement ever mandates GitFlow, this does not prevent it. It is
  a deliberate choice not to pay for machinery nobody here needs yet.

## Release procedure

```
git switch -c feature/<name>          # work
git push -u origin feature/<name>     # open a PR into main
                                      # merge once CI is green
git switch main && git pull
git tag v1.0.0 && git push origin v1.0.0
```

The tag triggers the publish job. Nothing else does.
