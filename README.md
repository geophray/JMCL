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

All 18 runtime components have been forked, converted to SDK-style projects
and build cleanly: `dotnet build` succeeds with 0 errors, and 10 of the 18
multi-target `net462` plus `net10.0` while the remaining 8 Dataverse
components are `net462` only. Roughly 350 files and 20,000 lines of
production source, excluding tests, generated proxies and build output.

Two things are still open before this is proven end to end, and both are
tracked rather than blocking:

- **No sample plugin has been registered and run in a live Dataverse
  environment yet.** Everything so far is verified by `dotnet build` and
  `dotnet test`; nothing has executed inside Dataverse.
- **Test coverage is minimal.** Only `JMCL.Core.IocContainer` has tests (3
  test methods). The other 17 components, including `Dataverse.Sdk` and
  `Telemetry`, have none yet.

Not yet published anywhere. CI is configured to publish tagged releases to
nuget.org (see [Packaging](#packaging)), but no version has been tagged, so
nothing is live there today.

Three components remain outside the 18 above: `Dataverse.Testing` (needs
re-platforming off its current test harness), `Dataverse.DevOps.PowerShell`
(in scope, being ported), and `Dataverse.ProxyGenerator`, which is ported
under `tools/` with its authentication moved from ADAL to MSAL (see
[Proxy generation](#proxy-generation)). None of the 18 components above
depend on any of the three.

## Proxy generation

`tools/` holds the early-bound proxy generator: `JMCL.Dataverse.ProxyGenerator`
(the T4 engine) and `JMCL.Dataverse.ProxyGenerator.Cmd`, which packs as
**`JMCL.Dataverse.ProxyBuilder`**. It is a drop-in replacement for
`CCLLC.CDS.ProxyBuilder`, with the same package layout (`tools\proxybuilder.exe`,
plus `ProxyTemplate.t4`, `proxies.json` and `codgen\proxybuilder.bat` under
`content\`), the same `/s:` search-path argument, the same `proxies.json` /
`spkl.json` settings, and the same T4 directives. It connects through
`Microsoft.PowerPlatform.Dataverse.Client` (MSAL). Run it with no `/c:` to get
an interactive sign-in, with saved connections and a token cache under
`%APPDATA%\jmcl-proxygen`. Pass `/c:` with any `ServiceClient` connection string
for client secret, certificate or other non-interactive auth.

To migrate a solution built from the plugin template:

1. Replace the `CCLLC.CDS.ProxyBuilder` PackageReference with
   `JMCL.Dataverse.ProxyBuilder`. Remove the old one entirely: the batch file
   runs the first `proxybuilder.exe` it finds under the solution folder.
2. In the solution's `ProxyTemplate.t4`, change the generated base class from
   `CCLLC.CDS.Sdk.EarlyBound.EntityProxy` to
   `JMCL.Dataverse.Sdk.EarlyBound.EntityProxy`.

`codgen\proxybuilder.bat` itself does not change.

## Layout

```
src/        runtime libraries, one project per component
tests/      unit tests, one project per library
samples/    sample consumers used to verify packaging end to end
build/      shared MSBuild props, packaging scripts, signing key location
docs/adr/   architecture decision records
upstream/   reserved for vendor branch pins of the unmodified upstream;
            not yet populated
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

For local development:

```bash
dotnet pack -c Release -o local-feed
```

`local-feed` is a folder feed registered in `NuGet.config`.

For releases: pushing a `v*` tag to `main` runs the `publish` job in
[ci.yml](.github/workflows/ci.yml), which packs and pushes every package to
nuget.org. Publishing uses nuget.org's trusted publishing (OIDC) rather than a
stored API key: the job exchanges a short-lived GitHub Actions token for a
one-hour nuget.org key, gated on a trusted publishing policy scoped to
`JMCL.*` and a GitHub Environment named `nuget`. That environment needs a
`NUGET_USER` variable (the nuget.org profile name to authenticate as)
configured, and nothing publishes until the trusted publishing policy also
exists on nuget.org. Versions come from the tag itself via MinVer — see
[CONTRIBUTING.md](CONTRIBUTING.md).

## Strong naming

Standalone Dataverse plugin assemblies must be strong named; assemblies inside a
plugin package need not be. Signing is wired up but inactive: drop a key at
`build/JMCL.snk` and `Directory.Build.props` enables it automatically. Keys are
gitignored and must not be committed.

## License

MIT. See [LICENSE](LICENSE).
