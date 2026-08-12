# Go-Live Smoke Tests

Non-destructive smoke test plan executed against a freshly deployed HelpDev instance before
returning it to traffic. Part of **Sprint 26 — Production Deployment Validation &
Go-Live Readiness**.

> **All checks are read-only or use approved test data.** Do not run destructive
> operations. Where a check requires authentication, use an **approved test mobile number**
> provisioned for go-live validation, and clean up any created data afterward.

Set a base URL for convenience:

```bash
BASE=https://api.<your-domain>
```

Replace `<...>` placeholders. Admin checks require an Admin JWT (`$ADMIN`); authenticated
checks require a user JWT (`$TOKEN`).

## 1. Pre-traffic

| Check | Command / endpoint | Expected safe result |
|-------|--------------------|----------------------|
| Config validation | `dotnet HelpDev.API.dll --validate-production-config` | Exit `0`; `Production safety validation passed.` No secrets printed. |
| DB migration validation | `dotnet HelpDev.API.dll --validate-production-config --validate-database` | `provider=PostgreSQL reachable=true`, `pendingMigrations=0`, `mutation=none`. |
| Liveness | `GET $BASE/health/live` | `200`, process alive. |
| Readiness | `GET $BASE/health/ready` | `200` (`Ready`) once initialization completes. |
| Release version | `GET $BASE/api/v1/admin/operations/version` (Admin) | Expected `version`/`commit`; matches the release manifest. |
| PostgreSQL connectivity | `GET $BASE/health/ready` + `--validate-database` above | Readiness `200` confirms DB reachable. |
| Outbox state | `GET $BASE/api/v1/admin/operations/outbox` (Admin) | Nominal; pending/failed counts drain, not grow. |

## 2. Public (unauthenticated, read-only)

| Area | Endpoint | Expected safe result |
|------|----------|----------------------|
| Content | `GET $BASE/api/v1/content` (or a public content read) | `200` with a content list/summary. |
| Search | `GET $BASE/api/v1/search?query=<term>` | `200`; results (eventually consistent via Outbox). |
| Learning | `GET $BASE/api/v1/learning/...` (public read) | `200` with learning content. |
| Toolbox | `GET $BASE/api/v1/toolbox/tools` | `200`; tool catalog. |
| PromptLab | `GET $BASE/api/v1/promptlab/...` (public read) | `200`; prompt templates/metadata. |

## 3. Authentication (approved test number)

| Step | Endpoint | Expected safe result |
|------|----------|----------------------|
| OTP request | `POST $BASE/api/v1/auth/send-otp` `{ "mobile": "<approved-test-mobile>" }` | `200`; OTP **not** exposed in the response. |
| OTP verify | `POST $BASE/api/v1/auth/verify-otp` `{ "mobile": "<approved-test-mobile>", "code": "<code>" }` | `200`; returns access token + user. |
| Profile | `GET $BASE/api/v1/profile` (Bearer `$TOKEN`) | `200`; the test user's profile. |
| Logout / expiry | Discard token; call a protected route with an expired/absent token | `401` (`authentication_required`). |

## 4. Admin (Admin JWT, read-only)

| Area | Endpoint | Expected safe result |
|------|----------|----------------------|
| Version | `GET $BASE/api/v1/admin/operations/version` | `200`; release metadata. |
| Operations | `GET $BASE/api/v1/admin/operations/status` | `200`; nominal subsystem status. |
| Audit read | `GET $BASE/api/v1/admin/audit?...` | `200`; audit page (read-only). |
| Analytics read | `GET $BASE/api/v1/admin/analytics?...` | `200`; analytics summary (read-only). |

## 5. Security

| Check | How | Expected safe result |
|-------|-----|----------------------|
| HTTPS | Request `http://` public host | Redirects to HTTPS; no redirect loop. |
| HSTS | Inspect response headers on HTTPS | `Strict-Transport-Security` present. |
| CORS | Preflight from allowed origin vs. disallowed origin | Allowed origin permitted; others rejected; no wildcard. |
| Headers | Inspect security/correlation headers | Security headers present; `X-Correlation-ID` echoed. |
| 401 | Protected route without token | `401` (`authentication_required`). |
| 403 | Authenticated non-admin hits admin route | `403` (`access_denied`). |
| 413 | Oversized body to a limited route | `413` (`security_request_too_large`). |
| 429 | Exceed a rate-limit policy | `429` (`security_rate_limit_exceeded`); honor `Retry-After`. |

## Pass criteria

- All pre-traffic checks pass; readiness is `200` on every instance.
- Public reads, the authenticated OTP flow, and admin reads succeed.
- Security behaviors (HTTPS/HSTS/CORS/headers/401/403/413/429) are as expected.
- Any data created by the auth flow is cleaned up.

Record results in the release evidence — see
[release-evidence-template.md](release-evidence-template.md).

## Related

- [go-live-checklist.md](go-live-checklist.md)
- [../deployment/release-runbook.md](../deployment/release-runbook.md)
- [../deployment/health-probes.md](../deployment/health-probes.md)
- [../api/errors.md](../api/errors.md)
