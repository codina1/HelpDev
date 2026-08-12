# Release Evidence Template

Template for capturing the evidence that a HelpDev release was built, validated, and
deployed correctly. Part of **Sprint 26 — Production Deployment Validation & Go-Live
Readiness**.

Copy this template per release and fill each field. **No secrets** — record hashes,
statuses, and metadata only, never connection strings, keys, or tokens. A completed example
lives at `api/artifacts/release/release-evidence.md`.

## Release identity

| Field | Value |
|-------|-------|
| Version | `<version>` |
| Commit | `<commit>` |
| Build timestamp (UTC) | `<utc-timestamp>` |
| Release channel | `<channel>` |

## Build & test

| Field | Value |
|-------|-------|
| Build result | `<succeeded (0 warnings, 0 errors) | failed>` |
| Test result | `<all backend tests pass | count / failures>` |
| Test count | `<n>` |

## Artifact integrity

| Field | Value |
|-------|-------|
| OpenAPI (v1) hash | `<sha256-or-n/a>` |
| Publish artifact hash (`HelpDev.API.dll`) | `<sha256>` |
| Manifest `migrationCount` | `11` |
| Manifest `binarySha256` matches computed hash | `<yes | no>` |

## Validation

| Field | Value |
|-------|-------|
| Config validation status | `<passed | failed>` (`--validate-production-config`) |
| Migration status | `<pending=0, applied=N | applied this release>` |
| Smoke-test result | `<all pass | list failures>` |

## Sign-off & deployment

| Field | Value |
|-------|-------|
| Reviewer | `<name/role>` |
| Deployment timestamp (UTC) | `<utc-timestamp>` |
| Rollback artifact version | `<previous-version>` |

## Notes

- Attach or link the emitted `release-manifest.json`.
- Attach smoke-test output/summary (see
  [go-live-smoke-tests.md](go-live-smoke-tests.md)).
- Record any waived checklist items with a reason (see
  [go-live-checklist.md](go-live-checklist.md)).

## Related

- [go-live-checklist.md](go-live-checklist.md)
- [go-live-smoke-tests.md](go-live-smoke-tests.md)
- [../deployment/publish-artifact.md](../deployment/publish-artifact.md)
