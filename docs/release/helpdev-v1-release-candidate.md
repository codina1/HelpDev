# HelpDev v1.0 — Release Candidate

**Sprint:** 46 / 46.5 — Production Certification & Release Closure v1  
**Status:** Release candidate — **Certification: PASSED**  
**Date:** 2026-07-23  
**Manifest:** `api/artifacts/release/release-manifest.json`

This document is release evidence for HelpDev **v1.0**. It does not introduce new product features.

---

## Architecture summary

HelpDev is a **modular monolith**:

- **API:** ASP.NET Core 8 (`HelpDev.API`) on PostgreSQL (Npgsql + EF Core), outbox-driven integration, pgvector search/RAG.
- **Modules:** Identity, Content (CMS + workflow + AI assistant/workflow), Learning (courses, enrollments, personalization), Search, Media, Analytics, Auditing, Administration, Toolbox, PromptLab.
- **Frontend:** Next.js App Router (`site/`) — public surfaces, user learning/dashboard/settings, Admin shell.
- **Persistence rule:** Real PostgreSQL for integration certification; no SQLite/InMemory as production substitutes.

---

## Features included (v1 surface)

| Area | Included |
|------|----------|
| Identity | OTP register/verify, JWT sessions, roles (User/Writer/Admin) |
| Content | Articles CMS, SEO metadata, revisions, workflow submit/approve/publish |
| Media | Asset upload + public URL attachment |
| Search | Lexical documents, semantic chunks/vectors, RAG answers |
| Learning | Courses, enrollments, progress, learning profile/preferences, recommendations, roadmaps |
| AI | Content assistant tasks, AI content workflow drafts, usage tracking (no new Sprint 46 AI capabilities) |
| Analytics / Audit | Event receipts, admin audit query |
| Admin / Ops | Operations health, features, CMS/SEO/media/workflow/AI/analytics screens |
| Frontend UX | Landing, search, dashboard, learning, settings, admin routes (Sprint 45 readiness) |

**Explicitly out of v1 / not in this RC:** Knowledge Graph, autonomous AI Agents, Billing, Community, new modules.

---

## Migration count

| Item | Value |
|------|-------|
| Expected applied migrations | **24** (`PostgreSqlDatabaseHelper.ExpectedMigrationCount`) |
| Production steady-state | `Database:MigrationMode=Validate`, `SeedMode=None` |
| Certification suites | `PostgreSqlSchemaCertificationE2ETests`, `CleanDatabaseMigrationTests`, `ProductionPlatformCertificationE2ETests` |

---

## Test results (certification package)

### Backend suites (Sprint 46 additions)

| Suite | Purpose |
|-------|---------|
| `PostgreSqlSchemaCertificationE2ETests` | Empty DB → all migrations; tables; indexes; PK/FK; EF model consistency |
| `ProductionPlatformCertificationE2ETests` | Full Identity→…→Audit journey + content assistant usage |
| `SecurityFinalAuditE2ETests` | OTP/JWT, authz matrix, no secret/vector/prompt leakage |
| Extended `ProductionSafetyValidatorTests` | Missing JWT identity, invalid secrets, migration/seed modes, body limits |

### Prior validation retained

- Sprint 44 platform journey, outbox, RAG, security matrix, smoke, performance, OpenAPI contract
- Sprint 45 frontend product readiness / UX tests

### Frontend certification

- `site/src/lib/release-certification.test.ts` — public/user/admin route presence vs `ADMIN_ROUTES` / critical paths
- Existing lint, typecheck, unit tests, production build

### Commands (release gate)

```bash
# Backend
dotnet restore
dotnet build -c Release
dotnet test

# Frontend (site/)
npm run lint
npm run typecheck
npm test
npm run build

# Integration (requires Docker Testcontainers or TEST_DATABASE_URL)
dotnet test --filter "Category=ProductionCertification"
```

### Verification snapshot (Sprint 46.5 — 2026-07-23)

| Gate | Result |
|------|--------|
| `dotnet build -c Release` | Passed (0 warnings / 0 errors) |
| Backend unit/API/architecture (non-PG) | **1289** passed |
| `Category=ProductionCertification` (real PostgreSQL) | **19/19** passed |
| Frontend lint | Passed (existing warnings only) |
| Frontend typecheck | Passed |
| Frontend vitest | **419** passed |
| Frontend `next build` | Passed |

### Sprint 46.5 — Production Certification Run

**Certification: PASSED**

| Item | Value |
|------|-------|
| Environment | Docker Desktop 29.2.0 + Testcontainers (`pgvector/pgvector:pg16`) |
| PostgreSQL | **16** (pgvector image) |
| Migration result | **1 → 21** applied on clean DB; schema/index/PK/FK/EF consistency asserted |
| ProductionCertification tests | **19** passed (4 artifact + 15 integration) |
| Security | `SecurityFinalAuditE2ETests` passed |
| Performance | `PerformanceCertificationEvidenceTests` passed |
| Platform journey | `ProductionPlatformCertificationE2ETests` passed |
| Schema | `PostgreSqlSchemaCertificationE2ETests` passed |

**Hardening applied during the run (no migrations / no new features):**

1. Test helper `CreateContextAsync` now calls `UseVector()` so schema certification matches production EF mapping.
2. Semantic similarity SQL quoted `"Id"` to match PostgreSQL EF column naming (`search_chunks."Id"`).

Release manifest emitted via `dotnet HelpDev.API.dll --emit-release-manifest` (`migrationCount=21`, `testCount=1308`).

---

## Deployment steps

1. Take a pre-release PostgreSQL backup ([backup-restore-validation-v1.md](../deployment/backup-restore-validation-v1.md)).
2. Deploy API artifact (Release configuration).
3. Apply migrations in a controlled window (`MigrationMode=Apply` on one instance) if pending; otherwise `Validate`.
4. Confirm production safety validation succeeds (JWT, DB, CORS, proxy, OpenAPI, logging, seed/migration modes).
5. Verify `/health/live` and `/health/ready`.
6. Deploy frontend build with correct API base URL.
7. Run smoke: OTP login, public content, search, admin CMS list, operations status.
8. Emit/update `release-manifest.json` with version, commit, timestamp, migration count (**21**), test count.

Detailed operator steps: [production-runbook-v1.md](../operations/production-runbook-v1.md).

---

## Rollback steps

1. Prefer **application-only** rollback to previous API (+ frontend if needed).
2. Keep database at current schema when backward compatible; use `MigrationMode=Validate`.
3. Confirm health probes and admin version endpoint.
4. Database restore only under change control using the backup/restore validation checklist.

See [api/docs/deployment/rollback-runbook.md](../../api/docs/deployment/rollback-runbook.md).

---

## Known limitations

- No automated backup inside the application (operator-owned).
- No distributed rate-limit / outbox lock across instances (per-instance coordination).
- OpenAPI UI disabled in Production by default.
- Offset pagination can shift under concurrent writes.
- Frontend JWT in `localStorage` (XSS hygiene required).
- Some UX foundations (notifications feed, bulk CMS) remain API-limited — see product readiness docs.
- Broader integration suite beyond `Category=ProductionCertification` should still be run in CI with Docker/`TEST_DATABASE_URL`.

Full list: [api/docs/release/known-limitations-v1.md](../../api/docs/release/known-limitations-v1.md).

---

## Certification status

**Certification: PASSED** (Sprint 46.5 — Production Certification Run v1)

| Gate | Result |
|------|--------|
| PostgreSQL schema 1→21 | **PASSED** |
| Platform journey E2E | **PASSED** |
| Production safety validator | **PASSED** (unit regression + host startup path) |
| Security final audit | **PASSED** |
| Performance sanity | **PASSED** |
| Frontend route certification | **PASSED** |
| Backup/restore drill documented | **PASSED** (operator-owned execution) |
| Release manifest current | **PASSED** |

**RC verdict:** Ready for production release from a certification perspective. Complete operator backup drill and controlled deploy per the production runbook before traffic cutover.
