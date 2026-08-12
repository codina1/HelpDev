# Health Probes

Operator guide to HelpDev health endpoints for orchestrators, load balancers, and
on-call diagnostics. Part of **Sprint 25 — Deployment Hardening**.

Public health responses are **status-only**: no secrets, no versions, no dependency
detail. Detailed subsystem information is Admin-only.

## Probe endpoints

| Endpoint | Type | Auth | Checks | Use for |
|----------|------|------|--------|---------|
| `GET /health/live` | Liveness | None | Process only (no dependency access) | Restart decisions |
| `GET /health/ready` | Readiness | None | PostgreSQL + critical components + lifecycle state | Traffic routing |
| `GET /api/health` | Legacy | None | Deprecated | Not recommended for new probes |

### `GET /health/live` — Liveness

- Answers **"Is the process running?"**
- Process-based only; performs **no dependency access**.
- Always cheap and fast; safe for aggressive polling.
- Response body: `{ "status": "Healthy" | "Degraded" | "Unhealthy" }`.
- **Use for restart decisions** (e.g. Kubernetes `livenessProbe`).

### `GET /health/ready` — Readiness

- Answers **"Can this instance accept traffic?"**
- Includes **PostgreSQL** connectivity, critical components, and the **lifecycle
  readiness state**: `Starting`, `Ready`, `Stopping`, `Failed`.
- Returns **Unhealthy (503)** while `Starting`, `Stopping`, or `Failed`.
- Returns **200** only when the instance is `Ready`.
- **Use for traffic routing** (e.g. Kubernetes `readinessProbe`, load balancer health
  checks).

### `GET /api/health` — Legacy (deprecated)

- Retained for backward compatibility only.
- **Not recommended** for new probes; prefer `/health/live` and `/health/ready`.

## Lifecycle readiness states

`/health/ready` reflects the application lifecycle:

| State | Ready result | Meaning |
|-------|--------------|---------|
| `Starting` | 503 Unhealthy | Startup in progress; not yet accepting traffic |
| `Ready` | 200 | Fully initialized; accepting traffic |
| `Stopping` | 503 Unhealthy | Draining / shutting down |
| `Failed` | 503 Unhealthy | Startup or a critical component failed |

## Admin operations endpoints

Require a valid **Admin** JWT. These provide detailed diagnostics and are **not**
substitutes for the public probes.

| Endpoint | Purpose |
|----------|---------|
| `GET /api/v1/admin/operations/status` | High-level operations summary |
| `GET /api/v1/admin/operations/health` | Detailed subsystem health |
| `GET /api/v1/admin/operations/outbox` | Outbox operational snapshot |
| `GET /api/v1/admin/operations/search` | Search index snapshot |
| `GET /api/v1/admin/operations/analytics` | Analytics ingestion snapshot |
| `GET /api/v1/admin/operations/audit` | Audit storage snapshot |
| `GET /api/v1/admin/operations/logging` | Logging configuration snapshot |
| `GET /api/v1/admin/operations/version` | Release metadata |

## Probe configuration guidance

- **Liveness** → `/health/live`. Restart the instance only when liveness fails; do not
  attach dependency checks to liveness.
- **Readiness** → `/health/ready`. Remove the instance from rotation on 503; return it
  once it reports 200.
- Health responses use an **instance-local cache** (`Observability` settings); brief
  caching is expected and does not reflect cluster-wide state.
- Health endpoints are **exempt from rate limiting**.

### Example Kubernetes probes (illustrative)

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 10
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 5
  periodSeconds: 5
  failureThreshold: 3
```

## Startup ordering and readiness

Readiness turns `Healthy` only after the full startup sequence completes. See
[release-runbook.md](release-runbook.md) for the ordered startup phases and stable log
event names to watch during a deploy.

## Related

- [../api/health.md](../api/health.md) — consumer-facing health contract
- [../api/operations.md](../api/operations.md) — admin operations detail
- [release-runbook.md](release-runbook.md)
- [production-checklist.md](production-checklist.md)
