#!/usr/bin/env python3
"""
Generate and pack the .Sources packages.

Why these exist
---------------
A Dataverse custom workflow activity cannot use plug-in packages (dependent
assemblies), and neither can an on-premises deployment. Those consumers must
compile the framework INTO their own assembly, which means consuming .cs files
rather than a DLL. Online plug-in assemblies can now use plug-in packages
instead, so this path is narrower than it was in 2020, but it is not optional.

Why not the upstream mechanism
------------------------------
Upstream nuspecs place sources under content/App_Packages/<Package>.<version>/.
"content" is a packages.config mechanism and is silently ignored under
PackageReference: the files arrive in the package and are never compiled. The
PackageReference equivalent is contentFiles with buildAction="Compile", which
also flows transitively, unlike content.

Why a generated nuspec rather than dotnet pack properties
---------------------------------------------------------
A .Sources package must depend on other .Sources packages. If it depended on the
assembly packages, a consumer would get the source copy AND the DLL copy of the
same types, which is exactly the CS0436 duplicate-type failure this fork already
hit once. MSBuild has no supported hook for rewriting pack dependencies, so the
nuspec is generated from the component manifest instead. Generated, not
hand-authored: the manifest stays the single source of truth.

Usage
-----
    python3 build/pack-sources.py --version 1.0.0            # generate + pack
    python3 build/pack-sources.py --generate-only            # nuspecs only
"""

import argparse
import pathlib
import subprocess
import sys

sys.path.insert(0, str(pathlib.Path(__file__).resolve().parent))
from importlib import import_module

rename = import_module("rename-upstream".replace("-", "_")) if False else None

# The manifest lives in rename-upstream.py. Import it by file path because the
# module name contains a hyphen.
import importlib.util

_spec = importlib.util.spec_from_file_location(
    "rename_upstream", pathlib.Path(__file__).resolve().parent / "rename-upstream.py"
)
_mod = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_mod)
COMPONENTS = _mod.COMPONENTS

REPO = pathlib.Path(__file__).resolve().parent.parent

NUSPEC = """<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd">
  <metadata minClientVersion="3.3.0">
    <id>{pkg}</id>
    <version>{version}</version>
    <title>{pkg}</title>
    <authors>Jeff Maughan</authors>
    <owners>Jeff Maughan</owners>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <license type="expression">MIT</license>
    <copyright>Copyright (c) 2026 Jeff Maughan. Portions copyright (c) 2020 Scott Colson.</copyright>
    <description>{description}

Source distribution. The .cs files are compiled into the consuming assembly, for
Dataverse custom workflow activities and on-premises deployments, which cannot
use plug-in packages. For every other consumer, reference {assembly_pkg} instead.

IMPORTANT: NuGet does not flow contentFiles transitively. Reference ALL of these
packages directly, not just this one:
{closure}</description>
    <tags>{tags} sources</tags>
    <dependencies>
{deps}    </dependencies>
{framework_assemblies}    <contentFiles>
      <files include="cs/any/{folder}/**/*.cs" buildAction="Compile" copyToOutput="false" flatten="false" />
    </contentFiles>
  </metadata>
  <files>
    <file src="src/{folder}/**/*.cs"
          target="contentFiles/cs/any/{folder}/"
          exclude="**/obj/**/*.cs;**/bin/**/*.cs" />
  </files>
</package>
"""


def msbuild_version():
    """Ask MSBuild for the version MinVer computed, so the source packages
    cannot drift from the assembly packages."""
    proj = REPO / "src" / "JMCL.Core.IocContainer" / "JMCL.Core.IocContainer.csproj"
    r = subprocess.run(
        # -t:MinVer is load-bearing. MinVer computes the version in a target;
        # evaluating the property alone returns the SDK default of 1.0.0.
        ["dotnet", "msbuild", str(proj), "-t:MinVer", "-getProperty:Version", "-nologo"],
        capture_output=True, text=True)
    v = r.stdout.strip().splitlines()[-1].strip() if r.returncode == 0 and r.stdout.strip() else ""
    if not v:
        raise SystemExit(
            "Could not read the version from MSBuild. Pass --version explicitly.\n"
            + (r.stdout + r.stderr).strip()[-500:])
    return v


def closure(name, seen=None):
    """Every .Sources package a consumer must reference directly to embed `name`.

    contentFiles are not transitive, so the consumer needs the whole closure as
    direct PackageReference items.
    """
    seen = seen if seen is not None else []
    full = f"JMCL.{name}"
    if full in seen:
        return seen
    seen.append(full)
    for ref in COMPONENTS[name].get("project_refs", []):
        closure(ref[len("JMCL."):], seen)
    return seen


def build_nuspec(name, spec, version):
    full = f"JMCL.{name}"
    pkg = f"{full}.Sources"
    deps = ""
    # Project references become .Sources dependencies. Never the assembly package:
    # mixing the two gives the consumer duplicate types.
    for r in spec.get("project_refs", []):
        deps += f'      <dependency id="{r}.Sources" version="{version}" />\n'
    # Real NuGet dependencies pass through unchanged.
    for p in spec.get("package_refs", []) + spec.get("package_refs_plugin", []):
        deps += f'      <dependency id="{p}" version="{_pkg_version(p)}" />\n'
    required = closure(name)
    listing = "\n".join(f"  {c}.Sources" for c in required)
    fw = ""
    for a in spec.get("framework_refs_plugin", []):
        fw += f'      <frameworkAssembly assemblyName="{a}" targetFramework="net462" />\n'
    fw = f"    <frameworkAssemblies>\n{fw}    </frameworkAssemblies>\n" if fw else ""

    return pkg, NUSPEC.format(
        framework_assemblies=fw,
        closure=listing,
        pkg=pkg,
        version=version,
        description=spec["description"],
        assembly_pkg=full,
        tags=spec["tags"].replace(";", " "),
        folder=full,
        deps=deps,
    )


_VERSIONS = None


def _pkg_version(pkg_id):
    """Read the pinned version out of Directory.Packages.props."""
    global _VERSIONS
    if _VERSIONS is None:
        import re
        text = (REPO / "Directory.Packages.props").read_text()
        _VERSIONS = dict(
            re.findall(r'PackageVersion Include="([^"]+)" Version="([^"]+)"', text)
        )
    if pkg_id not in _VERSIONS:
        raise SystemExit(f"No pinned version for {pkg_id} in Directory.Packages.props")
    return _VERSIONS[pkg_id]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--version", default=None,
                    help="Defaults to the MinVer-computed version, so source and "
                         "assembly packages always agree.")
    ap.add_argument("--output", default="local-feed")
    ap.add_argument("--generate-only", action="store_true")
    ap.add_argument("--closure", metavar="COMPONENT",
                    help="Print the .Sources packages a consumer must reference directly")
    args = ap.parse_args()

    if args.closure:
        name = args.closure[len("JMCL."):] if args.closure.startswith("JMCL.") else args.closure
        if name not in COMPONENTS:
            raise SystemExit(f"unknown component: {args.closure}")
        for c in closure(name):
            print(f"{c}.Sources")
        return 0

    out_nuspec = REPO / "artifacts" / "nuspec"
    out_nuspec.mkdir(parents=True, exist_ok=True)

    generated = []
    for name, spec in COMPONENTS.items():
        pkg, body = build_nuspec(name, spec, args.version)
        path = out_nuspec / f"{pkg}.nuspec"
        path.write_text(body, encoding="utf-8")
        generated.append((name, pkg, path))
        print(f"  {pkg}")

    print(f"\ngenerated {len(generated)} nuspec(s) in artifacts/nuspec")
    if args.generate_only:
        return 0

    failed = []
    for name, pkg, path in generated:
        # Pack against the single-target shim, never the real project.
        # "dotnet pack -p:NuspecFile=..." on a multi-targeted project exits 0
        # and produces nothing. See build/packaging/SourcePackShim.
        proj = REPO / "build" / "packaging" / "SourcePackShim" / "SourcePackShim.csproj"
        cmd = [
            "dotnet", "pack", str(proj),
            "-c", "Release",
            f"-p:NuspecFile={path}",
            f"-p:NuspecBasePath={REPO}",
            "-p:IncludeBuildOutput=false",
            "-p:IncludeSymbols=false",
            "-o", str(REPO / args.output),
            "--nologo", "-v", "m",
        ]
        r = subprocess.run(cmd, capture_output=True, text=True)
        expected = REPO / args.output / f"{pkg}.{args.version}.nupkg"

        # A zero exit code is not evidence. An earlier version of this script
        # reported "ok" eighteen times while producing no packages at all,
        # and the failure only surfaced two steps later as a restore error.
        if r.returncode != 0 or not expected.is_file():
            failed.append(pkg)
            why = "non-zero exit" if r.returncode != 0 else "no package produced"
            print(f"  pack {pkg}: FAILED ({why})")
            print(f"    expected: {expected}")
            print((r.stdout + r.stderr).strip()[-1200:])
        else:
            print(f"  pack {pkg}: ok  ({expected.stat().st_size:,} bytes)")

    print(f"\npacked {len(generated) - len(failed)}/{len(generated)}")
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
