# Errors

## Response shape

HelpDev returns a **flat JSON object**, not ASP.NET ProblemDetails:

```json
{
  "message": "Human-readable description",
  "code": "stable_machine_code"
}
```

JSON property names are **camelCase** (`message`, `code`).

Module exception filters populate both fields. A few controller paths (e.g. some auth validation) may return `{ "message": "..." }` without `code`; clients should treat missing `code` as non-specific.

## Correlation

Error responses include the same **`X-Correlation-ID`** response header as successful responses. Use it when contacting operators. See [correlation-id.md](correlation-id.md).

## HTTP status codes

| Status | Typical meaning | Example `code` values |
|--------|-----------------|------------------------|
| **400** Bad Request | Validation, invalid input | `validation_failed`, domain-specific codes |
| **401** Unauthorized | Missing/invalid/expired JWT | `authentication_required`, `toolbox_tool_requires_authentication` |
| **403** Forbidden | Authenticated but not permitted | `access_denied` |
| **404** Not Found | Resource does not exist | `resource_not_found`, `toolbox_tool_not_found` |
| **409** Conflict | State conflict (duplicate slug, wrong lifecycle) | `resource_conflict`, `toolbox_tool_slug_duplicate` |
| **413** Payload Too Large | Body exceeds route limit | `security_request_too_large` |
| **429** Too Many Requests | Rate limit exceeded | `security_rate_limit_exceeded` |
| **500** Internal Server Error | Unhandled server fault | (varies; may be generic) |
| **503** Service Unavailable | Dependency down / not ready | `service_unavailable` |

## 429 and Retry-After

Rate-limited responses return **429** with:

```json
{
  "message": "Too many requests. Please try again later.",
  "code": "security_rate_limit_exceeded"
}
```

When the limiter provides retry metadata, the response includes:

```http
Retry-After: {seconds}
```

Honor `Retry-After` before retrying. See [rate-limits.md](rate-limits.md).

## 413 request size

Triggered when the request body exceeds the configured limit for the route class (16 KB / 128 KB / 256 KB). See [README.md](README.md#request-body-size-limits).

## Retry guidance

| Status | Retry? | Notes |
|--------|--------|-------|
| 400 | No | Fix the request |
| 401 | No* | Re-authenticate via OTP |
| 403 | No | Insufficient permissions |
| 404 | No | Unless resource may appear later (rare) |
| 409 | Usually no | Resolve conflict first |
| 413 | No | Reduce payload size |
| 429 | Yes | Wait for `Retry-After` or backoff |
| 500 | Cautious | Exponential backoff; idempotent GETs safer |
| 503 | Yes | Backoff; check [health.md](health.md) |

\*Except after refreshing an expired token via OTP.

## Idempotency

v1 does not expose a standard idempotency-key header. Prefer safe retries only on GET and explicitly documented idempotent operations.

## Related

- [authentication.md](authentication.md) — 401 vs 403
- [rate-limits.md](rate-limits.md) — 429 behavior
