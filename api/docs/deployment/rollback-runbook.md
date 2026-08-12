# Rollback Runbook

Procedure to safely revert a HelpDev backend release. Part of **Sprint 25 — Deployment
Hardening**.

> **Guiding principle:** Prefer **application-only rollback**. The database schema is
> expected to be **backward compatible** across adjacent releases, so reverting the
> application binary is the primary and safest rollback path. Avoid database downgrades.

## When to roll back

Roll back when a release exhibits:

- `ProductionSafetyValidationFailed` or `DatabaseMigrationFailed` at startup.
- `/health/ready` not reaching 200 within the expected window.
- Failed smoke tests, or a sharp rise in error/429 rates or Outbox backlog.

## Application-only rollback (primary path)

This is the default and preferred rollback.

### 1. Redeploy the previous build

- [ ] Redeploy the previously known-good application version.
- [ ] Keep configuration unchanged unless the new release introduced a config change that
      must also be reverted.
- [ ] Restore the previous `RELEASE_*` metadata values if used.

### 2. Bring up one instance

- [ ] Start a single instance.
- [ ] Confirm ordered startup: `ApplicationStarting` →
      `ProductionSafetyValidationStarted` → `DatabaseMigrationValidationStarted` →
      `ApplicationStarted`.
- [ ] With `Database:MigrationMode=Validate`, confirm the previous build validates
      cleanly against the current schema (backward-compatible expectation).

### 3. Verify readiness

- [ ] `GET /health/live` = 200 and `GET /health/ready` = 200 (`Ready`).
- [ ] `GET /api/v1/admin/operations/version` reflects the rolled-back version.

### 4. Restore full traffic

- [ ] Scale up remaining instances; confirm each is `Ready`.
- [ ] Return instances to rotation and monitor.

## Backward-compatible schema expectation

- Releases are designed so the **prior application version runs against the newer
  schema**. Additive, backward-compatible migrations make application-only rollback safe.
- If the previous build fails `Validate` against the current schema, the schema is **not**
  backward compatible for that pair — treat as a database-involved rollback (below) and
  escalate.

## Database rollback risks

Database downgrades are **high risk** and are the last resort.

- Down-migrations can **drop columns/tables and lose data** written after the upgrade.
- Data written by the new version may be incompatible with the old schema.
- **This runbook does not provide down-migration scripts.** Do not generate or run ad hoc
  down-migrations under incident pressure.

### When NOT to downgrade the database

- When the schema change was **additive / backward compatible** (application-only
  rollback is sufficient).
- When new data has been written that a downgrade would discard.
- When no verified, tested down-path exists.

### Restoring a backup as last resort

If application-only rollback is insufficient and the schema is genuinely incompatible:

- [ ] Treat this as a data-loss event for changes since the backup.
- [ ] Follow [restore-runbook.md](restore-runbook.md) in full (isolated restore, validate,
      smoke test, cutover).
- [ ] Choose the recovery point deliberately and communicate expected data loss.

## Outbox compatibility

- The Outbox stores serialized domain events. When rolling back the application, ensure
  the previous version can process any messages enqueued by the newer version.
- Check `GET /api/v1/admin/operations/outbox` after rollback: the backlog should **drain**,
  not grow, and `failed` counts should not climb.
- If the older build cannot process newer message types, quarantine affected messages via
  the admin Outbox tooling and escalate rather than deleting payloads.

## Versioned API compatibility

- The API is **v1**, with legacy unversioned `/api/...` routes mapping to the same v1
  handlers. Rolling back within v1 preserves the public contract for clients.
- Avoid rollbacks that would remove a v1 endpoint clients now depend on; coordinate with
  consumers if the contract must change.

## Verification after rollback

- [ ] `/health/live` and `/health/ready` = 200 on all instances.
- [ ] Smoke tests pass (OTP flow, authenticated read, admin read).
- [ ] Error/429 rates return to baseline.
- [ ] Outbox backlog drains.
- [ ] No `ProductionSafetyValidationFailed` / `DatabaseMigrationFailed` in logs.
- [ ] Record the rollback, root cause, and follow-up actions.

## Related

- [release-runbook.md](release-runbook.md)
- [restore-runbook.md](restore-runbook.md)
- [backup-and-restore.md](backup-and-restore.md)
- [health-probes.md](health-probes.md)
