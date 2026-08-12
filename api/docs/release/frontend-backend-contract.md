# Frontend / Backend Contract

How the Next.js frontend consumes the HelpDev backend. Part of **Sprint 26 — Frontend
Integration & Go-Live Readiness**.

This is the integration contract the frontend targets for v1. Backend behavior is
authoritative; see [../api/README.md](../api/README.md), [../api/errors.md](../api/errors.md),
and [../api/correlation-id.md](../api/correlation-id.md).

## Base URL and configuration

- **Canonical base URL:** `/api/v1`. Configured via the public env var
  `NEXT_PUBLIC_HELPDEV_API_BASE_URL` (e.g. `https://api.example.com/api/v1`).
- HTTPS is **required in Production**; the value is validated at startup/build.
- Legacy `/api/...` routes exist for backward compatibility but new frontend code targets
  `/api/v1`. Health probes use `/health/live` and `/health/ready`.
- See [site/docs/api-configuration.md](../../../site/docs/api-configuration.md).

## Typed client modules

The frontend uses a centralized, typed API client (under `site/src/lib/`). Each module maps
a backend area to typed request/response functions and returns typed DTOs:

- **Auth** — OTP request/verify, session.
- **Profile** — current user profile.
- **Content** — public content lists/details.
- **Search** — query and results.
- **Toolbox** — tool catalog and execution.
- **PromptLab** — prompt templates and rendering.
- **Admin** — operational version/status, audit, analytics.

## Shared error contract

The backend returns a **flat JSON** error object (camelCase, not ProblemDetails):

```json
{
  "message": "Human-readable description",
  "code": "stable_machine_code"
}
```

Some auth paths may return `{ "message": "..." }` without `code`; treat missing `code` as
non-specific. The client surfaces this as an **`ApiClientError`** with fields:

| Field | Meaning |
|-------|---------|
| `message` | Human-readable message (safe to show; never a stack trace). |
| `code` | Stable machine code for UI logic (may be absent). |
| `status` | HTTP status code. |
| `correlationId` | Value of the `X-Correlation-ID` response header, for support. |
| `retryAfterSeconds` | Parsed `Retry-After` (present on `429` when provided). |

See [site/docs/error-handling.md](../../../site/docs/error-handling.md).

## Correlation handling

- The client generates an `X-Correlation-ID` per request when none is supplied and captures
  the value echoed on the response.
- Rules mirror the backend: **max 100 characters**; permitted characters are letters,
  digits, `-`, `_`, `.`. Invalid values are replaced server-side.
- The captured ID is preserved on `ApiClientError` for support/debugging. See
  [../api/correlation-id.md](../api/correlation-id.md).

## Authentication flow

```
mobile ──▶ POST /auth/send-otp ──▶ POST /auth/verify-otp ──▶ JWT (access token)
        ──▶ GET /profile ──▶ role-based routing ──▶ 401 / expiry ──▶ logout
```

- OTP is never exposed in Production responses.
- The access token is a JWT with a bounded lifetime (`Jwt:ExpirationMinutes`). **No refresh
  tokens exist** — on `401`/expiry the user re-authenticates via OTP.
- Token storage today uses browser `localStorage` (documented limitation). See
  [site/docs/authentication-security.md](../../../site/docs/authentication-security.md).

## Representative DTOs (illustrative)

Shapes are illustrative; the OpenAPI v1 contract is authoritative.

**Auth response** (from `verify-otp`)

```json
{ "accessToken": "<jwt>", "expiresIn": 3600, "user": { "id": "...", "mobile": "...", "role": "User" } }
```

**User profile**

```json
{ "id": "...", "mobile": "...", "role": "User", "displayName": "...", "email": "...", "profileCompletionPercent": 0 }
```

**Content summary**

```json
{ "id": "...", "slug": "...", "title": "...", "summary": "...", "publishedAt": "<utc>" }
```

**Search result**

```json
{ "query": "...", "items": [ { "id": "...", "type": "content", "title": "...", "score": 0.0 } ], "total": 0 }
```

**Tool execution response**

```json
{ "toolSlug": "...", "status": "succeeded", "output": { } }
```

**Prompt render response**

```json
{ "templateId": "...", "renderedPrompt": "...", "tokensEstimated": 0 }
```

**API error response**

```json
{ "message": "...", "code": "..." }
```

**Audit page**

```json
{ "items": [ { "id": "...", "action": "...", "occurredAt": "<utc>" } ], "page": 1, "pageSize": 20, "total": 0 }
```

**Operational version response** (`GET /api/v1/admin/operations/version`)

```json
{ "version": "1.0.0", "commit": "<commit>", "channel": "production" }
```

## OpenAPI contract check

The frontend validates its expectations against the backend **OpenAPI v1** contract. Since
OpenAPI is disabled in Production, run the contract check against a non-Production build/spec
export, not the live Production surface. Categorized tests
(`FrontendIntegration`) exercise this contract.

## Related

- [../api/README.md](../api/README.md)
- [../api/errors.md](../api/errors.md)
- [../api/correlation-id.md](../api/correlation-id.md)
- [../api/authentication.md](../api/authentication.md)
- [go-live-checklist.md](go-live-checklist.md)
