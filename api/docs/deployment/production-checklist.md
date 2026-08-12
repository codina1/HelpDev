# Production Checklist

Go-live gate for the HelpDev backend. Part of **Sprint 25 — Deployment Hardening**.

Work through every section before promoting to Production. This checklist mirrors the
application's fail-fast startup validation — items marked **enforced** will fail startup
in Production if violated.

## Secrets and identity

- [ ] `ConnectionStrings__DefaultConnection` provided via secret/env (not committed).
      **Enforced:** required in Production.
- [ ] `Jwt__Secret` set: ≥ 32 chars, not a placeholder, provided via secret. **Enforced.**
- [ ] `Security__PartitionHashKey` set: ≥ 32 chars, not a placeholder, via secret.
      **Enforced.**
- [ ] `Jwt__Secret` and `Security__PartitionHashKey` are **different** values.
      **Enforced.**
- [ ] Placeholder blocklist avoided: `changeme`, `secret`, `password`, `test`,
      `dev-secret`, `your-secret-here`, `replace-me`, `change_in_production`, `dev_secret`.

## Transport security

- [ ] `Security__RequireHttpsMetadata=true`. **Enforced.**
- [ ] `Https:RedirectToHttps=true` (default) — HTTPS redirection applies outside
      Development.
- [ ] `Https:EnableHsts=true` (default); `Https:HstsMaxAgeDays` within `0`–`730`.
- [ ] HSTS `IncludeSubDomains` / `Preload` set per policy.

## CORS

- [ ] CORS origins are **absolute HTTPS URIs** with no path. **Enforced** (HTTPS required
      in Production).
- [ ] No wildcard `*`. **Enforced.**
- [ ] No `localhost` origins unless this is a controlled environment.

## Reverse proxy (if fronted by a proxy)

- [ ] `ReverseProxy__Enabled` set correctly (default `false`).
- [ ] If enabled in Production: at least one `ReverseProxy:TrustedProxyAddresses` **or**
      `ReverseProxy:TrustedProxyNetworks` (CIDR) configured. **Enforced.**
- [ ] `ReverseProxy:ForwardLimit` (`1`–`8`) matches the number of proxies.
- [ ] `ReverseProxy:RequireForwardedProto` / `RequireKnownProxyConfiguration` reviewed.

## Database

- [ ] `Database__MigrationMode=Validate` (Production default) — or `Apply` only if a
      migration is intentionally being applied.
- [ ] `Database__SeedMode=None`. **Enforced:** `DevelopmentDemo` is forbidden in
      Production.
- [ ] Connection pool sized: `Database:Postgres:MaxPoolSize` (recommended **50–100**)
      aligned with PostgreSQL `max_connections`.
- [ ] Timeouts reviewed (`CommandTimeoutSeconds`, `ConnectionTimeoutSeconds`).
- [ ] `EnableRetryOnFailure=false` (unless retry-aware transaction handling is confirmed).
- [ ] A fresh backup exists immediately before deploy
      ([backup-and-restore.md](backup-and-restore.md)).

## Observability and logging

- [ ] `Logging:EnableSensitiveDataLogging=false`. **Enforced.**
- [ ] `Logging:EnableDetailedErrors=false`. **Enforced.**
- [ ] `Logging:LogLevel:Default` is not `Debug`/`Trace`. **Enforced.**
- [ ] Health probes wired: liveness → `/health/live`, readiness → `/health/ready`
      ([health-probes.md](health-probes.md)).
- [ ] Note: rate limiting, health cache, and Outbox heartbeat are **instance-local**; no
      external observability exporter is bundled
      ([../release/known-limitations-v1.md](../release/known-limitations-v1.md)).

## OpenAPI

- [ ] `OpenApi__EnableInProduction=false` (default) unless intentionally exposing specs.
- [ ] `OpenApi:ExposeAdminDocumentInProduction=false`.
- [ ] `OpenApi:EnableTryItOutInProduction=false`.

## Outbox

- [ ] `Outbox__Enabled=true` (default) unless intentionally disabled.
- [ ] Batch/poll/lock/attempt settings reviewed for the environment.

## Shutdown

- [ ] `Shutdown:TimeoutSeconds` (`1`–`300`, default `30`) appropriate for drain time.

## Release metadata (optional)

- [ ] `RELEASE_VERSION`, `RELEASE_COMMIT`, `RELEASE_BUILD_TIMESTAMP`, `RELEASE_CHANNEL`
      set (bounded, safe characters) or intentionally left to assembly metadata.

## Startup verification (post-deploy)

- [ ] Ordered startup events observed: `ApplicationStarting` →
      `ProductionSafetyValidationStarted` → `DatabaseMigrationValidationStarted` →
      `ApplicationStarted`.
- [ ] No `ProductionSafetyValidationFailed` / `DatabaseMigrationFailed`.
- [ ] `/health/ready` = 200 on all instances.
- [ ] Smoke tests pass (OTP flow, authenticated read, admin read).

## Operational readiness

- [ ] Backup/restore procedure tested ([restore-runbook.md](restore-runbook.md)).
- [ ] Rollback plan reviewed ([rollback-runbook.md](rollback-runbook.md)).
- [ ] Release runbook followed ([release-runbook.md](release-runbook.md)).
- [ ] Known limitations acknowledged
      ([../release/known-limitations-v1.md](../release/known-limitations-v1.md)).

## Related

- [configuration-reference.md](configuration-reference.md)
- [environment-variables.md](environment-variables.md)
- [../release/rc-v1-checklist.md](../release/rc-v1-checklist.md)
