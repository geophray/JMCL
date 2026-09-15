# ADR 0001: Fork Scott Colson's Power Platform code libraries

**Status:** Accepted
**Date:** 2026-09-08

## Context

A reusable Power Platform solution template needs a code framework underneath it.
Scott Colson's libraries provide a mature one: a platform-agnostic process model,
a sandbox-safe IoC container, a declarative plugin event registration model, and
Application Insights compatible telemetry that works inside the plugin sandbox.

The code exists in two places. Three public repositories under
github.com/ScottColson, and eleven repositories in the the hosting organization Azure DevOps
collection. Review established that the ADO repositories are Colson's own
continued development rather than a third party fork: he is the dominant author
in every one, each carries an MIT license with his copyright, and the ADO set is
a strict superset of the public code.

Upstream is effectively unmaintained. The newest commit anywhere is March 2024;
most components last moved in 2020 or 2021.

## Decision

Fork, under the root namespace `JMCL`, from the ADO repositories, using the
public repositories only to cross-check overlapping code.

Treat Colson's code as upstream and carry it forward directly under the MIT
grant, with attribution in LICENSE, NOTICE and README.

Exclude `solution_template` and `deployment_template`. Those are multi-author
the hosting organization work product with no license file. Their equivalents are designed
from first principles in Phase 4.

## Consequences

- Permanent ownership of maintenance for roughly 28,000 lines of production source.
- The namespace change is a hard break. Nothing on `CCLLC.*` or `the hosting organization.*` packages
  can incrementally adopt this. New projects forward only.
- Six components with no public counterpart come into scope, which is additional
  capability and additional maintenance surface.
- The framework is not end to end useful until Phase 4 replaces the excluded
  templates.
