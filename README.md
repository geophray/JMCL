# JMCL Power Platform Code Libraries

A modern, opinionated code framework for Power Platform and Dataverse solution
development: a platform-agnostic process model, a sandbox-safe IoC container, a
declarative plugin event registration model, and telemetry that reaches
Application Insights from inside the plugin sandbox.

## Lineage

This framework began as a fork of the Power Platform code libraries created by
**Scott Colson**, licensed under the MIT License. It has been renamed,
restructured and modernized: SDK-style projects, central package management,
`dotnet pack` in place of hand-authored nuspecs, and current dependencies.

The architectural ideas are Colson's. See [NOTICE](NOTICE) for the full
attribution, the upstream repositories, and the commits forked from.

Not affiliated with or endorsed by Colson Code, LLC.

## Status

Early. Phase 1 of the fork is in progress. Only `JMCL.Core.IocContainer` has
been converted so far. Nothing here is published to a public feed.

## Layout

```
src/        runtime libraries
tests/      unit tests, one project per library
tools/      build-time and deploy-time tooling (proxy generator, PowerShell)
build/      shared MSBuild props, packaging, signing key location
docs/adr/   architecture decision records
upstream/   vendor branch pins of the unmodified upstream
```

## Building

Requires the .NET SDK 10.0 or later. `net462` compiles on macOS and Linux via the
`Microsoft.NETFramework.ReferenceAssemblies` package, which is wired into
`Directory.Build.props`.

```bash
dotnet restore
dotnet build
```

## Testing

`net462` tests can only be **executed** on Windows, since there is no .NET
Framework runtime elsewhere. On macOS or Linux, run the `net10.0` leg:

```bash
dotnet test -f net10.0
```

CI runs both target frameworks on a Windows agent. See
[ADR 0002](docs/adr/0002-multi-target-pure-core-libraries.md).

## Packaging

```bash
dotnet pack -c Release -o local-feed
```

`local-feed` is a folder feed registered in `NuGet.config`, used while the
package identities are still settling. No public feed yet.

## Strong naming

Standalone Dataverse plugin assemblies must be strong named; assemblies inside a
plugin package need not be. Signing is wired up but inactive: drop a key at
`build/JMCL.snk` and `Directory.Build.props` enables it automatically. Keys are
gitignored and must not be committed.

## License

MIT. See [LICENSE](LICENSE).
