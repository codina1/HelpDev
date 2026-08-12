# Changelog

## v1 — Initial documented API contract

First consumer-facing documentation release aligned with the implemented HelpDev API.

- **Versioning:** URL segment `/api/v1/...` as canonical; unversioned `/api/...` retained as v1 compatibility aliases with `AssumeDefaultVersionWhenUnspecified`
- **Authentication:** Mobile OTP (`send-otp`, `verify-otp`) issuing JWT Bearer tokens; Admin role for admin routes; no OAuth or API keys
- **OpenAPI:** Split documents at `/openapi/public-v1.json`, `authenticated-v1.json`, `admin-v1.json`, `all-v1.json`; exported to `api/artifacts/openapi/helpdev-*-v1.json`
- **Errors:** Canonical `{ message, code }` JSON shape (not ProblemDetails)
- **Correlation:** Optional `X-Correlation-ID` request header echoed on responses
- **Rate limits:** Policy-based 429 responses with optional `Retry-After`
- **Request sizes:** 16 KB (OTP), 128 KB (toolbox execute / prompt render), 256 KB (general/admin JSON)
- **Health:** `GET /health/live` and `GET /health/ready` for probes; `GET /api/health` deprecated
- **Pagination:** Documented per-area defaults and maximums; content list explicitly non-paginated
- **Admin surface:** Users, dashboard, announcements, feature flags, settings, analytics, audit, operations, outbox recovery, toolbox admin, PromptLab admin, search reindex
- **Public surface:** Auth, content, learning catalog, search, settings, toolbox, PromptLab, announcements
- **Authenticated surface:** Profile, content create, learning enrollments/management, toolbox/PromptLab favorites and history

Future breaking changes will be announced here with migration notes before a new major URL version ships.
