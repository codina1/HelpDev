# Release Runbook

Ordered procedure to deploy a HelpDev backend release to Production. Part of
**Sprint 25 — Deployment Hardening**.

This runbook assumes a **backward-compatible schema** and standard rolling deployment.
For undoing a release, see [rollback-runbook.md](rollback-runbook.md).

## Pre-release

- [ ] Release candidate signed off — see [../release/rc-v1-checklist.md](../release/rc-v1-checklist.md).
- [ ] Confirm commit/tag and build version to deploy.
- [ ] Confirm **0 warnings / 0 errors** build and full test suite green.
- [ ] Confirm migration state: verify whether the release **requires no migration** or a
      backward-compatible migration.
- [ ] Confirm configuration for the target environment (see
      [production-checklist.md](production-checklist.md)).
- [ ] Take a **fresh database backup** immediately before deploy
      ([backup-and-restore.md](backup-and-restore.md)).
- [ ] Set release metadata env vars if used: `RELEASE_VERSION`, `RELEASE_COMMIT`,
      `RELEASE_BUILD_TIMESTAMP`, `RELEASE_CHANNEL`.

## Configuration validation

The application performs fail-fast validation at startup. Confirm these will pass in the
target environment:

- [ ] `ConnectionStrings__DefaultConnection` set (secret).
- [ ] `Jwt__Secret` and `Security__PartitionHashKey` set, ≥ 32 chars, non-placeholder,
      and **different** from each other.
- [ ] `Security__RequireHttpsMetadata=true`.
- [ ] CORS origins are absolute HTTPS URIs (no `*`, no path).
- [ ] `Database__MigrationMode=Validate` (or `Apply` only if a migration is intended).
- [ ] `Database__SeedMode=None` (`DevelopmentDemo` is forbidden in Production).
- [ ] `OpenApi__EnableInProduction=false` (unless intentionally enabled).
- [ ] `Logging:EnableSensitiveDataLogging=false`, `EnableDetailedErrors=false`,
      log level not `Debug`/`Trace`.

## Startup sequence (what a healthy boot looks like)

The application starts in this fixed order. Watch the stable log event names:

| Phase | Stable log events |
|-------|-------------------|
| 1. Config load | `ApplicationStarting` |
| 2. Options validation (`ValidateOnStart`) | — |
| 3. Centralized `ProductionSafetyValidator` | `ProductionSafetyValidationStarted`, `ProductionSafetyValidationFailed` (on failure) |
| 4. Database connectivity / migration policy | `DatabaseMigrationValidationStarted`, `DatabaseMigrationPending`, `DatabaseMigrationApplyStarted`, `DatabaseMigrationCompleted`, `DatabaseMigrationFailed` |
| 5. Required system seed | — |
| 6. Host start | `ApplicationStarted` |
| 7. Hosted processors (Outbox) | — |
| 8. Readiness `Healthy` | `/health/ready` returns 200 |

Shutdown events: `ApplicationStopping`, `ApplicationStopped`,
`HostedServiceShutdownTimedOut` (on forced shutdown).

## Deploy

### 1. Deploy the new build

- [ ] Roll out the new build with the validated configuration.
- [ ] Keep the previous build/version available for rollback.

### 2. Bring up one instance (canary)

- [ ] Start a single instance first.
- [ ] Confirm ordered startup events through `ApplicationStarted`.
- [ ] Confirm **no** `ProductionSafetyValidationFailed` or `DatabaseMigrationFailed`.

### 3. Verify readiness

- [ ] `GET /health/live` = 200.
- [ ] `GET /health/ready` = 200 (`Ready`).
- [ ] `GET /api/v1/admin/operations/version` reflects expected release metadata.
- [ ] `GET /api/v1/admin/operations/status` and `/outbox` look nominal.

### 4. Smoke tests

- [ ] OTP send/verify flow succeeds.
- [ ] One authenticated read succeeds.
- [ ] One admin read succeeds.

### 5. Roll out remaining instances

- [ ] Scale up; confirm each instance reaches `Ready`.
- [ ] Return all instances to rotation.

## Post-deploy verification

- [ ] Monitor error rates and 429 rates for a stabilization window.
- [ ] Monitor Outbox backlog (`GET /api/v1/admin/operations/outbox`) — pending/failed
      counts should drain, not grow.
- [ ] Confirm log volume/levels are as expected (no sensitive data logging).
- [ ] Confirm HTTPS/HSTS behavior at the edge.

## Migration handling

- **No migration required** (preferred RC state): deploy with
  `Database__MigrationMode=Validate`; startup validates schema and proceeds.
- **Backward-compatible migration**: apply via `Database__MigrationMode=Apply` in a
  controlled step. `Apply` uses a PostgreSQL advisory lock (key `4207770001`) with a
  bounded timeout (`Database:MigrationLockTimeoutSeconds`), so concurrent instances
  coordinate safely. Prefer applying before or during a low-traffic window.

## Abort / rollback triggers

Abort the release and follow [rollback-runbook.md](rollback-runbook.md) if any of:

- Canary logs `ProductionSafetyValidationFailed` or `DatabaseMigrationFailed`.
- `/health/ready` does not reach 200 within the expected window.
- Smoke tests fail.
- Error/429 rates or Outbox backlog rise sharply post-deploy.

## Related

- [rollback-runbook.md](rollback-runbook.md)
- [production-checklist.md](production-checklist.md)
- [../release/rc-v1-checklist.md](../release/rc-v1-checklist.md)
- [health-probes.md](health-probes.md)
- [backup-and-restore.md](backup-and-restore.md)
