# Audit

Immutable audit trail for security and administration events. **Admin role required** for all audit API routes.

## Endpoints

Base: `/api/v1/admin/audit`

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/audit` | Paginated audit records |
| GET | `/api/v1/admin/audit/{id}` | Single record by ID |
| GET | `/api/v1/admin/audit/actions` | Supported action identifiers |
| GET | `/api/v1/admin/audit/categories` | Supported category identifiers |

## List filters

Query parameters for `GET /api/v1/admin/audit`:

| Parameter | Description |
|-----------|-------------|
| `from`, `to` | UTC date range (max span **366 days**) |
| `category` | Filter by audit category |
| `action` | Filter by action |
| `outcome` | Filter by outcome |
| `actorUserId` | Filter by acting user |
| `subjectId`, `subjectType` | Filter by subject entity |
| `page`, `pageSize` | Pagination (default 20, max 100) |

Invalid filters return **400** with audit-specific error codes.

## Supported categories

Returned by `GET .../categories`:

- `Authentication`
- `Authorization`
- `Administration`
- `ToolboxManagement`
- `PromptManagement`
- `OutboxOperations`
- `Security`

## Supported actions (examples)

Returned by `GET .../actions`; includes:

- `Authentication.OtpRequested`, `Authentication.OtpVerified`, `Authentication.OtpVerificationFailed`
- `Authentication.LoginSucceeded`, `Authentication.RateLimited`
- `Authorization.AccessDenied`
- `Administration.FeatureFlagCreated/Updated/Enabled/Disabled`
- `Administration.SettingCreated/Updated`
- `Security.RateLimitExceeded`

Exact strings match server constants; use the list endpoint for authoritative values.

## Record content

Audit records expose sanitized metadata suitable for compliance review. Sensitive values (tokens, OTPs, raw payloads) are **not** stored in audit metadata.

Each record includes correlation context when available (`X-Correlation-ID` from the originating request).

## Operational snapshot

For storage health and ingestion metrics (not individual records):

**`GET /api/v1/admin/operations/audit`**

See [operations.md](operations.md).

## Rate limits

Audit admin controller uses **AdminMutation** policy.

## Related

- [admin-api.md](admin-api.md)
- [pagination.md](pagination.md)
- [correlation-id.md](correlation-id.md)
- [operations.md](operations.md)
