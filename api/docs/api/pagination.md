# Pagination

## Query parameters

Paginated list endpoints accept:

| Parameter | Rules |
|-----------|-------|
| `page` | 1-based; must be ≥ 1 |
| `pageSize` | Must be ≥ 1 and ≤ area maximum |

Invalid values return **400** with a domain-specific error code and message.

## Defaults and maximums by area

| Area | Default `pageSize` | Max `pageSize` |
|------|-------------------|----------------|
| Search | 20 | 50 |
| Toolbox catalog & history | 20 | 100 |
| PromptLab catalog & history | 20 | 100 |
| Admin announcements | 20 | 100 |
| Admin outbox messages | 20 | 100 |
| Admin audit records | 20 | 100 |
| Admin toolbox/prompt definitions | 20 | 100 |

## Non-paginated lists

These endpoints return a full array in one response (no `page` / `pageSize`):

| Endpoint | Notes |
|----------|-------|
| `GET /api/v1/content` | All published content items |
| `GET /api/v1/learning/courses` | All published courses |
| `GET /api/v1/tools/categories` | All published categories |
| `GET /api/v1/prompts/categories` | All published categories |
| `GET /api/v1/announcements/active` | Active announcements |
| `GET /api/v1/admin/users` | All users (admin) |
| `GET /api/v1/me/tool-favorites` | User favorites |
| `GET /api/v1/me/prompt-favorites` | User favorites |

## Response shape

Paged responses typically include:

```json
{
  "page": 1,
  "pageSize": 20,
  "total": 142,
  "items": [ ... ]
}
```

Exact DTO names vary by module (`ToolCatalogPageDto`, `SearchResultDto`, `AuditPageResult`, etc.). Refer to OpenAPI schemas for field details.

## Audit date filters

`GET /api/v1/admin/audit` supports optional `from` / `to` (UTC). Maximum range: **366 days**. Invalid ranges return **400**.

## Related

- [public-api.md](public-api.md), [admin-api.md](admin-api.md) — which lists are paginated
- [audit.md](audit.md) — audit paging and filters
