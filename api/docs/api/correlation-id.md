# Correlation ID

HelpDev supports distributed tracing via the **`X-Correlation-ID`** HTTP header.

## Request header

```http
X-Correlation-ID: my-trace-abc123
```

| Rule | Value |
|------|-------|
| Required | No |
| Max length | 100 characters |
| Allowed characters | Letters, digits, `-`, `_`, `.` |

If the header is **missing**, the server generates a correlation ID.

If the header is **present but invalid** (wrong charset or too long), it is **replaced** with a server-generated ID.

## Response header

Every response echoes the correlation ID in use:

```http
X-Correlation-ID: {id}
```

Use this value when reporting errors to operators or correlating client logs with server audit entries.

## CORS

Allowed CORS request headers include `X-Correlation-ID` alongside `Authorization`, `Content-Type`, and `Accept`.

## Error correlation

Error bodies do **not** embed the correlation ID; read it from the response header. See [errors.md](errors.md).

## Client guidance

- Generate a unique ID per user action or request chain (UUID without braces, trace ID, etc.)
- Reuse the same ID for retries of the **same** logical operation when safe
- Do not include secrets or PII in the correlation ID
- Log the response header value on failed requests

## Related

- [README.md](README.md)
- [errors.md](errors.md)
