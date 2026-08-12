# Restore Runbook

Ordered procedure to restore the HelpDev PostgreSQL database from a backup. Part of
**Sprint 25 — Deployment Hardening**.

> **Destructive-operation warning:** Restore procedures overwrite data. Every command in
> this runbook is a **placeholder**. Never run a restore against a live production
> database. Always restore into an **isolated environment first** and validate before
> switching production traffic. Do not embed real credentials anywhere.

## Prerequisites

- [ ] Confirm the incident/reason and the **target recovery point** (timestamp or backup
      ID).
- [ ] Identify the application **version** that matches the backup's schema.
- [ ] Have an isolated restore environment (separate database instance) available.
- [ ] Notify stakeholders and open a maintenance window.

## Procedure

### 1. Stop application traffic

- [ ] Remove all instances from the load balancer / mark them out of rotation.
- [ ] Confirm `GET /health/ready` is no longer routed to (readiness returns 503 while
      `Stopping`).

### 2. Stop background processing

- [ ] Stop the application instances (this stops the Outbox processor and other hosted
      services).
- [ ] Confirm graceful shutdown in logs: `ApplicationStopping` → `ApplicationStopped`.
      Watch for `HostedServiceShutdownTimedOut` (indicates a forced shutdown).

### 3. Verify backup integrity

- [ ] Verify the backup file/snapshot checksum and that it is decryptable.
- [ ] Confirm the backup corresponds to the intended recovery point.

```bash
# Placeholder — validate a custom-format dump can be listed.
pg_restore --list "helpdev-<TIMESTAMP>.dump" > /dev/null && echo "archive readable"
```

### 4. Restore into an isolated environment first

> ⚠️ Restore to a **non-production** database instance. Do **not** target the live
> database at this step.

```bash
# Placeholder — restore into an isolated/staging database.
pg_restore \
  --host="$RESTORE_PGHOST" \
  --port="$RESTORE_PGPORT" \
  --username="$RESTORE_PGUSER" \
  --dbname="$RESTORE_PGDATABASE" \
  --clean --if-exists --no-owner \
  "helpdev-<TIMESTAMP>.dump"
```

### 5. Validate schema and migration history

- [ ] Confirm the schema restored without errors.
- [ ] Inspect the EF Core migration history to record the restored schema version.
- [ ] Compare against the application version to be deployed.

> **Decision point:** If the restored schema has **pending migrations** relative to the
> target application build, `Database:MigrationMode=Validate` will fail startup. Either
> deploy a matching application version or apply migrations in a controlled window before
> proceeding. See [backup-and-restore.md](backup-and-restore.md#migration-compatibility).

### 6. Run smoke tests

- [ ] Point a single **isolated** application instance at the restored database.
- [ ] Verify `GET /health/live` = 200 and `GET /health/ready` = 200.
- [ ] Run smoke tests: OTP send/verify flow, an authenticated read, an admin read.
- [ ] Confirm no startup safety validation failures in logs
      (`ProductionSafetyValidationFailed` must be absent).

> **Decision point:** If validation or smoke tests fail, **stop**. Do not switch
> production. Reassess the backup, recovery point, or application version.

### 7. Switch application connection

- [ ] Once validated, repoint the production `ConnectionStrings__DefaultConnection` to the
      restored database (or promote the restored instance per your platform's cutover
      procedure).
- [ ] Ensure secrets are updated only via the secret provider / environment.

### 8. Start one instance

- [ ] Start a **single** application instance first.
- [ ] Watch startup log events in order: `ApplicationStarting` →
      `ProductionSafetyValidationStarted` → `DatabaseMigrationValidationStarted` →
      `ApplicationStarted`.

### 9. Verify readiness

- [ ] Confirm `GET /health/ready` returns **200** (`Ready`).
- [ ] Confirm `GET /health/live` returns **200**.
- [ ] Spot-check admin operations: `GET /api/v1/admin/operations/status` and `/outbox`.

### 10. Resume full traffic

- [ ] Scale up remaining instances and confirm each reports ready.
- [ ] Return instances to the load balancer.
- [ ] Monitor error rates, 429 rates, and outbox backlog for a stabilization period.

## Rollback decision points (summary)

| Checkpoint | If it fails |
|------------|-------------|
| Step 3 — integrity | Select a different backup / recovery point |
| Step 5 — schema/migration | Match app version or apply migrations; do not proceed blindly |
| Step 6 — smoke tests | **Abort**; keep production on prior database |
| Step 9 — readiness | Stop the instance, investigate logs, do not resume traffic |

## Post-restore

- [ ] Record actual RTO achieved.
- [ ] Capture a fresh backup of the now-current database.
- [ ] File a brief incident note and any follow-up actions.

## Related

- [backup-and-restore.md](backup-and-restore.md)
- [rollback-runbook.md](rollback-runbook.md)
- [health-probes.md](health-probes.md)
- [release-runbook.md](release-runbook.md)
