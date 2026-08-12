# Public API

Endpoints callable **without** a JWT unless noted. Canonical paths use `/api/v1/...`; unversioned `/api/...` aliases work identically.

OpenAPI: [`/openapi/public-v1.json`](/openapi/public-v1.json) · Artifact: `api/artifacts/openapi/helpdev-public-v1.json`

## Authentication

| Method | Path | Summary |
|--------|------|---------|
| POST | `/api/v1/auth/send-otp` | Request login OTP (16 KB body limit) |
| POST | `/api/v1/auth/verify-otp` | Verify OTP; returns JWT (16 KB body limit) |

See [authentication.md](authentication.md).

## Health

| Method | Path | Summary |
|--------|------|---------|
| GET | `/health/live` | Liveness probe |
| GET | `/health/ready` | Readiness probe |
| GET | `/api/health` | **Deprecated** legacy health |

See [health.md](health.md).

## Announcements

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/announcements/active` | List active public announcements |

## Content

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/content` | List all published content (**not paginated**) |
| GET | `/api/v1/content/{slug}` | Get published content by slug |

## Learning (public catalog)

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/learning/courses` | List published courses (**not paginated**) |
| GET | `/api/v1/learning/courses/{id}` | Get course by ID |
| GET | `/api/v1/learning/courses/by-slug/{slug}` | Get course by slug |

## Search

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/search` | Search content, courses, tools, prompts ([paginated](pagination.md)) |

Query: `q`, `type`, `page`, `pageSize`. Dedicated search rate limits apply.

## Settings

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/settings/public` | Public system settings |

## Toolbox

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/tools/categories` | List tool categories |
| GET | `/api/v1/tools` | Paginated tool catalog |
| GET | `/api/v1/tools/{slug}` | Tool details |
| POST | `/api/v1/tools/{slug}/execute` | Execute tool (128 KB body; rate limited) |

Anonymous allowed; some tools may require authentication at execution time (**401**). See [toolbox.md](toolbox.md).

## PromptLab

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/prompts/categories` | List prompt categories |
| GET | `/api/v1/prompts` | Paginated prompt catalog |
| GET | `/api/v1/prompts/{slug}` | Prompt details |
| POST | `/api/v1/prompts/{slug}/render` | Render prompt (128 KB body; rate limited) |

See [promptlab.md](promptlab.md).

## Cross-cutting behavior

- [Correlation ID](correlation-id.md) on all routes
- [General API rate limits](rate-limits.md) plus endpoint-specific policies
- [Errors](errors.md) — `{ message, code }`
- Timestamps in UTC

Authenticated-only endpoints are documented in [authenticated-api.md](authenticated-api.md).
