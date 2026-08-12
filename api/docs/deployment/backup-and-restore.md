# Backup and Restore

Policy and guidance for backing up and restoring the HelpDev PostgreSQL database. Part of
**Sprint 25 — Deployment Hardening**.

> **Scope note:** HelpDev does **not** include automated backup. Backups are the
> responsibility of the database operator / hosting provider. This document defines the
> policy and recommended procedures; it does not create schedules, credentials, or
> pipelines. Use placeholders for all commands and never embed real credentials.

## Backup responsibility

- The application persists all durable state to **PostgreSQL** (via Npgsql). There is no
  secondary datastore to back up.
- Backup scheduling, storage, encryption, and retention are owned by the **database /
  platform operator**.
- The application must be treated as **stateless**; it can be redeployed freely provided
  the database is intact.

## Backup options

### Option A — Logical backup (`pg_dump`)

Best for portability, selective restore, and moderate data sizes.

```bash
# Placeholder — set connection parameters via environment/secret manager.
pg_dump \
  --host="$PGHOST" \
  --port="$PGPORT" \
  --username="$PGUSER" \
  --format=custom \
  --file="helpdev-$(date -u +%Y%m%dT%H%M%SZ).dump" \
  "$PGDATABASE"
```

- Use `--format=custom` (or `directory`) to enable parallel/selective restore.
- Restore with `pg_restore` (see [restore-runbook.md](restore-runbook.md)).
- Capture the migration history table so schema version is verifiable after restore.

### Option B — Physical / provider snapshot

Best for large databases and fast full-cluster recovery.

- Use the hosting provider's volume/disk snapshot or PostgreSQL physical base backup.
- Snapshots are **point-in-time for the whole instance**; combine with WAL archiving for
  finer recovery granularity.
- Verify the provider snapshot is application-consistent (crash-consistent snapshots must
  replay WAL on restore).

### Point-in-time recovery (PITR)

- Where the hosting platform supports **continuous WAL archiving**, enable PITR to recover
  to a specific timestamp.
- Document the recovery target format used by the platform and validate it during restore
  drills.

## Encryption

- **At rest:** enable encryption for backup storage (provider-managed keys or
  operator-managed KMS).
- **In transit:** transfer backups over encrypted channels only.
- Restrict decryption access to the minimum set of operators.

## Retention

Define retention per environment. Suggested baseline (adjust to compliance needs):

| Backup type | Frequency | Retention |
|-------------|-----------|-----------|
| Logical (`pg_dump`) | Daily | 14–30 days |
| Provider snapshot | Daily | 7–30 days |
| WAL archive (PITR) | Continuous | Match snapshot window |
| Pre-release backup | Per release | Until next successful release verified |

- Take a **fresh backup immediately before each release** (see
  [release-runbook.md](release-runbook.md)).

## Offsite copy

- Keep at least one copy in a **separate location / region / account** from the primary
  database.
- Offsite copies protect against region or account-level loss.
- Verify offsite replication completes as part of the backup job.

## Restore testing

- **Test restores regularly** into an isolated environment — an untested backup is not a
  backup.
- Follow the full procedure in [restore-runbook.md](restore-runbook.md).
- Record restore duration to inform RTO expectations.
- Validate schema and migration history after every test restore.

## Migration compatibility

- A restored database is at the **schema version captured in the backup**. Confirm it
  matches the application version being deployed.
- With `Database:MigrationMode=Validate` (Production default), the application **fails
  startup** if the restored schema has pending migrations relative to the deployed build.
- If a restore predates required migrations, apply migrations in a controlled maintenance
  window (or deploy a matching application version) before resuming traffic.
- See [rollback-runbook.md](rollback-runbook.md) for schema-compatibility expectations
  during rollback.

## Recovery objectives (to be set per environment)

| Objective | Definition | Target |
|-----------|------------|--------|
| RPO | Max acceptable data loss | _fill in_ |
| RTO | Max acceptable downtime | _fill in_ |

## Related

- [restore-runbook.md](restore-runbook.md) — step-by-step restore
- [rollback-runbook.md](rollback-runbook.md)
- [release-runbook.md](release-runbook.md)
- [configuration-reference.md](configuration-reference.md) — `Database` settings
