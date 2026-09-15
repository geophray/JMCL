#!/usr/bin/env python3
"""
Port a component from the upstream Colson repositories into this fork.

Why this exists
---------------
Renaming namespaces is what normally makes a fork permanently un-mergeable with
its upstream. This script keeps the rename a deterministic, repeatable
TRANSFORM rather than a one-time hand edit. When upstream changes, re-run it
against the updated source and diff the output: conflicts then appear only
where this fork genuinely diverged, not everywhere it was merely renamed.

Treat this as a maintained artifact. Do not hand-edit ported sources in ways the
script cannot reproduce without recording why in the component manifest.

Python rather than PowerShell because it is present on macOS, Linux and Windows
without an install, and because this is string and file manipulation rather than
object pipeline work.

Usage
-----
    python3 build/rename-upstream.py --source ~/Repos/pp-itt
    python3 build/rename-upstream.py --source ~/Repos/pp-itt --only Core.ProcessModel
    python3 build/rename-upstream.py --source ~/Repos/pp-itt --dry-run
"""

import argparse
import pathlib
import re
import shutil
import sys

def detect_upstream_prefix(source_root):
    """Work out the upstream namespace root from the clone directory names.

    Colson's repositories are named <Root>.<Component>, where <Root> is the
    namespace root his sources use. Detecting it means this script carries no
    reference to any particular upstream organization: point --source at a set
    of clones and it reads the prefix off them. Override with
    --upstream-prefix if the directories are named differently from the
    namespaces inside them.
    """
    from collections import Counter
    counts = Counter(
        d.name.split(".", 1)[0]
        for d in source_root.iterdir()
        if d.is_dir() and "." in d.name and not d.name.startswith(".")
    )
    if not counts:
        raise SystemExit(
            f"No <Root>.<Component> directories under {source_root}.\n"
            "Point --source at the upstream clones, or pass --upstream-prefix."
        )
    return counts.most_common(1)[0][0]


# --------------------------------------------------------------------------
# Identifier rewrites, applied longest-first so the specific cases win.
# `up` is the upstream namespace root, detected from the clone directories.
# --------------------------------------------------------------------------
def build_renames(up):
    return [
        # Upstream spelled this RESTClient; normalize to match every other component.
        (f"{up}.Core.RESTClient", "JMCL.Core.RestClient"),
        ("CCLLC.Core.RESTClient", "JMCL.Core.RestClient"),

        # Drop the retired "CDS" branding in favour of Dataverse.
        (f"{up}.Dataverse", "JMCL.Dataverse"),
        (f"{up}.CDS", "JMCL.Dataverse"),
        ("CCLLC.CDS", "JMCL.Dataverse"),

        # Straight root swap.
        (f"{up}.", "JMCL."),
        ("CCLLC.", "JMCL."),

        # Non-dotted forms the root swap misses. These are NOT namespaces:
        #   <root>_decryptthis         a Dataverse column logical name
        #   <root>_/configuration/...  a Dataverse web resource path
        #   "<root>:0.1.1-100"         an Application Insights SdkVersion string
        # The underscore forms carry a publisher prefix, which belongs to
        # whoever deploys the solution rather than to this framework. jmcl_ is
        # a neutral default, not a correct one: see docs/adr/0008.
        (f"{up}_", "jmcl_"),
        ("CCLLC_", "jmcl_"),
        (f"{up}:", "JMCL:"),
        ("CCLLC:", "JMCL:"),
    ]


# Files that exist only to serve the legacy project system.
SKIP_NAMES = {"AssemblyInfo.cs"}
# Never READ these from upstream. App_Packages holds vendored copies of other
# components' sources; this fork uses real ProjectReferences instead.
SKIP_DIRS = {"obj", "bin", ".git", ".vs", "Properties", "App_Packages"}

# Never TOUCH these in the destination. Deliberately smaller than SKIP_DIRS:
# a directory we stop porting must still be cleaned up, or stale copies linger.
PRESERVE_DIRS = {"obj", "bin", ".git", ".vs"}

CSPROJ_TEMPLATE = """<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>{tfms}</TargetFrameworks>
    <RootNamespace>{root_namespace}</RootNamespace>
    <AssemblyName>{name}</AssemblyName>
    <PackageId>{name}</PackageId>
    <Title>{name}</Title>
    <Description>{description}</Description>
    <PackageTags>{tags}</PackageTags>
  </PropertyGroup>
{project_refs}{package_refs}
</Project>
"""

# --------------------------------------------------------------------------
# Component manifest.
#
# "sources" are paths relative to --source. Multiple entries are merged into
# one project, which is how the upstream shared projects collapse into ordinary
# class libraries.
# --------------------------------------------------------------------------
COMPONENTS = {
    # ---------------- Core: portable, no platform dependency ----------------
    "Core.IocContainer": {
        "sources": ["{UP}.Core.IocContainer/{UP}.Core.IocContainer"],
        "root_namespace": "JMCL.Core",
        "description": "A lightweight inversion of control container that runs inside the Dataverse plugin sandbox.",
        "tags": "dataverse;powerplatform;ioc;dependency-injection",
        "project_refs": [],
    },
    "Core.ProcessModel": {
        "sources": ["{UP}.Core.Libraries/{UP}.Core/SharedProjects/ProcessModel"],
        "root_namespace": "JMCL.Core",
        "description": "Platform-agnostic process execution context for business logic that must run identically in a Dataverse plug-in, an Azure Function, or a test harness.",
        "tags": "dataverse;powerplatform;process-model;abstraction",
        "project_refs": ["JMCL.Core.IocContainer"],
        "package_refs_portable": ["System.Runtime.Caching"],
        "framework_refs_plugin": ["System.Runtime.Caching"],
    },
    "Core.Serialization": {
        "sources": ["{UP}.Core.Libraries/{UP}.Core/SharedProjects/Serialization"],
        "root_namespace": "JMCL.Core.Serialization",
        "description": "Standardized JSON and XML serialization over DataContract serializers.",
        "tags": "serialization;json;xml;datacontract",
        "project_refs": [],
    },
    "Core.Net": {
        "sources": ["{UP}.Core.Net/{UP}.Core.Net"],
        "root_namespace": "JMCL.Core.Net",
        "description": "Testable HTTP request abstraction. Wraps the platform web request so calling code can be unit tested against a mock.",
        "tags": "http;webrequest;testability",
        "project_refs": [],
        "framework_refs_plugin": ["System.Net.Http"],
    },
    "Core.RestClient": {
        "sources": ["{UP}.Core.Libraries/{UP}.Core/SharedProjects/RestClient"],
        "root_namespace": "JMCL.Core.RestClient",
        "description": "POCO based REST client built on the Net and Serialization abstractions.",
        "tags": "rest;http;client",
        "project_refs": ["JMCL.Core.Net", "JMCL.Core.Serialization"],
    },
    "Core.Encryption": {
        # D7: audit pending. SHA256 is a hash, not encryption; the name may be wrong.
        # Carried because Dataverse.FieldEncryption depends on it.
        "sources": ["{UP}.Core.Libraries/{UP}.Core/SharedProjects/Encryption"],
        "root_namespace": "JMCL.Core",
        "description": "SHA256 hashing helper. Name inherited from upstream and under review.",
        "tags": "hashing;sha256",
        "project_refs": [],
    },
    "Telemetry": {
        "sources": ["{UP}.Core.Libraries/{UP}.Telemetry/SharedProjects/Telemetry"],
        "root_namespace": "JMCL.Telemetry",
        "description": "Sandbox-safe telemetry that is wire compatible with Application Insights, usable from inside the Dataverse plug-in sandbox where the Application Insights SDK cannot load.",
        "tags": "telemetry;applicationinsights;observability;dataverse",
        "project_refs": [],
    },
    "Core.Net.Instrumented": {
        "sources": ["{UP}.Core.Net/{UP}.Core.Net.Instrumented"],
        "root_namespace": "JMCL.Core.Net",
        "description": "Net abstraction decorated with dependency telemetry capture.",
        "tags": "http;telemetry;instrumentation",
        "project_refs": ["JMCL.Core.Net", "JMCL.Telemetry"],
    },
    "Azure.Authentication": {
        "sources": ["{UP}.Azure.Authentication/{UP}.Azure.Authentication"],
        "root_namespace": "JMCL.Azure.Authentication",
        "description": "OAuth2 authentication, including V2 flows, for reaching Azure services from constrained hosts.",
        "tags": "azure;oauth2;authentication",
        "project_refs": ["JMCL.Core.Net", "JMCL.Core.RestClient", "JMCL.Core.Serialization"],
        "package_refs_plugin": ["System.Text.Json"],
        "framework_refs_plugin": ["System.Net.Http"],
    },
    "Azure.Secrets": {
        "sources": ["{UP}.Azure.Secrets/{UP}.Azure.Secrets"],
        "root_namespace": "JMCL.Azure.Secrets",
        "description": "Secret retrieval for constrained hosts.",
        "tags": "azure;secrets;keyvault",
        "project_refs": ["JMCL.Core.IocContainer", "JMCL.Core.ProcessModel", "JMCL.Core.Net",
                         "JMCL.Core.RestClient", "JMCL.Core.Serialization"],
        "framework_refs_plugin": ["System.Net.Http"],
    },

    # ------- Dataverse: plugin target only, CrmSdk ships no modern asset -------
    "Dataverse.Data": {
        "sources": ["{UP}.Dataverse.Sdk.Data/{UP}.Dataverse.Sdk.Data"],
        "root_namespace": "JMCL.Dataverse.Sdk.Data",
        "description": "Fluent Dataverse query builder and entity proxies.",
        "tags": "dataverse;powerplatform;query;fluent",
        "tfms": "plugin",
        "project_refs": ["JMCL.Core.IocContainer", "JMCL.Core.ProcessModel"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies"],
    },
    "Dataverse.Search": {
        "sources": ["{UP}.Dataverse.Sdk.SearchUtilities/{UP}.Dataverse.Sdk.SearchUtilities"],
        "root_namespace": "JMCL.Dataverse.Sdk.Utilities.Search",
        "description": "Dataverse search helpers.",
        "tags": "dataverse;powerplatform;search",
        "tfms": "plugin",
        "project_refs": ["JMCL.Core.IocContainer", "JMCL.Core.ProcessModel",
                         "JMCL.Dataverse.Sdk", "JMCL.Dataverse.Data"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies"],
    },
    "Dataverse.Sdk": {
        "sources": ["{UP}.Dataverse.Sdk/{UP}.Dataverse.Sdk"],
        "root_namespace": "JMCL.Dataverse.Sdk",
        "description": "Dataverse plug-in framework: typed event registration, execution context, enhanced organization service, environment variable settings.",
        "tags": "dataverse;powerplatform;plugin;framework",
        "tfms": "plugin",
        "project_refs": ["JMCL.Core.IocContainer", "JMCL.Core.ProcessModel", "JMCL.Core.Net",
                         "JMCL.Dataverse.Data"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies", "System.Text.Json"],
    },
    "Dataverse.Metadata": {
        # D7: review pending, may be superseded by the platform.
        "sources": ["{UP}.Core.Libraries/{UP}.CDS/SharedProjects/{UP}.Dataverse.Sdk.Metadata"],
        "root_namespace": "JMCL.Dataverse.Sdk.Metadata",
        "description": "Dataverse metadata proxies and message metadata.",
        "tags": "dataverse;powerplatform;metadata",
        "tfms": "plugin",
        "project_refs": ["JMCL.Dataverse.Sdk"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies"],
    },
    "Dataverse.Sdk.Instrumented": {
        "sources": ["{UP}.Dataverse.Sdk/{UP}.Dataverse.Sdk.Instrumented"],
        "root_namespace": "JMCL.Dataverse.Sdk",
        "description": "Dataverse plug-in base with external telemetry capture.",
        "tags": "dataverse;powerplatform;plugin;telemetry",
        "tfms": "plugin",
        "project_refs": ["JMCL.Dataverse.Sdk", "JMCL.Telemetry", "JMCL.Core.Net.Instrumented"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies"],
    },
    "Dataverse.Workflow": {
        "sources": ["{UP}.Dataverse.Sdk/{UP}.Dataverse.Sdk.Workflow"],
        "root_namespace": "JMCL.Dataverse.Sdk.Workflow",
        "description": "Custom workflow activity framework built on the Dataverse SDK abstractions.",
        "tags": "dataverse;powerplatform;workflow",
        "tfms": "plugin",
        "project_refs": ["JMCL.Dataverse.Sdk"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies", "Microsoft.CrmSdk.Workflow"],
    },
    "Dataverse.Workflow.Instrumented": {
        "sources": ["{UP}.Dataverse.Sdk/{UP}.Dataverse.Sdk.Workflow.Instrumented"],
        "root_namespace": "JMCL.Dataverse.Sdk.Workflow",
        "description": "Custom workflow activity base with external telemetry capture.",
        "tags": "dataverse;powerplatform;workflow;telemetry",
        "tfms": "plugin",
        "project_refs": ["JMCL.Dataverse.Workflow", "JMCL.Dataverse.Sdk.Instrumented",
                         "JMCL.Telemetry", "JMCL.Core.Net.Instrumented"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies", "Microsoft.CrmSdk.Workflow"],
    },
    "Dataverse.FieldEncryption": {
        # Adopt only after the platform column-level-security overlap check.
        "sources": ["{UP}.Dataverse.FieldEncryption/{UP}.Dataverse.FieldEncryption"],
        "root_namespace": "JMCL.Dataverse.FieldEncryption",
        "description": "Field level encryption for Dataverse columns.",
        "tags": "dataverse;powerplatform;encryption",
        "tfms": "plugin",
        "project_refs": ["JMCL.Core.Encryption", "JMCL.Core.IocContainer", "JMCL.Core.ProcessModel",
                         "JMCL.Core.Net", "JMCL.Core.RestClient", "JMCL.Core.Serialization",
                         "JMCL.Azure.Secrets", "JMCL.Dataverse.Sdk", "JMCL.Dataverse.Data"],
        "package_refs": ["Microsoft.CrmSdk.CoreAssemblies"],
    },
}


def rewrite(text, renames):
    for old, new in renames:
        text = text.replace(old, new)
    return text


def strip_bom_and_normalize(text):
    if text.startswith("﻿"):
        text = text[1:]
    # Upstream files are a mix of CRLF and LF; normalize so diffs are meaningful.
    return text.replace("\r\n", "\n")


def port(name, spec, source_root, repo_root, dry_run, up, renames):
    dest = repo_root / "src" / f"JMCL.{name}"
    full = f"JMCL.{name}"
    copied = []

    for rel in spec["sources"]:
        src = source_root / rel.format(UP=up)
        if not src.is_dir():
            print(f"  MISSING source: {src}", file=sys.stderr)
            return None
        for f in sorted(src.rglob("*.cs")):
            if f.name in SKIP_NAMES or set(f.parts) & SKIP_DIRS:
                continue
            target = dest / f.relative_to(src)
            body = rewrite(strip_bom_and_normalize(f.read_text(encoding="utf-8-sig")), renames)
            copied.append((target, body))

    if not copied:
        print(f"  no sources found for {full}", file=sys.stderr)
        return None

    refs = "".join(
        f'    <ProjectReference Include="..\\{r}\\{r}.csproj" />\n'
        for r in spec.get("project_refs", [])
    )
    project_refs = f"\n  <ItemGroup>\n{refs}  </ItemGroup>\n" if refs else ""

    pkg = ""
    for p in spec.get("package_refs", []):
        pkg += f'\n  <ItemGroup>\n    <PackageReference Include="{p}" />\n  </ItemGroup>\n'
    for p in spec.get("package_refs_plugin", []):
        pkg += (
            f'\n  <ItemGroup Condition="\'$(TargetFramework)\' == '
            f'\'$(PluginTargetFramework)\'">\n'
            f'    <PackageReference Include="{p}" />\n  </ItemGroup>\n'
        )
    for p in spec.get("package_refs_portable", []):
        pkg += (
            f'\n  <ItemGroup Condition="\'$(TargetFramework)\' == '
            f'\'$(PortableTargetFramework)\'">\n'
            f'    <PackageReference Include="{p}" />\n  </ItemGroup>\n'
        )
    for p in spec.get("framework_refs_plugin", []):
        pkg += (
            f'\n  <ItemGroup Condition="\'$(TargetFramework)\' == '
            f'\'$(PluginTargetFramework)\'">\n'
            f'    <Reference Include="{p}" />\n  </ItemGroup>\n'
        )

    tfms = ("$(PluginTargetFramework)" if spec.get("tfms") == "plugin"
            else "$(PluginTargetFramework);$(PortableTargetFramework)")
    csproj = CSPROJ_TEMPLATE.format(
        tfms=tfms,
        root_namespace=spec["root_namespace"],
        name=full,
        description=spec["description"],
        tags=spec["tags"],
        project_refs=project_refs,
        package_refs=pkg,
    )

    print(f"  {full}: {len(copied)} files -> src/{full}")
    if dry_run:
        return full

    # Idempotent for the files this script owns: .cs sources and the csproj.
    # Build output (obj/, bin/) is left alone, and deletion failures are not
    # fatal, because some environments mount the tree without delete rights.
    # Orphans are reported rather than silently left behind; git will show them.
    written = {dest / f"{full}.csproj"} | {t for t, _ in copied}
    orphans = []
    if dest.exists():
        for existing in dest.rglob("*"):
            if not existing.is_file():
                continue
            if set(existing.relative_to(dest).parts) & PRESERVE_DIRS:
                continue
            if existing.suffix not in {".cs", ".csproj"} or existing in written:
                continue
            try:
                existing.unlink()
            except OSError:
                orphans.append(existing.relative_to(repo_root))

    for target, body in copied:
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(body, encoding="utf-8")
    (dest / f"{full}.csproj").write_text(csproj, encoding="utf-8")

    for o in orphans:
        print(f"    ORPHAN (could not remove, delete by hand): {o}", file=sys.stderr)
    return full


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--source", required=True, help="Root holding the upstream clones")
    ap.add_argument("--only", action="append", help="Port only these components")
    ap.add_argument("--dry-run", action="store_true")
    ap.add_argument("--upstream-prefix", default=None,
                    help="Upstream namespace root. Detected from the clone "
                         "directory names when omitted.")
    args = ap.parse_args()

    source_root = pathlib.Path(args.source).expanduser().resolve()
    repo_root = pathlib.Path(__file__).resolve().parent.parent

    up = args.upstream_prefix or detect_upstream_prefix(source_root)
    renames = build_renames(up)

    names = args.only or list(COMPONENTS)
    print(f"source: {source_root}\ntarget: {repo_root}\nupstream root: {up}\n")
    ported = []
    for n in names:
        if n not in COMPONENTS:
            print(f"unknown component: {n}", file=sys.stderr)
            return 2
        r = port(n, COMPONENTS[n], source_root, repo_root, args.dry_run, up, renames)
        if r:
            ported.append(r)

    print(f"\nported {len(ported)} component(s)")
    if ported and not args.dry_run:
        print("\nAdd to the solution with:")
        for p in ported:
            print(f"  dotnet sln add src/{p}/{p}.csproj")
    return 0


if __name__ == "__main__":
    sys.exit(main())
