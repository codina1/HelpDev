# Staging Environment

Guidance for a Production-like staging environment used to validate releases before
go-live. Part of **Sprint 26 — Production Deployment Validation & Go-Live Readiness**.

Staging exists to exercise the exact production configuration path and catch environment
problems before real traffic. It must be **as close to Production as practical** so that a
pass in staging is meaningful.

## Production-like requirements

- **Environment:** `ASPNETCORE_ENVIRONMENT=Production` (or a dedicated `Staging` value that
  applies the **same production safety rules**). Do not relax safety validation.
- **Database:** a **real PostgreSQL** instance (not in-memory, not SQLite).
- **Reverse proxy & TLS:** a **real reverse proxy** terminating TLS, matching the
  [reverse-proxy-contract.md](reverse-proxy-contract.md).
- **Seeding:** **no demo seed** (`Database__SeedMode=None`).
- **OTP:** **no public/deterministic OTP** (`Auth:ExposeOtpInResponse=false`); never enable
  a deterministic OTP provider.
- **OpenAPI/Swagger:** disabled (`OpenApi__EnableInProduction=false`).
- **API surface:** canonical **`/api/v1`** routes and `/health/*` probes.
- **Request limits:** same request-body-size limits as Production (same 413 behavior).
- **CORS:** same structure — absolute HTTPS origins, no wildcard, no path — pointing at the
  staging frontend origin.
- **Secrets:** distinct, non-Production secrets injected by the host mechanism;
  `Jwt__Secret` and `Security__PartitionHashKey` still ≥ 32 chars and distinct.

## Staging database policy

- **Isolated from Production.** A separate database/instance; staging must never read or
  write Production data.
- **Non-Production credentials.** Dedicated staging credentials, never Production secrets.
- **No un-sanitized Production personal data.** If Production-derived data is used, it must
  be sanitized/anonymized first.
- **Apply only during a controlled migration step.** Use
  `dotnet HelpDev.API.dll --apply-migrations` in a deliberate step; normal startup uses
  `Database__MigrationMode=Validate`.
- **Validate during normal startup.** Routine boots validate schema and fail on pending
  migrations, mirroring Production.
- **Backup before migration validation.** Take a backup before applying/validating
  migrations so staging can be restored if a migration is rejected.
- **Smoke-test data cleanup permitted.** Data created by smoke tests may be cleaned up
  afterward.
- **No destructive tests.** Do not run destructive or load-to-failure tests against a
  shared staging database; keep validation non-destructive and read-oriented.

## Validate the staging release

```bash
# Configuration + production safety (no server start, no secrets printed)
dotnet HelpDev.API.dll --validate-production-config

# Additionally verify PostgreSQL connectivity and migration counts (non-mutating)
dotnet HelpDev.API.dll --validate-production-config --validate-database
```

Then run the go-live smoke tests against staging — see
[../release/go-live-smoke-tests.md](../release/go-live-smoke-tests.md).

## Intentional differences from Production

These differences are expected and acceptable, provided the safety posture is unchanged:

- **Hostnames/origins:** staging domains and CORS origins differ from Production.
- **Secrets:** distinct, staging-only secret values.
- **Data:** smaller, sanitized, or synthetic datasets; not real Production data.
- **Scale:** fewer instances / smaller resources than Production.
- **Release metadata:** `RELEASE_CHANNEL` may indicate staging (e.g. `staging`).
- **Retention:** shorter log/backup retention than Production.

Everything security-relevant — HTTPS/HSTS, CORS structure, OpenAPI disabled, migration
policy, seed policy, OTP policy, request limits — must **match** Production.

## Related

- [reverse-proxy-contract.md](reverse-proxy-contract.md)
- [production-config-example.md](production-config-example.md)
- [../release/go-live-smoke-tests.md](../release/go-live-smoke-tests.md)
- [../release/go-live-checklist.md](../release/go-live-checklist.md)
- [production-checklist.md](production-checklist.md)
