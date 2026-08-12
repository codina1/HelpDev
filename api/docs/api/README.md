# HelpDev API — Consumer Documentation

HelpDev is a .NET 8 modular monolith exposing a single HTTP API for public browsing, authenticated users, and admin operations.

## Base path and version

| Item | Value |
|------|-------|
| Current version | **v1** (`1.0`) |
| Canonical routes | `/api/v1/...` |
| Legacy aliases | `/api/...` (same handlers, v1 compatibility) |
| Health probes | `/health/live`, `/health/ready` (outside `/api`) |

All documented routes use the versioned prefix. Unversioned `/api/...` paths remain supported; see [versioning.md](versioning.md) and [migration-guide-v1.md](migration-guide-v1.md).

## Authentication

Access is **JWT Bearer only** after mobile OTP verification. There is no OAuth, API keys, or third-party identity provider integration.

1. `POST /api/v1/auth/send-otp` — request OTP
2. `POST /api/v1/auth/verify-otp` — verify OTP, receive JWT
3. Send `Authorization: Bearer {token}` on protected requests

Details: [authentication.md](authentication.md)

## OpenAPI documents

When OpenAPI is enabled on the host, JSON specs are served at:

| Document | URL | Audience |
|----------|-----|----------|
| Public | `/openapi/public-v1.json` | Anonymous endpoints |
| Authenticated | `/openapi/authenticated-v1.json` | Bearer JWT required |
| Admin | `/openapi/admin-v1.json` | Admin role required |
| Complete | `/openapi/all-v1.json` | Full v1 surface |

Exported artifacts (CI/build): `api/artifacts/openapi/helpdev-{public,authenticated,admin,all}-v1.json`.

Swagger UI is optional (`OpenApi:EnableUi`). In Production, OpenAPI is **disabled by default** (`OpenApi:EnableInProduction=false`).

Endpoint catalogs: [public-api.md](public-api.md), [authenticated-api.md](authenticated-api.md), [admin-api.md](admin-api.md).

## Errors

Errors use a flat JSON object — **not** RFC 7807 ProblemDetails:

```json
{ "message": "Human-readable text", "code": "stable_machine_code" }
```

Some older code paths may return `message` only; module exception filters return both fields. See [errors.md](errors.md).

## Timestamps

All API timestamps are **UTC**, serialized as ISO 8601 (e.g. `2026-07-20T16:30:00Z`).

## Correlation

Optional request header `X-Correlation-ID` (max 100 chars; alphanumeric, `_`, `-`, `.`). Invalid or missing values are replaced; the chosen ID is echoed on the response. See [correlation-id.md](correlation-id.md).

## Rate limits

Most routes share a general API limiter. Dedicated policies apply to OTP, search, toolbox execute, prompt render, public content reads, and admin mutations. Exceeding a limit returns **429** with optional `Retry-After`. See [rate-limits.md](rate-limits.md).

## Request body size limits

| Area | Limit |
|------|-------|
| OTP (`/api/.../auth/...`) | 16 KB |
| Toolbox execute, Prompt render | 128 KB |
| General JSON, admin JSON | 256 KB |

Oversized bodies return **413** with `code: security_request_too_large`.

## Health

| Endpoint | Purpose |
|----------|---------|
| `GET /health/live` | Liveness (process up) |
| `GET /health/ready` | Readiness (dependencies); 503 when Unhealthy |
| `GET /api/health` | **Deprecated** legacy probe |

See [health.md](health.md).

## Backward compatibility

- Unversioned `/api/...` routes map to the same v1 handlers as `/api/v1/...`.
- `AssumeDefaultVersionWhenUnspecified` treats unspecified version as v1.
- No forced migration deadline; new integrations should prefer `/api/v1/...` and `/health/*`.

## Documentation index

| Topic | File |
|-------|------|
| Authentication | [authentication.md](authentication.md) |
| Versioning | [versioning.md](versioning.md) |
| Errors | [errors.md](errors.md) |
| Pagination | [pagination.md](pagination.md) |
| Rate limits | [rate-limits.md](rate-limits.md) |
| Correlation ID | [correlation-id.md](correlation-id.md) |
| Health | [health.md](health.md) |
| Public endpoints | [public-api.md](public-api.md) |
| Authenticated endpoints | [authenticated-api.md](authenticated-api.md) |
| Admin endpoints | [admin-api.md](admin-api.md) |
| Toolbox | [toolbox.md](toolbox.md) |
| PromptLab | [promptlab.md](promptlab.md) |
| Audit | [audit.md](audit.md) |
| Operations & outbox | [operations.md](operations.md) |
| Migration guide | [migration-guide-v1.md](migration-guide-v1.md) |
| Changelog | [changelog.md](changelog.md) |
