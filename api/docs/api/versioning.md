# API Versioning

## Current version

**v1** is the only published API version (`ApiVersion(1.0)`).

## URL segment

Canonical routes embed the version:

```
/api/v1/{resource}/...
```

Example: `/api/v1/tools` and `/api/tools` invoke the same handler.

## Default version behavior

Configuration:

- `DefaultApiVersion = 1.0`
- `AssumeDefaultVersionWhenUnspecified = true`
- `ReportApiVersions = true`
- Version read from **URL segment** (`UrlSegmentApiVersionReader`)

Unversioned `/api/...` requests are treated as v1 without requiring clients to send a version header.

## Response headers

When versioning middleware reports versions, responses may include:

```http
api-supported-versions: 1.0
```

## OpenAPI

OpenAPI documents describe **versioned paths only** (`/api/v1/...`) to avoid duplicate operation IDs. Unversioned aliases are documented in prose (see [README.md](README.md)).

Document names: `public-v1`, `authenticated-v1`, `admin-v1`, `all-v1`.

## Legacy routes

All controllers declare dual routes:

```csharp
[Route("api/{resource}")]
[Route("api/v{version:apiVersion}/{resource}")]
```

Legacy unversioned paths remain **fully supported** as v1 compatibility aliases. No removal is scheduled.

Deprecated exceptions:

- `GET /api/health` — use `/health/live` and `/health/ready` instead ([health.md](health.md))

## Deprecation policy

1. **Non-breaking changes** (additive) ship within the same major version: new optional fields, new endpoints, higher default limits with backward-compatible caps.
2. **Breaking changes** require a new major URL version (e.g. `/api/v2/...`) with a documented migration period.
3. Deprecated endpoints remain functional until a new major version is announced; OpenAPI marks them `deprecated: true` where applicable.
4. No fake or placeholder sunset dates — refer to [changelog.md](changelog.md) for contract announcements.

## Breaking vs non-breaking (examples)

| Non-breaking | Breaking |
|--------------|----------|
| Add optional query parameter | Remove or rename a field |
| Add new endpoint | Change error code semantics for same input |
| Add new enum value (clients ignore unknown) | Require new mandatory request field |
| Increase max `pageSize` | Change pagination to cursor-only |
| Add `/api/v1/...` alongside `/api/...` | Remove unversioned alias in a minor release |

## Migration

Existing clients may keep unversioned paths. New integrations should use `/api/v1/...`. See [migration-guide-v1.md](migration-guide-v1.md).
