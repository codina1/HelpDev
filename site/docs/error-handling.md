# Error Handling

Frontend error model for the HelpDev application. Part of **Sprint 26 — Frontend
Integration & Go-Live Readiness**.

The backend returns a **flat JSON** error object (not ProblemDetails). The frontend maps it
to a typed error and applies consistent, safe handling.

## Backend error shape

```json
{
  "message": "Human-readable description",
  "code": "stable_machine_code"
}
```

- Property names are camelCase. `message` is human-readable; `code` is a stable machine code
  for UI logic.
- Some auth paths may omit `code`; treat a missing `code` as non-specific.
- The correlation ID is **not** in the body — read it from the `X-Correlation-ID` response
  header.

## `ApiClientError`

The typed client raises an `ApiClientError` carrying:

| Field | Type | Meaning |
|-------|------|---------|
| `message` | string | Safe, human-readable message. Never a stack trace. |
| `code` | string \| undefined | Stable machine code for UI branching (may be absent). |
| `status` | number | HTTP status code. |
| `correlationId` | string \| undefined | Captured `X-Correlation-ID` for support. |
| `retryAfterSeconds` | number \| undefined | Parsed `Retry-After` (present on `429` when provided). |

## Handling per status

| Status | Meaning | Frontend handling |
|--------|---------|-------------------|
| **400** | Validation / bad input | Show field or form errors from `message`/`code`; do not retry automatically. |
| **401** | Missing/invalid/expired token | Clear session; route to authentication (OTP). No refresh tokens exist. |
| **403** | Authenticated but not permitted | Show an access-denied message; do not retry. |
| **404** | Not found | Show a not-found state; do not retry. |
| **409** | State conflict | Surface the conflict (`message`/`code`); prompt the user to resolve before retrying. |
| **413** | Payload too large (`security_request_too_large`) | Ask the user to reduce input size; do not retry as-is. |
| **429** | Rate limited (`security_rate_limit_exceeded`) | Respect `retryAfterSeconds`; back off; retry only safe (idempotent) reads. |
| **500** | Server error | Show a generic error; offer retry for idempotent GETs with backoff. |
| **503** | Service unavailable | Show a temporary-unavailable state; retry with backoff. |

## Rules

- **Never expose stack traces or internal details.** Show only the safe `message`.
- **Show a safe user message.** Fall back to a generic localized message when `message` is
  absent.
- **Preserve `code` for UI logic.** Branch on `code` rather than parsing `message` strings.
- **Preserve the correlation ID for support.** Keep `correlationId` so users/operators can
  reference a specific request.
- **Parse `Retry-After` safely.** Only honor a valid non-negative number; ignore malformed
  values.
- **Do not blindly retry mutations.** Auto-retry only safe/idempotent GETs. Never
  auto-retry POST/PUT/DELETE without explicit, idempotency-aware handling.

## Correlation ID display

- The correlation ID is for support and debugging, not primary UI copy.
- Display it only in a support/debug details area (e.g. an expandable "Details" section or
  an error report), so users can quote it when contacting support.

## Related

- [api-configuration.md](api-configuration.md)
- [authentication-security.md](authentication-security.md)
- [../../api/docs/api/errors.md](../../api/docs/api/errors.md)
- [../../api/docs/api/correlation-id.md](../../api/docs/api/correlation-id.md)
- [../../api/docs/api/rate-limits.md](../../api/docs/api/rate-limits.md)
