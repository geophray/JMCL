# ADR 0005: Source distribution via contentFiles and generated nuspecs

**Status:** Proposed, unverified
**Date:** 2026-09-14

## Context

Some consumers must compile the framework INTO their own assembly rather than
reference a DLL:

- **Custom workflow activities.** Microsoft's plug-in packages (dependent
  assemblies) explicitly do not support them.
- **On-premises deployments.** Also unsupported by plug-in packages.

Online plug-in assemblies no longer need this, since plug-in packages cover
them, so the path is narrower than it was in 2020. It is not optional.

The upstream mechanism cannot be carried forward. Its nuspecs place sources
under `content/App_Packages/<Package>.<version>/`. The `content` folder is a
`packages.config` mechanism and is silently ignored under `PackageReference`:
the files ship inside the package and are never compiled. This is the same
finding recorded in the fork plan, section 5.1, and it is why converting the
project system and replacing the packaging are one change rather than two.

## Decision

Ship a `<PackageId>.Sources` package alongside every assembly package, using
`contentFiles` with `buildAction="Compile"`, generated from the component
manifest in `build/rename-upstream.py` by `build/pack-sources.py`.

**Dependencies of a `.Sources` package point at other `.Sources` packages,
never at the assembly packages.** Mixing the two would give a consumer both a
compiled source copy and a DLL copy of the same types, which is precisely the
`CS0436` duplicate-type failure this fork already hit once during the port.

## Why a generated nuspec

MSBuild offers no supported hook for rewriting pack dependencies, and the
dependency rewrite above is the whole point. A nuspec gives full control.

This partially walks back the fork plan's "all 44 hand-authored nuspecs go
away". The distinction that matters is hand-authored versus generated: these
are produced from the manifest, which remains the single source of truth, and
are gitignored build artifacts rather than checked-in files. The upstream's
defects came from hand maintenance, such as an assembly nuspec and a source
nuspec sharing one package id, and `CCLLCCdsSdkDatapdb` missing its dot.

## Verification

`samples/SourcePackageSmokeTest` is an outside consumer. It deliberately does
not inherit the repository `Directory.Build.props`, so it cannot succeed for
reasons a real consumer would not have. It references only
`JMCL.Dataverse.Sdk.Sources` and touches types that must arrive transitively,
and it promotes `CS0436` to an error so a duplicate type fails the build.

**Status is Proposed until that project restores and compiles.** Neither the
`contentFiles` path nor the generated nuspecs have been exercised end to end.

## Fallback

If `contentFiles` proves unreliable, the alternative is shipping a
`build/<PackageId>.targets` inside each package that globs the package's own
source folder into `@(Compile)`. More deterministic, less idiomatic, and it
does not rely on NuGet's content handling.


## Addendum, 2026-09-14: contentFiles are not transitive

Verified by `samples/SourcePackageSmokeTest`. Two findings.

**The mechanism works.** With the smoke test referencing
`JMCL.Dataverse.Sdk.Sources`, the compiler received all 55 source files, and the
errors it reported cited paths under
`~/.nuget/packages/jmcl.dataverse.sdk.sources/1.0.0/contentFiles/cs/any/`.
`contentFiles` with `buildAction="Compile"` does reach the compiler, nothing is
copied into the consuming project, and nothing needs committing or ignoring.
This is the decisive difference from the `packages.config` era, where
`.Sources` content was copied in at install time, was not restored on a fresh
clone, and therefore had to be committed.

**An earlier claim in this ADR was wrong.** contentFiles do NOT flow
transitively. `contentfiles` sits in NuGet's default suppressed-asset set for
any package that is not a direct `PackageReference`. Referencing only
`JMCL.Dataverse.Sdk.Sources` produced 48 `CS0246` errors for `IIocContainer`,
`ICache`, `IWebRequest`, `IRecordPointer<>` and friends: its dependencies
resolved, but their sources were never handed to the compiler.

**Consequence.** A consumer must reference every `.Sources` package in the
closure as a direct `PackageReference`, not just the entry point. This is not a
workaround; it is why the upstream's `SamplePlugin` lists nine `.Sources`
packages in its `packages.config` rather than one.

`build/pack-sources.py --closure <Component>` prints the required set, and each
generated nuspec carries that list in its package description so a consumer can
find it without reading this file.

The dependency entries in the nuspecs remain worthwhile: they pin versions and
document the graph, even though they cannot deliver the sources by themselves.

## Addendum 2, 2026-09-14: framework assemblies

The closure fix took the smoke test from 48 errors to 2. The remainder exposed a
second structural requirement of source distribution.

`JMCL.Core.ProcessModel` uses `System.Runtime.Caching`, which on `net462` is a
framework assembly reference rather than a package. The assembly project gets it
from `<Reference Include="System.Runtime.Caching" />`. A consumer compiling the
package's sources gets nothing, because a `.cs` file cannot carry a reference.

The nuspec `<frameworkAssemblies>` element is the mechanism, and the generator
now emits one per entry in the manifest's `framework_refs_plugin`. Affects
`Core.ProcessModel` (`System.Runtime.Caching`) and `Core.Net`,
`Azure.Authentication`, `Azure.Secrets` (`System.Net.Http`).

The general rule this establishes: **anything the assembly project needs beyond
its own sources has to be restated in the source package.** Project references
become `.Sources` dependencies, package references pass through, and framework
references become `frameworkAssemblies`. Miss any one and the consumer gets
errors pointing at files inside the NuGet cache.

## Addendum 3, 2026-09-15: pack the nuspec against a single-target shim

`dotnet pack -p:NuspecFile=<file>` against a **multi-targeted** project returns
exit code 0 and produces no package at all. Reproduced in isolation: the same
command against a single-`TargetFramework` project writes the package correctly;
switching only to `TargetFrameworks` breaks it silently.

Every JMCL library is multi-targeted or inherits a multi-targeted default, so
all eighteen source packs were silent no-ops. The failure surfaced two steps
later as `NU1101` on the consumer, which sent three separate debugging rounds
after the wrong thing.

`build/packaging/SourcePackShim` is now the pack vehicle: a single-target
project contributing no content, which deliberately does not inherit the
repository `Directory.Build.props`. The nuspec supplies every file and all
metadata, so the project's identity is irrelevant.

`build/packaging/SourcePackage.props` is deleted. It was the first sketch of
this, was never imported by any project, and would only mislead.

**The lesson worth keeping**: `build/pack-sources.py` now asserts the expected
`.nupkg` exists rather than trusting a zero exit code, and `build/verify.sh`
asserts 18 packages of each kind before touching the consumers. A packaging
step that can report success without producing a package is not a check.
