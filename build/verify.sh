#!/usr/bin/env bash
# Full local verification loop: build, test, pack both package kinds, then prove
# both consumption paths work against the packed output.
#
# Run this rather than hand-typing the steps. The cache clear in particular is
# not optional: NuGet caches by id AND version, so without it you silently test
# whichever build got there first.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "=== 1. BUILD ==="
dotnet build -c Release

echo "=== 2. TEST ==="
# The net462 leg cannot execute off Windows; CI covers it.
if [[ "$(uname -s)" == "Darwin" || "$(uname -s)" == "Linux" ]]; then
  dotnet test -c Release --no-build -f net10.0
else
  dotnet test -c Release --no-build
fi

echo "=== 3. PACK ASSEMBLIES ==="
rm -f local-feed/*.nupkg local-feed/*.snupkg
dotnet pack -c Release -o local-feed

# Derive the version from what was actually produced rather than asking
# MSBuild. MinVer computes Version in a target, so -getProperty:Version without
# -t:MinVer silently returns the SDK default 1.0.0, which is how an earlier run
# packed 0.0.0-alpha.0.16 and then asked for 1.0.0.
PROBE="$(ls local-feed/JMCL.Core.IocContainer.*.nupkg | head -1)"
VERSION="$(basename "${PROBE}" .nupkg | sed 's/^JMCL\.Core\.IocContainer\.//')"
echo "=== version under test: ${VERSION} ==="

echo "=== 4. PACK SOURCES ==="
python3 build/pack-sources.py --output local-feed --version "${VERSION}"

ASM="$(ls local-feed/*.nupkg | grep -vc '\.Sources\.')"
SRC="$(ls local-feed/*.nupkg | grep -c '\.Sources\.')"
echo "--- produced: ${ASM} assembly, ${SRC} source ---"
if [[ "${ASM}" -ne 19 || "${SRC}" -ne 18 ]]; then
  echo "EXPECTED 19 assembly (18 original + JMCL.Dataverse.ProxyGenerator) and 18 source. Packing silently produced the wrong set." >&2
  exit 1
fi

echo "=== 5. CLEAR CACHE ==="
# Otherwise a stale package with the same id and version wins.
rm -rf ~/.nuget/packages/jmcl.* 2>/dev/null || true

echo "=== 6. ASSEMBLY PATH (plugin packages) ==="
dotnet build samples/SamplePlugin/SamplePlugin.csproj -c Release -p:JmclVersion="${VERSION}"

echo "=== 7. SOURCE PATH (workflow activities, on-prem) ==="
dotnet build samples/SourcePackageSmokeTest/SourcePackageSmokeTest.csproj -c Release -p:JmclVersion="${VERSION}"

echo "=== ALL VERIFIED at ${VERSION} ==="
