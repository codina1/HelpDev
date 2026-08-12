# Authenticated API

Endpoints requiring a valid **JWT Bearer** token (`Authorization: Bearer {token}`) unless a role is specified.

OpenAPI: [`/openapi/authenticated-v1.json`](/openapi/authenticated-v1.json) · Artifact: `api/artifacts/openapi/helpdev-authenticated-v1.json`

Obtain a token via [authentication.md](authentication.md).

## Profile

| Method | Path | Role | Summary |
|--------|------|------|---------|
| GET | `/api/v1/profile/me` | Authenticated | Get my profile |
| PUT | `/api/v1/profile/me` | Authenticated | Update my profile |

## Content

| Method | Path | Role | Summary |
|--------|------|------|---------|
| POST | `/api/v1/content` | Writer or Admin | Create content item |

## Learning — enrollments & progress

| Method | Path | Role | Summary |
|--------|------|------|---------|
| POST | `/api/v1/learning/courses/{courseId}/enroll` | Authenticated | Enroll in course |
| GET | `/api/v1/learning/me/enrollments` | Authenticated | List my enrollments |
| GET | `/api/v1/learning/me/enrollments/{enrollmentId}` | Authenticated | Get enrollment |
| GET | `/api/v1/learning/me/enrollments/by-course/{courseId}` | Authenticated | Get enrollment by course |
| POST | `/api/v1/learning/courses/{courseId}/lessons/{lessonId}/start` | Authenticated | Start lesson |
| POST | `/api/v1/learning/courses/{courseId}/lessons/{lessonId}/complete` | Authenticated | Complete lesson |

## Learning — course management

Requires **Writer** or **Admin** role.

Base path: `/api/v1/learning/manage/courses`

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/learning/manage/courses` | List manageable courses |
| POST | `/api/v1/learning/manage/courses` | Create course |
| GET | `/api/v1/learning/manage/courses/{id}` | Get course |
| PUT | `/api/v1/learning/manage/courses/{id}` | Update course |
| DELETE | `/api/v1/learning/manage/courses/{id}` | Delete course |
| POST | `/api/v1/learning/manage/courses/{id}/publish` | Publish course |
| POST | `/api/v1/learning/manage/courses/{id}/sections` | Add section |
| PUT | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}` | Update section |
| DELETE | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}` | Delete section |
| POST | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}/order` | Reorder section |
| POST | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}/lessons` | Add lesson |
| PUT | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}/lessons/{lessonId}` | Update lesson |
| DELETE | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}/lessons/{lessonId}` | Delete lesson |
| POST | `/api/v1/learning/manage/courses/{id}/sections/{sectionId}/lessons/{lessonId}/order` | Reorder lesson |

## Toolbox — my data

See [toolbox.md](toolbox.md).

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/me/tool-favorites` | List favorites |
| PUT | `/api/v1/me/tool-favorites/{toolId}` | Add favorite |
| DELETE | `/api/v1/me/tool-favorites/{toolId}` | Remove favorite |
| GET | `/api/v1/me/tool-history` | Paginated execution history |
| GET | `/api/v1/me/tool-history/{id}` | Get history item |

## PromptLab — my data

See [promptlab.md](promptlab.md).

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/me/prompt-favorites` | List favorites |
| PUT | `/api/v1/me/prompt-favorites/{promptId}` | Add favorite |
| DELETE | `/api/v1/me/prompt-favorites/{promptId}` | Remove favorite |
| GET | `/api/v1/me/prompt-history` | Paginated render history |
| GET | `/api/v1/me/prompt-history/{id}` | Get history item |

## Secure probe

| Method | Path | Summary |
|--------|------|---------|
| GET | `/api/v1/secure` | Authenticated smoke endpoint |

## Public endpoints with optional auth

These live in the [public catalog](public-api.md) but accept an optional Bearer token for personalization or higher rate limits:

- `GET /api/v1/content/{slug}`
- `GET /api/v1/prompts/{slug}`
- `POST /api/v1/tools/{slug}/execute`
- `POST /api/v1/prompts/{slug}/render`
- `GET /api/v1/search`

## Admin endpoints

Admin-only routes are in [admin-api.md](admin-api.md).

## Related

- [authentication.md](authentication.md)
- [errors.md](errors.md) — 401 vs 403
- [pagination.md](pagination.md)
