# Health Checks

HelpDev exposes anonymous health probes for orchestrators and load balancers.

## Recommended endpoints

### Liveness — `GET /health/live`

Answers: **Is the process running?**

- **200** — process is alive (`status: Healthy`)
- Response body: `{ "status": "Healthy" | "Degraded" | "Unhealthy" }`
- Minimal payload; suitable for Kubernetes liveness probes
- Not rate limited

### Readiness — `GET /health/ready`

Answers: **Can this instance accept traffic?** (dependency checks)

- **200** — `Healthy` or `Degraded`
- **503** — `Unhealthy`
- Response body: `{ "status": "..." }`
- Not rate limited

Use readiness for load balancer routing and Kubernetes readiness probes.

## Legacy endpoint (deprecated)

### `GET /api/health`

**Deprecated.** Retained for backward compatibility only.

Returns extended JSON including database connectivity:

```json
{
  "status": "Healthy",
  "service": "HelpDev API",
  "database": {
    "provider": "PostgreSQL",
    "connected": true
  }
}
```

- **200** when database is reachable (`Healthy`)
- **503** when database is unreachable (`Degraded` payload)

New integrations must use `/health/live` and `/health/ready`. See [migration-guide-v1.md](migration-guide-v1.md).

OpenAPI marks this operation `deprecated: true`.

## Admin detailed health

Operators with the **Admin** role can call:

**`GET /api/v1/admin/operations/health`**

Returns detailed subsystem status for internal dashboards — not for public probes. See [operations.md](operations.md).

## Related

- [README.md](README.md)
- [operations.md](operations.md) — admin health diagnostics
