# Toolbox

Developer tools catalog: browse published tools, execute with input, and manage favorites/history when authenticated.

## Public catalog

Base: `/api/v1/tools` (alias `/api/tools`)

| Method | Path | Auth | Summary |
|--------|------|------|---------|
| GET | `.../categories` | None | All published categories |
| GET | `...` | None | Paginated catalog (`category`, `search`, `page`, `pageSize`) |
| GET | `.../{slug}` | None | Tool details |
| POST | `.../{slug}/execute` | Optional | Execute tool |

### Execute

**`POST /api/v1/tools/{slug}/execute`**

- Request body limit: **128 KB**
- Rate limit: **ToolboxExecution** (higher limit when authenticated)
- Some tools require authentication → **401** with `toolbox_tool_requires_authentication`
- Returns execution result DTO

Pagination defaults: `pageSize` 20, max 100. See [pagination.md](pagination.md).

## Authenticated — my toolbox

Base: `/api/v1/me` (shared route prefix with PromptLab)

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/me/tool-favorites` | List favorites |
| PUT | `/api/v1/me/tool-favorites/{toolId}` | Add favorite (204) |
| DELETE | `/api/v1/me/tool-favorites/{toolId}` | Remove favorite (204) |
| GET | `/api/v1/me/tool-history` | Paginated history (`toolId`, `succeeded`, `page`, `pageSize`) |
| GET | `/api/v1/me/tool-history/{id}` | Single history record |

Requires Bearer JWT ([authentication.md](authentication.md)).

## Admin — toolbox management

Base: `/api/v1/admin/toolbox` · Requires **Admin** role

### Categories

| Method | Path | Summary |
|--------|------|---------|
| GET | `/categories` | List |
| POST | `/categories` | Create |
| GET | `/categories/{id}` | Get |
| PUT | `/categories/{id}` | Update |
| POST | `/categories/{id}/activate` | Activate |
| POST | `/categories/{id}/deactivate` | Deactivate |

### Tools

| Method | Path | Summary |
|--------|------|---------|
| GET | `/tools` | Paginated definitions |
| POST | `/tools` | Create |
| GET | `/tools/{id}` | Get |
| PUT | `/tools/{id}` | Update |
| PUT | `/tools/{id}/schema` | Update input schema |
| POST | `/tools/{id}/publish` | Publish |
| POST | `/tools/{id}/unpublish` | Unpublish |
| POST | `/tools/{id}/enable` | Enable |
| POST | `/tools/{id}/disable` | Disable |

Admin routes use **AdminMutation** rate limits.

## Search integration

Published tools appear in `GET /api/v1/search` when indexed.

## Related

- [public-api.md](public-api.md) — public toolbox routes
- [authenticated-api.md](authenticated-api.md) — favorites and history
- [admin-api.md](admin-api.md) — admin toolbox routes
- [rate-limits.md](rate-limits.md) — execution limits
- [errors.md](errors.md) — toolbox error codes (`toolbox_*`)
