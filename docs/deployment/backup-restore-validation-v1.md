# Backup & Restore Validation v1

**Sprint:** 46 — Production Certification & Release Closure v1  
**Audience:** Operators preparing HelpDev v1.0 production release  
**Scope:** Documented backup assumptions, restore checklist, and integrity verification.  
**Safety:** This document is procedural guidance only. **Do not** execute destructive restore commands automatically from CI or agent tooling.

Related runbooks:

- [api/docs/deployment/backup-and-restore.md](../../api/docs/deployment/backup-and-restore.md)
- [api/docs/deployment/restore-runbook.md](../../api/docs/deployment/restore-runbook.md)
- [api/docs/deployment/rollback-runbook.md](../../api/docs/deployment/rollback-runbook.md)

---

## Backup assumptions

| Assumption | Detail |
|------------|--------|
| Single durable store | All durable product state lives in **PostgreSQL** (Npgsql). Media files may live on disk/object storage separately. |
| No app-managed backup | HelpDev does **not** schedule or encrypt backups; the platform/DB operator owns schedule, retention, and encryption. |
| Pre-release backup required | Take a fresh logical or snapshot backup **before** each production release. |
| Migration history included | Backups must include `__EFMigrationsHistory` so schema version is verifiable after restore. |
| Extensions preserved | Production requires **pgvector** (and any other enabled extensions). Restore targets must support the same extensions. |
| Application is stateless | API instances can be replaced freely if the database (and media store) are intact. |

### Recommended logical backup (`pg_dump`)

```bash
# Placeholders only — resolve credentials from a secret manager.
pg_dump \
  --host="$PGHOST" \
  --port="$PGPORT" \
  --username="$PGUSER" \
  --format=custom \
  --file="helpdev-$(date -u +%Y%m%dT%H%M%SZ).dump" \
  "$PGDATABASE"
```

Record alongside the dump:

- HelpDev application version / release commit
- Applied migration count (expect **21** for v1.0 RC)
- Backup timestamp (UTC)
- Storage location (encrypted offline/offsite copy)

---

## Restore procedure (manual checklist)

> Perform restores in a **non-production drill environment** first. Production restores require change control.

1. [ ] Stop application traffic (remove instances from load balancer / scale to zero).
2. [ ] Confirm `/health/ready` is not receiving production traffic.
3. [ ] Provision or clear the restore target database.
4. [ ] Ensure required extensions exist (`CREATE EXTENSION IF NOT EXISTS vector;` as privileged role if needed).
5. [ ] Restore from dump:

```bash
pg_restore \
  --host="$PGHOST" \
  --port="$PGPORT" \
  --username="$PGUSER" \
  --dbname="$PGDATABASE" \
  --clean \
  --if-exists \
  "helpdev-YYYYMMDDTHHMMSSZ.dump"
```

6. [ ] Do **not** run destructive drop/recreate against the live production cluster without an approved window.
7. [ ] Start **one** API instance with `Database:MigrationMode=Validate` (preferred steady-state) or controlled `Apply` only if migrations are intentionally pending after restore.
8. [ ] Confirm startup: production safety validation → migration validation → `ApplicationStarted`.
9. [ ] Verify `/health/live` = 200 and `/health/ready` = 200.
10. [ ] Run verification queries (below) and smoke tests.
11. [ ] Restore media/object storage if assets were backed up separately.
12. [ ] Return traffic gradually; monitor error rates and outbox backlog.

---

## Migration after restore

| Scenario | Action |
|----------|--------|
| Restored DB migration count equals release expectation (21) | Keep `MigrationMode=Validate`. Do not re-apply. |
| Restored DB is behind application binary | Controlled deploy with `MigrationMode=Apply` once, then switch back to `Validate`. |
| Restored DB is ahead of application binary | Prefer application-only rollback of the binary, or restore an older compatible dump. Avoid down-migrations. |

HelpDev v1 designs adjacent releases for **backward-compatible** schema so application-only rollback remains the primary path.

---

## Verification queries

Run as a read-only role when possible. Adjust schema name if not `public`.

### Migration count

```sql
SELECT COUNT(*) AS migration_count
FROM "__EFMigrationsHistory";
-- Expect: 21 for HelpDev v1.0 RC
```

### Critical tables present

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_type = 'BASE TABLE'
  AND table_name IN (
    'users', 'contents', 'outbox_messages',
    'search_documents', 'search_chunks', 'search_vectors',
    'courses', 'enrollments', 'learning_profiles',
    'media_assets', 'audit_records', 'analytics_event_receipts',
    'ai_usage_records'
  )
ORDER BY table_name;
```

### Foreign keys / indexes sanity

```sql
SELECT COUNT(*) AS fk_count
FROM information_schema.table_constraints
WHERE table_schema = 'public'
  AND constraint_type = 'FOREIGN KEY';

SELECT COUNT(*) AS index_count
FROM pg_indexes
WHERE schemaname = 'public';
```

### Data integrity spot checks

```sql
SELECT COUNT(*) AS users FROM users;
SELECT COUNT(*) AS contents FROM contents;
SELECT COUNT(*) AS published_search FROM search_documents WHERE is_published = TRUE;
SELECT COUNT(*) AS pending_outbox FROM outbox_messages WHERE processed_at_utc IS NULL;
SELECT COUNT(*) AS audit_records FROM audit_records;
```

Investigate large pending outbox counts before returning full traffic.

---

## Data integrity checklist

- [ ] Migration count matches release evidence (**21**).
- [ ] Module tables from certification suite exist (see `PostgreSqlDatabaseHelper.ExpectedModuleTables`).
- [ ] No unexpected missing FK/PK on core tables.
- [ ] Spot-check a known admin user can authenticate (OTP flow) in a controlled drill.
- [ ] Published content appears in public content/search APIs.
- [ ] Media public URLs resolve (or object storage restore completed).
- [ ] Admin operations status endpoint returns healthy subsystems.
- [ ] Audit trail still queryable for pre-backup timestamps.
- [ ] AI usage / analytics tables readable (counts may be zero in empty drills).

Automated schema certification (non-destructive) lives in:

- `PostgreSqlSchemaCertificationE2ETests`
- `CleanDatabaseMigrationTests`
- `ProductionPlatformCertificationE2ETests`

---

## Explicit non-goals

- Automated destructive restore from CI
- Fake production seed data
- Database down-migrations as a routine rollback path
