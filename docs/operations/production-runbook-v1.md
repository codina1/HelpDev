# Production Runbook v1

**Sprint:** 46 — Production Certification & Release Closure v1  
**Product:** HelpDev API + Next.js site  
**Version target:** v1.0 release candidate

Companion docs:

- [helpdev-v1-release-candidate.md](../release/helpdev-v1-release-candidate.md)
- [backup-restore-validation-v1.md](../deployment/backup-restore-validation-v1.md)
- [api/docs/deployment/health-probes.md](../../api/docs/deployment/health-probes.md)
- [api/docs/deployment/rollback-runbook.md](../../api/docs/deployment/rollback-runbook.md)

---

## Startup

1. Confirm PostgreSQL is reachable and pgvector is available.
2. Confirm secrets are injected (JWT, partition HMAC, DB connection) — never commit secrets.
3. Preferred steady-state config:
   - `Database:MigrationMode=Validate`
   - `Database:SeedMode=None`
   - OpenAPI UI disabled in Production
   - Explicit CORS allow-list (HTTPS)
4. For a controlled schema upgrade window only: start one instance with `MigrationMode=Apply`, wait for success, then run remaining instances with `Validate`.
5. Start the API process (container/service).
6. Watch ordered startup logs:
   - `ApplicationStarting`
   - `ProductionSafetyValidationStarted` / success
   - `DatabaseMigrationValidationStarted` / success
   - `ApplicationStarted`
7. Probe:
   - `GET /health/live` → 200
   - `GET /health/ready` → 200 (`Ready`)
8. Start/scale the Next.js site against the API base URL.
9. Run go-live smoke checks (auth OTP, public content, admin login, search).

If production safety validation fails, **do not** force-start. Fix configuration and redeploy.

---

## Shutdown

1. Drain traffic (remove from load balancer / set desired count to zero gradually).
2. Confirm `/health/ready` returns 503 while `Stopping` (expected).
3. Stop API instances after in-flight requests drain.
4. Stop the frontend separately; it is stateless relative to PostgreSQL.
5. Do **not** shut down PostgreSQL unless performing maintenance with a backup completed.

---

## Health checks

| Probe | Auth | Meaning |
|-------|------|---------|
| `GET /health/live` | None | Process alive (no DB dependency) |
| `GET /health/ready` | None | Safe for traffic (DB + lifecycle `Ready`) |
| `GET /api/v1/admin/operations/status` | Admin JWT | Operator summary |
| `GET /api/v1/admin/operations/health` | Admin JWT | Subsystem detail |
| `GET /api/v1/admin/operations/outbox` | Admin JWT | Outbox backlog / heartbeat |
| `GET /api/v1/admin/operations/version` | Admin JWT | Release metadata |

Orchestrators should restart on sustained liveness failure; route traffic only when readiness is 200.

---

## Logs

- Prefer structured application logs; default Production level must not be Debug/Trace.
- EF sensitive-data logging must remain **off** in Production.
- Correlation IDs propagate on API responses — use them when tracing a user report.
- Never paste JWT secrets, connection strings, OTP codes, or raw AI prompts into tickets.

Useful filters:

- `ProductionSafetyValidationFailed`
- `DatabaseMigrationFailed`
- Outbox processor heartbeat / failure events
- `content.ai_task_failed` / AI provider errors (via audit + logs)

---

## Audit investigation

1. Authenticate as Admin.
2. Query `GET /api/v1/admin/audit` with time range / action filters (respect pagination).
3. High-value actions include OTP request/verify, content workflow transitions, learning AI, content AI tasks, admin operations.
4. Correlate `correlationId` from client error responses with log streams.
5. Do not export audit payloads that may contain PII to unsecured channels.

---

## Database issues

| Symptom | First actions |
|---------|----------------|
| `/health/ready` unhealthy | Check PostgreSQL connectivity, credentials, network, disk. |
| Migration validation failure | Compare `__EFMigrationsHistory` count/names to release (expect **21**). |
| Rising outbox backlog | Check outbox processor enabled; inspect failed messages; avoid duplicate multi-writer processors without understanding lock semantics. |
| Search stale after publish | Confirm outbox processed `content.published.v1`; check search operations snapshot. |
| Suspected corruption | Take a **new** backup before invasive fixes; prefer restore drill in staging. |

Destructive SQL, truncate, or restore against production requires change control. Prefer documented restore checklist in [backup-restore-validation-v1.md](../deployment/backup-restore-validation-v1.md).

---

## Rollback

Primary path: **application-only rollback** to the previous known-good binary/config while keeping the current schema (backward-compatible expectation).

1. Redeploy previous API artifact.
2. Keep `MigrationMode=Validate`.
3. Confirm health probes and admin version endpoint.
4. Redeploy previous frontend if the release included site changes.
5. Monitor error rates and outbox.

Database downgrade is last resort — see [rollback-runbook.md](../../api/docs/deployment/rollback-runbook.md).

---

## Frontend notes

- Public: `/`, content listing pages, `/search`
- User: `/dashboard`, `/learning`, `/learning/assistant`, `/settings`, `/profile`
- Admin: `/admin`, CMS/SEO/media/workflow/AI/analytics/operations routes under `/admin/*`
- Ensure `NEXT_PUBLIC_API_BASE_URL` (or project equivalent) points at the certified API.
