# Go-Live Checklist

Final gate before promoting a HelpDev release to Production. Part of **Sprint 26 —
Production Deployment Validation & Go-Live Readiness**. Work top to bottom; every item must
be checked or explicitly waived with a recorded reason.

## Artifact

- [ ] Release publish completed (`dotnet publish -c Release`, clean tree at target commit).
- [ ] Release manifest generated (`--emit-release-manifest`); `migrationCount = 11`.
- [ ] Hashes verified (`binarySha256` matches the computed SHA-256 of `HelpDev.API.dll`).
- [ ] Previous artifact retained for rollback.

## Configuration

- [ ] Production validator passes (`--validate-production-config`, exit `0`).
- [ ] Secrets injected via host mechanism (not committed).
- [ ] JWT and partition keys are distinct (`Jwt__Secret` ≠ `Security__PartitionHashKey`).
- [ ] CORS origin correct (absolute HTTPS, no path, no wildcard; matches frontend domain).
- [ ] Trusted proxy correct (`ForwardedHeaders__TrustedProxyAddresses`, `ForwardLimit=1`).
- [ ] Swagger disabled (`OpenApi__EnableInProduction=false`).

## Database

- [ ] Backup verified immediately before deploy.
- [ ] Migration reviewed (none required, or backward-compatible).
- [ ] Migration applied once via the controlled `--apply-migrations` step (if required).
- [ ] Normal startup set to `Validate` (`Database__MigrationMode=Validate`).
- [ ] Pending migration count is zero (`--validate-database`).

## Runtime

- [ ] Service account correct (least-privilege, non-login).
- [ ] Port bound (internal `ASPNETCORE_URLS` port; not publicly exposed).
- [ ] Reverse proxy healthy (contract satisfied; no redirect loop).
- [ ] HTTPS / HSTS correct at the edge.
- [ ] Liveness / readiness correct (`/health/live` and `/health/ready` return `200`).

## Application

- [ ] Outbox healthy (pending/failed draining, not growing).
- [ ] Search healthy (queries return; eventual consistency via Outbox).
- [ ] Analytics reachable (admin analytics read succeeds).
- [ ] Audit reachable (admin audit read succeeds).
- [ ] Version metadata correct (`GET /api/v1/admin/operations/version` matches manifest).

## Frontend

- [ ] Canonical API base URL configured (`.../api/v1`).
- [ ] Login works (OTP request → verify → JWT → profile).
- [ ] Public pages work (Content, Search, Learning, Toolbox, PromptLab).
- [ ] Protected routes work (authenticated views load with a valid token).
- [ ] Admin authorization works (admin-only views gated; non-admin gets 403).
- [ ] Errors / correlation work (error `code` handled; `X-Correlation-ID` captured).

## Rollback

- [ ] Previous artifact available.
- [ ] Rollback steps reviewed ([../deployment/rollback-runbook.md](../deployment/rollback-runbook.md)).
- [ ] Owner assigned for the go-live window.

## Related

- [go-live-smoke-tests.md](go-live-smoke-tests.md)
- [release-evidence-template.md](release-evidence-template.md)
- [../deployment/publish-artifact.md](../deployment/publish-artifact.md)
- [../deployment/production-checklist.md](../deployment/production-checklist.md)
- [../deployment/release-runbook.md](../deployment/release-runbook.md)
