# Migration Guide — API v1

This guide helps clients adopt the documented v1 contract without breaking existing integrations.

## No forced migration

- Unversioned routes (`/api/...`) remain **fully supported** as v1 aliases
- Authentication flow is **unchanged** — OTP + JWT Bearer
- There is **no sunset date** for legacy paths

New projects should prefer explicit versioned URLs and modern health probes.

## URL changes (recommended)

| Legacy | Recommended |
|--------|-------------|
| `/api/tools` | `/api/v1/tools` |
| `/api/prompts` | `/api/v1/prompts` |
| `/api/auth/send-otp` | `/api/v1/auth/send-otp` |
| `/api/admin/...` | `/api/v1/admin/...` |

Behavior is identical; only the path prefix differs.

## Health check migration

| Legacy | Recommended |
|--------|-------------|
| `GET /api/health` (deprecated) | `GET /health/live` for liveness |
| | `GET /health/ready` for readiness |

Legacy `/api/health` returns database-centric JSON and may respond **503** when PostgreSQL is unreachable. Standard probes should use the minimal `/health/*` endpoints ([health.md](health.md)).

## Authentication

No changes required:

1. Continue using `POST /api/auth/send-otp` and `POST /api/auth/verify-otp`, or switch to `/api/v1/auth/...`
2. Continue sending `Authorization: Bearer {token}`
3. JWT claims, roles, and expiration semantics are unchanged

See [authentication.md](authentication.md).

## OpenAPI

If you generate clients from OpenAPI, point tooling at:

- `/openapi/public-v1.json`
- `/openapi/authenticated-v1.json`
- `/openapi/admin-v1.json`
- `/openapi/all-v1.json`

Or use committed artifacts under `api/artifacts/openapi/`.

OpenAPI lists **versioned paths only**; generate clients with base path `/api/v1` or configure a server URL accordingly.

## Error handling

Ensure clients parse `{ "message", "code" }` rather than ProblemDetails (`type`, `title`, `status`). See [errors.md](errors.md).

## Correlation

Optionally adopt `X-Correlation-ID` on outbound requests for easier support. See [correlation-id.md](correlation-id.md).

## Version headers

Responses may include `api-supported-versions: 1.0`. Future major versions will introduce new URL segments (e.g. `/api/v2/...`) with advance notice in [changelog.md](changelog.md).

## Checklist for new integrations

- [ ] Use `/api/v1/...` base paths
- [ ] Use `/health/live` and `/health/ready` for probes
- [ ] Handle `{ message, code }` errors and 429 + `Retry-After`
- [ ] Store JWT securely; never in query strings
- [ ] Respect pagination defaults per area ([pagination.md](pagination.md))
- [ ] Download the appropriate OpenAPI document for your audience

## Related

- [versioning.md](versioning.md)
- [README.md](README.md)
- [changelog.md](changelog.md)
