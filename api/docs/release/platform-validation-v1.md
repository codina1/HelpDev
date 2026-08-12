# Platform Validation v1 — Sprint 44 Release Evidence

**Sprint:** 44 — Full Platform Integration & Release Validation v1  
**Scope:** End-to-end validation of the HelpDev modular monolith against **real PostgreSQL**.  
**Non-goals:** No new business features, no schema redesign, no auth bypass, no fake persistence.

## Architecture status

| Layer | Status |
|-------|--------|
| Modular monolith (Identity, Content, Learning, Search, Media, Analytics, Auditing, Administration, Toolbox, PromptLab, AI usage) | Validated via cross-module E2E |
| Outbox → processor → handlers | Validated (`OutboxPipelineE2ETests`) |
| RAG (chunk → embed → pgvector → context → answer) | Validated (`RagPlatformE2ETests`) |
| Shared contracts / no cross-module DbContext leakage | Unchanged; exercised by platform journey |
| Frontend Admin Shell + learning assistant | Route + API client critical-flow coverage |

## Migration status

| Item | Value |
|------|-------|
| Expected applied migration count | **24** (`PostgreSqlDatabaseHelper.ExpectedMigrationCount`) |
| Latest product migration | `AddPersonalizedAiLearningV1` (Sprint 43) |
| Clean DB apply (1 → latest) | Covered by `CleanDatabaseMigrationTests` / `UpgradeMigrationTests` |
| Production mode | `Database:MigrationMode=Validate` (see `MigrationModeAndAdvisoryLockTests`, Production host settings) |
| Module tables asserted | users, contents (+ revisions/workflow/ideas/AI sessions), learning (+ personalization), search (+ vectors), outbox, analytics, audit, media, AI usage, admin/toolbox/promptlab |

Schema checks exercised in smoke: tables exist for critical modules; FK/index/constraint coverage remains in dedicated migration integration tests.

## Test coverage added (Sprint 44)

### Backend (PostgreSQL / Testcontainers)

| Suite | Purpose |
|-------|---------|
| `FullPlatformUserJourneyE2ETests` | OTP → profile → content/SEO/media → workflow publish → outbox/search → enroll → AI recommend/roadmap → AI content workflow → analytics + audit |
| `OutboxPipelineE2ETests` | Event → outbox → processor → handlers; no duplicate processing (content + learning) |
| `RagPlatformE2ETests` | Knowledge → chunks/vectors → non-empty RAG context → answer; no secret/vector leakage |
| `SecurityMatrixE2ETests` | Anonymous / User / Writer / Admin matrix; writer ownership; AI non-mutation of enrollments/profile |
| `ProductionReadinessSmokeTests` | `/health/live`, `/health/ready`, security + correlation headers, CORS, OpenAPI (non-prod), migration count + tables |
| `PerformanceSanityE2ETests` | Bounded admin lists, search page size, clamp/reject oversized pages, dashboard payload sanity |
| `ApiContractValidationE2ETests` | OpenAPI public/authenticated/admin export; `/api/v1/*` paths; OperationIds |

Supporting factory: `AuthenticatedClientFactory` Writer clients + `*WithIdAsync` helpers.

### Frontend (Vitest)

| Suite | Purpose |
|-------|---------|
| `site/src/lib/critical-flow.test.ts` | Auth OTP canonical routes; learning profile/assistant/roadmap API paths; user + admin critical pages exist |

## Modules validated

- **Identity** — OTP registration/JWT, profile authz  
- **Content** — CMS create/update/SEO/media cover, workflow, AI content workflow  
- **Learning** — enroll/progress, personalization profile/recommend/roadmap  
- **Search** — lexical projection, semantic chunks/vectors, RAG  
- **Media** — upload + public URL attach  
- **Analytics** — event receipts after outbox  
- **Auditing** — OTP + learning sensitive actions  
- **AI** — usage records; suggestion-only learning (no enrollment mutation)  
- **Admin / Ops** — features, operations status, OpenAPI gating (ProductionHostTests)

## Deployment requirements

1. PostgreSQL 16+ with **pgvector**  
2. Apply migrations before traffic (`MigrationMode=Apply` at deploy, then `Validate` in steady state)  
3. Strong distinct `Jwt:Secret` and `Security:PartitionHashKey`  
4. Explicit CORS allow-list for frontend origins  
5. Outbox processor enabled in Production  
6. OpenAPI disabled in Production by default (`OpenApi:EnableInProduction=false`)  
7. Health probes: `/health/live`, `/health/ready`  
8. Configure media storage root / object storage for Production uploads  

## Fixes applied during validation

- **Route parameter collision:** content revision routes used `{version}` alongside API `{version:apiVersion}`, which prevented Production host startup (`RoutePatternException`). Renamed to `{revisionVersion}` (URL path shape unchanged: `/revisions/{n}`).
- **Enrollment routes:** aligned `LearningEnrollmentsController` with dual class-level `[Route]` pattern used by other controllers.
- **Test compile:** `OutboxProcessor.ProcessBatchAsync` calls pass `CancellationToken.None`.

## Known limitations

See also [known-limitations-v1.md](./known-limitations-v1.md).

- Integration E2E requires Docker Testcontainers **or** `TEST_DATABASE_URL`  
- Fake AI / embedding providers are used in the test host (deterministic; not live LLM)  
- Outbox is processed **manually** in WebApplicationFactory tests (`ProcessBatchAsync`) — host background processor is disabled in the test factory  
- Offset pagination can shift under concurrent inserts  
- Feature-flag admin list is not paginated (small control-plane set)  
- Sprint 44 does not add load/soak benchmarks beyond pagination sanity  
- Full PostgreSQL E2E suite was not executed in this environment (Docker unavailable); unit/API/architecture + frontend suites were green  

## Verification commands

```bash
# Backend
dotnet restore
dotnet build -c Release
dotnet test

# Frontend
cd site
npm run lint
npm run typecheck
npm test
npm run build
```

## Sign-off checklist

- [ ] Migration count = 21 on target database  
- [ ] PostgreSQL E2E suites green  
- [ ] Production smoke + Production host OpenAPI gating green  
- [ ] Frontend lint / typecheck / test / build green  
- [ ] Operator runbook: backup + restore rehearsed  
