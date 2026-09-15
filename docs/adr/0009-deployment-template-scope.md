# ADR 0009: What the deployment template covers, and what it does not

**Status:** Accepted
**Date:** 2026-09-15

## Context

A prior pac migration of a large private estate was reviewed before building
this template. That migration is more careful than its own summary suggests,
and reading the scripts rather than the summary changed three things here.

## Corrections to the first cut

**Package Deployer is not universally eliminable.** The first version of this
template claimed to drop it outright. The prior migration could not, because
two operations have no `pac` equivalent: importing document templates, and
resetting an environment. Both remained on the old authentication stack, and
the scripts say so at the point of use rather than pretending otherwise. This
template drops Package Deployer because nothing here needs those operations,
which is a scope statement, not a general result.

**A zero exit code is not evidence an artifact appeared.** The prior scripts
check `$LASTEXITCODE -ne 0 -or -not (Test-Path $zip)` on every export. That is
the same lesson this framework learned painfully when a pack step reported
success eighteen times while producing nothing. `Invoke-Pac -Produces <path>`
now asserts the file exists.

**Packing Managed needs `--useUnmanagedFileForMissingManaged`.** Some
components unpack with only an unmanaged XML file, and packing Managed fails on
exactly those without it.

## Deliberately not carried forward yet, in priority order

1. ~~Solution XML normalization.~~ **Done, deliberately narrow.**
   `scripts/Normalize-SolutionXml.ps1` with `settings/normalization.json`.

   The first cut had ten rules using `//` wildcards across every XML file in
   the solution. That was a generalization of a narrow, proven tool into a
   broad, unproven one, on a format with no real example in hand. It is now
   **three absolute XPaths**, matching the only containers known to be
   order-insensitive in practice:

   ```
   /ImportExportXml/SolutionManifest/MissingDependencies
   /EntityRelationships
   /Entity/EntityInfo/entity/attributes
   ```

   Absolute paths are the safety property: a rule fires only on a document
   whose root element matches, so it cannot stray into a similarly-named
   element elsewhere. `neverDescendInto` guards positional regions as a second
   line of defence.

   Validated against synthetic documents covering all three root shapes, a
   decoy document with a matching element under a different root, an
   `attributes` block nested inside `FormXml`, and a second pass proving
   idempotency. The decoy and the `FormXml` block were both left untouched.
   `-Verify` makes it assertable in CI.

2. **Multiple solutions per repository, with import ordering.** This template
   assumes one solution. Real repositories carry several, and dependency
   solutions must import first.

3. **Configuration data and test data as separate concerns.** Distinct schema
   files, distinct archives, distinct lifecycles. Configuration data belongs in
   every environment; test data does not belong in production.

4. **Power Pages.** `pac pages list` and `pac pages download`, which need a
   tenant id, a site id and a public client id, plus `--cloud` for sovereign
   clouds.

5. **Solution version stamping.** `pac solution online-version`, or a
   date-derived version such as `0.2026.0915.1430`.

6. **Missing-dependency handling.** Adding and removing keys under
   `MissingDependencies` in `solution.xml`, which works around import failures
   caused by dependencies the target environment resolves differently.

7. **Lazy authentication.** The prior scripts authenticate only when the
   requested action needs a live connection, so purely local operations run
   without a round trip.

## Decision

Ship the single-solution round trip with normalization now. Treat multiple
solutions and the configuration/test data split as required before this
template is used on a real project.

The three rules are narrow enough to be low risk, but the first real round trip
should still confirm that the containers appear with the expected keys and that
the solution imports afterwards. Add a fourth rule only with that same evidence
in hand, never because a container looks sortable.
