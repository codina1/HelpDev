# PromptLab

Prompt template catalog: browse published prompts, render with variables, and track favorites/history when authenticated.

## Public catalog

Base: `/api/v1/prompts` (alias `/api/prompts`)

| Method | Path | Auth | Summary |
|--------|------|------|---------|
| GET | `.../categories` | None | All published categories |
| GET | `...` | None | Paginated catalog (`category`, `purpose`, `search`, `page`, `pageSize`) |
| GET | `.../{slug}` | Optional | Prompt details (may vary by auth) |
| POST | `.../{slug}/render` | Optional | Render prompt with variables |

### Render

**`POST /api/v1/prompts/{slug}/render`**

- Request body limit: **128 KB**
- Rate limit: **PromptRender** (higher limit when authenticated)
- Some prompts require authentication → **401**
- Returns rendered output DTO

Pagination defaults: `pageSize` 20, max 100. See [pagination.md](pagination.md).

## Authenticated — my PromptLab

Base: `/api/v1/me` (shared route prefix with Toolbox)

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/me/prompt-favorites` | List favorites |
| PUT | `/api/v1/me/prompt-favorites/{promptId}` | Add favorite (204) |
| DELETE | `/api/v1/me/prompt-favorites/{promptId}` | Remove favorite (204) |
| GET | `/api/v1/me/prompt-history` | Paginated render history |
| GET | `/api/v1/me/prompt-history/{id}` | Single history record |

Requires Bearer JWT ([authentication.md](authentication.md)).

## Admin — PromptLab management

Base: `/api/v1/admin/prompt-lab` · Requires **Admin** role

### Categories

| Method | Path | Summary |
|--------|------|---------|
| GET | `/categories` | List |
| POST | `/categories` | Create |
| GET | `/categories/{id}` | Get |
| PUT | `/categories/{id}` | Update |
| POST | `/categories/{id}/activate` | Activate |
| POST | `/categories/{id}/deactivate` | Deactivate |

### Prompt definitions

| Method | Path | Summary |
|--------|------|---------|
| GET | `/prompts` | Paginated definitions |
| POST | `/prompts` | Create |
| GET | `/prompts/{id}` | Get |
| PUT | `/prompts/{id}` | Update |
| POST | `/prompts/{id}/enable` | Enable |
| POST | `/prompts/{id}/disable` | Disable |
| POST | `/prompts/{id}/unpublish` | Unpublish |

### Versions

| Method | Path | Summary |
|--------|------|---------|
| GET | `/prompts/{id}/versions` | List versions |
| POST | `/prompts/{id}/versions` | Create version |
| GET | `/prompts/{id}/versions/{versionNumber}` | Get version |
| POST | `/prompts/{id}/versions/{versionNumber}/publish` | Publish version |

Admin routes use **AdminMutation** rate limits.

## Writer — library prompts

Base: `/api/v1/writer/prompts` · Requires **Writer** or **Admin**

Writers create drafts and submit them for review. They cannot publish.

| Method | Path | Summary |
|--------|------|---------|
| GET | `...` | List my prompts (`status`, `page`, `pageSize`) |
| POST | `...` | Create draft |
| GET | `.../{id}` | Get my prompt |
| PUT | `.../{id}` | Update draft |
| POST | `.../{id}/submit` | Submit for review (`Draft` → `Submitted`) |

## Admin — writer prompt review

Base: `/api/v1/admin/prompts` · Requires **Admin**

Review queue for writer library prompts. Drafts are not listed. Approving a prompt publishes it (`Submitted` → `Approved`).

| Method | Path | Summary |
|--------|------|---------|
| GET | `...` | List by `status` (`Submitted`, `Approved`, `Rejected`) |
| GET | `.../{id}` | Review details |
| POST | `.../{id}/approve` | Approve and publish |
| POST | `.../{id}/reject` | Reject with required `{ "reason": "..." }` |

## Search integration

Published prompts appear in `GET /api/v1/search` when indexed.

## Related

- [public-api.md](public-api.md) — public PromptLab routes
- [authenticated-api.md](authenticated-api.md) — favorites and history
- [admin-api.md](admin-api.md) — admin PromptLab routes
- [rate-limits.md](rate-limits.md) — render limits
- [errors.md](errors.md) — PromptLab error codes (`prompt_lab_*`)
