# Rate Limits

HelpDev applies **sliding-window** rate limits per policy. Limits are configurable in `RateLimiting` application settings; values below reflect **default deployment configuration**.

When exceeded, the API returns **429** with `code: security_rate_limit_exceeded` and optionally `Retry-After` (seconds). See [errors.md](errors.md).

## Policy overview

| Policy | Applied to | Default window | Default permit limit |
|--------|------------|------------------|----------------------|
| **GeneralApi** | All mapped controllers (baseline) | 60 s | 120 |
| **OtpRequest** | `POST .../auth/send-otp` | 900 s (15 min) | 5 |
| **OtpVerify** | `POST .../auth/verify-otp` | 900 s | 10 |
| **Search** | `GET .../search` (authenticated) | 60 s | 60 |
| **Search** (anonymous) | Same endpoint, no JWT | 60 s | 30 |
| **ToolboxExecution** | `POST .../tools/{slug}/execute` (authenticated) | 60 s | 30 |
| **ToolboxExecution** (anonymous) | Same, no JWT | 60 s | 10 |
| **PromptRender** | `POST .../prompts/{slug}/render` (authenticated) | 60 s | 30 |
| **PromptRender** (anonymous) | Same, no JWT | 60 s | 10 |
| **PublicContentRead** | Content controller reads | 60 s | 180 |
| **AdminMutation** | All `/api/.../admin/...` actions | 60 s | 60 |
| **Authentication** | Reserved policy (general auth traffic) | 300 s | 20 |

Endpoint-specific policies **override** the controller baseline where `[EnableRateLimiting(...)]` or conventions apply.

## OTP

OTP endpoints use the strictest limits to reduce abuse:

- Separate policies for **request** vs **verify**
- Additional network-level throttling may apply for repeated send-otp calls from the same origin

Clients should not implement OTP resend loops; respect **429** and `Retry-After`.

## Search

`GET /api/v1/search` uses the **Search** policy. Authenticated callers receive a higher permit limit than anonymous callers on the same endpoint.

## Toolbox and PromptLab

Execution and render are rate-limited independently of catalog browsing:

- **ToolboxExecution** — tool execute only
- **PromptRender** — prompt render only

Catalog list/detail endpoints use **GeneralApi** (plus general limits on the controller pipeline).

## Admin

All routes under `/api/v1/admin/` (and unversioned `/api/admin/`) attach **AdminMutation** via controller convention. Limits apply to reads and writes alike.

Admin endpoints require a valid JWT with the **Admin** role; limits are scoped to authenticated admin traffic.

## Health and legacy health

`GET /health/live`, `GET /health/ready`, and `GET /api/health` are **exempt** from rate limiting.

## Implementation notes (consumer-facing)

- Limits use sliding windows, not fixed daily quotas
- Partitioning distinguishes authenticated users from anonymous traffic where policies define both tiers
- Internal partition keys, HMAC salting, and client IP handling are implementation details and not part of the public contract
- Limits may be tuned per environment without a version bump; monitor **429** rates in production

## Related

- [authentication.md](authentication.md) — OTP limits
- [errors.md](errors.md) — 429 handling
- [toolbox.md](toolbox.md), [promptlab.md](promptlab.md) — execute/render limits
