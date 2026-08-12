# Admin API

Endpoints for **trusted internal UI and operations**. Every route requires:

- Valid JWT Bearer token
- **`Admin`** role

OpenAPI: [`/openapi/admin-v1.json`](/openapi/admin-v1.json) · Artifact: `api/artifacts/openapi/helpdev-admin-v1.json`

All admin routes use the **AdminMutation** rate limit policy. JSON bodies default to **256 KB** max. See [rate-limits.md](rate-limits.md).

Unversioned `/api/admin/...` aliases mirror `/api/v1/admin/...`.

## Users

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/users` | List all users |
| GET | `/api/v1/admin/users/{id}` | Get user |
| PUT | `/api/v1/admin/users/{id}` | Update user profile and role |

## Dashboard

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/dashboard` | Admin dashboard summary |

## Announcements

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/announcements` | List announcements ([paginated](pagination.md)) |
| POST | `/api/v1/admin/announcements` | Create |
| GET | `/api/v1/admin/announcements/{id}` | Get |
| PUT | `/api/v1/admin/announcements/{id}` | Update |
| DELETE | `/api/v1/admin/announcements/{id}` | Delete |
| POST | `/api/v1/admin/announcements/{id}/publish` | Publish |
| POST | `/api/v1/admin/announcements/{id}/archive` | Archive |

## Feature flags

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/features` | List feature flags |
| POST | `/api/v1/admin/features` | Create |
| GET | `/api/v1/admin/features/{key}` | Get |
| PUT | `/api/v1/admin/features/{key}` | Update |
| PUT | `/api/v1/admin/features/{key}/state` | Set enabled state |

## System settings

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/settings` | List settings |
| POST | `/api/v1/admin/settings` | Create |
| GET | `/api/v1/admin/settings/{key}` | Get |
| PUT | `/api/v1/admin/settings/{key}` | Update |

## Analytics

Aggregate reports; eventually consistent.

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/analytics/overview` | Overview |
| GET | `/api/v1/admin/analytics/time-series` | Time series |
| GET | `/api/v1/admin/analytics/search` | Search metrics |
| GET | `/api/v1/admin/analytics/toolbox` | Toolbox metrics |
| GET | `/api/v1/admin/analytics/prompt-lab` | PromptLab metrics |
| GET | `/api/v1/admin/analytics/top/content` | Top content |
| GET | `/api/v1/admin/analytics/top/courses` | Top courses |
| GET | `/api/v1/admin/analytics/top/tools` | Top tools |
| GET | `/api/v1/admin/analytics/top/prompts` | Top prompts |

## Audit

See [audit.md](audit.md).

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/audit` | List audit records |
| GET | `/api/v1/admin/audit/{id}` | Get record |
| GET | `/api/v1/admin/audit/actions` | Supported actions |
| GET | `/api/v1/admin/audit/categories` | Supported categories |

## Operations

See [operations.md](operations.md).

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/operations/status` | Operations summary |
| GET | `/api/v1/admin/operations/health` | Detailed subsystem health |
| GET | `/api/v1/admin/operations/outbox` | Outbox snapshot |
| GET | `/api/v1/admin/operations/search` | Search index snapshot |
| GET | `/api/v1/admin/operations/analytics` | Analytics ingestion snapshot |
| GET | `/api/v1/admin/operations/audit` | Audit storage snapshot |
| GET | `/api/v1/admin/operations/logging` | Logging configuration |

## Outbox recovery

See [operations.md](operations.md). Message **payloads are not exposed** in API responses.

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/admin/outbox/status` | Aggregate status |
| GET | `/api/v1/admin/outbox/messages` | List messages ([paginated](pagination.md)) |
| GET | `/api/v1/admin/outbox/messages/{id}` | Message detail (no payload) |
| POST | `/api/v1/admin/outbox/messages/{id}/retry` | Retry single message |
| POST | `/api/v1/admin/outbox/retry-failed` | Batch retry failed messages |

## Toolbox administration

See [toolbox.md](toolbox.md).

Base: `/api/v1/admin/toolbox`

Categories and tools CRUD, publish/unpublish, enable/disable, schema updates.

## PromptLab administration

See [promptlab.md](promptlab.md).

Base: `/api/v1/admin/prompt-lab`

Categories, prompt definitions, versions, publish workflow.

## Search administration

| Method | Path | Summary |
|--------|------|---------|
| POST | `/api/v1/search/manage/reindex` | Trigger search reindex |

## Authorization errors

| Status | Cause |
|--------|-------|
| 401 | Missing or invalid JWT |
| 403 | Valid JWT without Admin role |

403 on admin paths may generate audit records.

## Related

- [authentication.md](authentication.md)
- [operations.md](operations.md), [audit.md](audit.md)
