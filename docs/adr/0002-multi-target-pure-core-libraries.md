# ADR 0002: Multi-target the platform-independent Core libraries

**Status:** Proposed
**Date:** 2026-09-08

## Context

The plan of record defers `net8.0` to Phase 3 and builds Phase 1 on `net462`
alone, because Dataverse requires plug-in and custom workflow activity
assemblies to target .NET Framework.

Building `net462` on macOS works: the `Microsoft.NETFramework.ReferenceAssemblies`
package supplies the reference assemblies to compile against.

**Executing** `net462` on macOS does not work. There is no .NET Framework
runtime outside Windows. A `net462`-only test project cannot be run with
`dotnet test` on the primary development machine, which means Phase 1 would
produce libraries whose tests can only be run in CI.

## Decision

Multi-target `net462` plus the current .NET LTS for components with no platform dependency,
starting with `JMCL.Core.IocContainer`. Test projects multi-target to match.

Local development runs `dotnet test -f $(PortableTargetFramework)`. CI on a Windows agent runs both
target frameworks, so the framework the sandbox actually loads is still verified
before release.

This does not extend to anything referencing `Microsoft.CrmSdk.CoreAssemblies`.
Those stay `net462` until Phase 3 establishes what a modern Dataverse target
looks like.

## Consequences

- Tests are runnable locally from day one.
- Core libraries become consumable from Azure Functions and APIs earlier than
  planned, which was a Phase 3 goal anyway.
- CI must run on Windows to execute the `net462` leg. Compilation alone is not
  sufficient verification.
- Any component later found to have a hidden framework dependency drops back to
  `net462` only, and its tests become CI-only.

## Open question

Whether `Microsoft.CrmSdk.CoreAssemblies` ships a `netstandard2.0` asset. If it
does, more of the Dataverse layer can multi-target than assumed. Unverified:
nuget.org was not reachable from the environment where this was drafted.

## Addendum, 2026-09-08: the target framework floor

Checked against Microsoft sources on this date:

- "Build and package plug-in code" still states plug-in and custom workflow
  activity assemblies must target .NET Framework, with `net462` shown. .NET 8
  and .NET Core remain unsupported for plug-ins.
- "Supported customizations" states only that assemblies created with .NET
  Framework 4.6.2 are supported. No roadmap, no deprecation timeline.
- The Dataverse 2026 release wave 1 plan contains no plug-in runtime, .NET
  version, or sandbox modernization feature.
- .NET Framework 4.6.2 leaves support on 2027-01-13. Versions 4.7 through
  4.8.1 carry no published end date.

There is therefore no announced release plan for Dataverse plug-ins beyond
.NET Framework, and none should be assumed.

The 4.6.2 end-of-support date is less alarming than it looks. .NET Framework 4.x
is a single in-place-updated runtime on CLR 4; targeting `net462` selects a
compile-time reference surface, not a 4.6.2 runtime on the sandbox server, which
runs whatever the host OS carries. The cheap move available to Microsoft is to
raise the accepted target to `net472` or `net48`, not to port the sandbox.

Consequence for this repository: the plug-in target framework is a single
property, `PluginTargetFramework`, in `Directory.Build.props`. A platform-
mandated bump is one edit rather than one per project. Do not hardcode `net462`
in a project file.

## Addendum, 2026-09-08: portable target is net10.0, not net8.0

The first draft of this ADR specified `net8.0`. That was wrong on the day it was
written. Per Microsoft's .NET support policy, checked 2026-09-08:

| Version | Type | End of support |
|---|---|---|
| .NET 8 | LTS | 2026-11-10 |
| .NET 9 | STS | 2026-11-10 |
| .NET 10 | LTS | 2028-11-14 |

Targeting `net8.0` would have shipped a framework with roughly two months of
support remaining. `net10.0` is the current LTS and is what the development
machine already has installed (SDK 10.0.400, runtime 10.0.11), so it also
removes an install step rather than adding one.

The portable target is expressed as `PortableTargetFramework` in
`Directory.Build.props` for the same reason as `PluginTargetFramework`: rolling
to .NET 12 in November 2028 should be one edit.
