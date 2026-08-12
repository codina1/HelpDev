# Release Candidate v1 Checklist

Sign-off checklist and evidence record for the HelpDev backend **v1** release candidate.
Part of **Sprint 25 — Deployment Hardening**.

Fill in each evidence placeholder before sign-off. An RC is not approved until every item
is checked and its evidence recorded.

## Build and source identity

- [ ] Commit / tag recorded — evidence: `______________________`
- [ ] Build version recorded — evidence: `______________________`
- [ ] Build completed with **0 warnings / 0 errors** — evidence: `______________________`

## Tests

- [ ] Full test suite green; total tests recorded — evidence:
      `____ tests, ____ passed, 0 failed`
- [ ] PostgreSQL integration tests passed — evidence: `______________________`
- [ ] Security regression suite passed — evidence: `______________________`
- [ ] Smoke test passed (OTP flow, authenticated read, admin read) — evidence:
      `______________________`

## API artifacts

- [ ] OpenAPI artifacts exported and reviewed — evidence:
      `api/artifacts/openapi/helpdev-{public,authenticated,admin,all}-v1.json`
- [ ] No unintended public surface changes vs prior v1 contract.

## Database / migrations

- [ ] Migration state confirmed: **no migration required** for this release — evidence:
      `______________________`
- [ ] `Database:MigrationMode=Validate` passes against the target schema.
- [ ] Migration history matches the deployed build.

## Configuration validation

- [ ] Centralized production safety validation result recorded (no
      `ProductionSafetyValidationFailed`) — evidence: `______________________`
- [ ] Configuration reviewed against
      [../deployment/production-checklist.md](../deployment/production-checklist.md).

## Operational readiness

- [ ] Rollback plan reviewed and attached — evidence:
      [../deployment/rollback-runbook.md](../deployment/rollback-runbook.md)
- [ ] Backup taken and restore procedure validated — evidence:
      [../deployment/restore-runbook.md](../deployment/restore-runbook.md)
- [ ] Known limitations reviewed and accepted — evidence:
      [known-limitations-v1.md](known-limitations-v1.md)

## Evidence summary table

| Item | Required evidence | Value / link |
|------|-------------------|--------------|
| Commit / tag | Hash or tag | `__________` |
| Build version | Version string | `__________` |
| Build cleanliness | 0 warnings / 0 errors | `__________` |
| Tests | Total tests + result | `__________` |
| OpenAPI artifacts | Exported spec files | `__________` |
| Migration state | No migration required | `__________` |
| Configuration validation | Validation result | `__________` |
| Smoke test | Result | `__________` |
| Security regression | Result | `__________` |
| PostgreSQL integration | Result | `__________` |
| Rollback plan | Link | `__________` |
| Known limitations | Reviewed / accepted | `__________` |

## Sign-off

- [ ] Engineering lead: `__________`  Date: `__________`
- [ ] Operations / on-call: `__________`  Date: `__________`

## Related

- [../deployment/release-runbook.md](../deployment/release-runbook.md)
- [../deployment/production-checklist.md](../deployment/production-checklist.md)
- [known-limitations-v1.md](known-limitations-v1.md)
