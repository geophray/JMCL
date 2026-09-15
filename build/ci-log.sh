#!/usr/bin/env bash
# Dump the most recent failed CI run's log where the assistant can read it.
#
# "gh run view --log-failed" needs an explicit run id when its output is
# redirected, which is easy to trip over. This resolves the latest run id first.
set -euo pipefail
cd "$(dirname "$0")/.."

OUT="${1:-$HOME/Repos/ci-log.txt}"
RUN="$(gh run list --limit 1 --json databaseId -q '.[0].databaseId')"

{
  echo "### run ${RUN}"
  gh run view "${RUN}" --json displayTitle,status,conclusion,headBranch,createdAt \
    -q '"\(.displayTitle)  [\(.headBranch)]  \(.status)/\(.conclusion)  \(.createdAt)"'
  echo
  gh run view "${RUN}" --log-failed
} > "${OUT}" 2>&1

echo "wrote ${OUT} ($(wc -l < "${OUT}") lines)"
