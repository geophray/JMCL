# ADR 0008: The Dataverse publisher prefix is not the framework's to choose

**Status:** Accepted, with follow-up
**Date:** 2026-09-15

## Context

Purging upstream branding surfaced three strings the namespace rename had
missed, because the rules only matched `the hosting organization.` with a dot:

| File | String | What it actually is |
|---|---|---|
| `Telemetry/Client/ComponentTelemetryClient.cs` | `"the hosting organization:0.1.1-100"` | An Application Insights `SdkVersion` value |
| `Dataverse.FieldEncryption/ServiceConfiguration.cs` | `"the hosting organization_decryptthis"` | A Dataverse **column logical name** |
| `Dataverse.FieldEncryption/EncryptedFieldSettings.cs` | `"the hosting organization_/configuration/encryptedfields/"` | A Dataverse **web resource path** |

The first is cosmetic. The other two are not branding at all: `the hosting organization_` is a
**publisher prefix**, which Dataverse assigns per solution publisher in the
environment where a solution is deployed. It belongs to whoever ships the
solution, not to the framework they build it with.

## Decision

The rename rules now also handle the non-dotted forms, so a re-run cannot
reintroduce them. `jmcl_` is used as the default prefix.

**`jmcl_` is a neutral default, not a correct one.** Substituting one hardcoded
vendor prefix for another fixes the branding and leaves the design flaw intact:
a consumer whose publisher prefix is `contoso_` still has to override both
values, and will get a runtime failure rather than a compile error if they do
not, because a column that does not exist is only discovered when something
reads it.

Both values are already overridable, which is why this is a follow-up rather
than a blocker. `GlobalUnmaskTriggerAttributeName` is settable from the XML
configuration, and `ConfigurationDataPath` reads the
`JMCL.EncryptedFields.DataPath` setting.

## Follow-up

When `Dataverse.FieldEncryption` is evaluated for adoption (it is already
conditional on the platform column-level-security overlap check from the fork
plan), make the publisher prefix a first-class setting and compose these
defaults from it, rather than embedding a prefix in each string. If no prefix is
configured, fail at configuration time with a clear message instead of at read
time with a missing column.

Deliberately not done now: Phase 1 is a mechanical conversion, and behavior
changes belong with a test that proves the behavior. There are currently no
tests for `FieldEncryption`.
