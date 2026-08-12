# Operations

Admin-only operational visibility and recovery. All routes require **Admin** role and JWT Bearer auth.

Two complementary areas:

1. **`/api/v1/admin/operations/*`** — live snapshots and health diagnostics
2. **`/api/v1/admin/outbox/*`** — outbox message recovery

## Operations snapshots

Base: `/api/v1/admin/operations`

| Method | Path | Summary |
|--------|------|---------|
| GET | `/status` | High-level operations summary |
| GET | `/health` | Detailed health for platform subsystems |
| GET | `/outbox` | Outbox operational snapshot (counts, ages) |
| GET | `/search` | Search index snapshot |
| GET | `/analytics` | Analytics ingestion snapshot |
| GET | `/audit` | Audit storage snapshot |
| GET | `/logging` | Logging configuration (`minimumLogLevel`, `redactionEnabled`) |

These endpoints support internal dashboards and on-call playbooks. They are **not** substitutes for public probes — use `/health/live` and `/health/ready` for load balancers ([health.md](health.md)).

### Detailed admin health

`GET /api/v1/admin/operations/health` returns subsystem-level status (PostgreSQL, outbox, search, analytics, audit, etc.) with configurable thresholds. May include environment and version metadata when enabled.

## Outbox recovery

Base: `/api/v1/admin/outbox`

Transactional outbox messages propagate domain events asynchronously. When processing fails, operators can inspect and retry messages through the admin API.

### Payload privacy

**Message payloads are never returned** by the outbox management API. List and detail DTOs expose:

- `id`, `type`, `occurredAtUtc`, `processedAtUtc`
- `attemptCount`, `lastAttemptAtUtc`, `error` (truncated/sanitized)
- `lockedUntilUtc`, `status` (`pending`, `processing`, `failed`, `processed`)

This prevents accidental exposure of user data or secrets in support tooling.

### Endpoints

| Method | Path | Summary |
|--------|------|---------|
| GET | `/status` | Aggregate counts (pending, processing, failed, processed) |
| GET | `/messages` | Paginated list (`status`, `type`, `page`, `pageSize`) |
| GET | `/messages/{id}` | Detail without payload |
| POST | `/messages/{id}/retry` | Retry one failed/pending message |
| POST | `/retry-failed` | Batch retry (`{ "limit": N, "type": "..." }` optional body) |

### Retry behavior

- Retry single message: returns updated detail DTO; **409** if already processed or actively processing
- Retry failed batch: returns count of reset messages and completion timestamp
- Errors in logs omit raw payload content by design

### Operational snapshot vs management

| Concern | Use |
|---------|-----|
| Dashboards / alerting | `GET /admin/operations/outbox` |
| Message-level recovery | `GET/POST /admin/outbox/...` |

## Search reindex

**`POST /api/v1/search/manage/reindex`** — Admin-only full reindex trigger. See [admin-api.md](admin-api.md).

## Rate limits

All operations and outbox routes use **AdminMutation** limits ([rate-limits.md](rate-limits.md)).

## Related

- [admin-api.md](admin-api.md)
- [health.md](health.md) — public probes
- [audit.md](audit.md) — audit trail for admin actions
- [errors.md](errors.md) — outbox error codes (`outbox_*`)
