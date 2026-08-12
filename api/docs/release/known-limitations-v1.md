# Known Limitations — v1

Documented design choices and boundaries for the HelpDev backend **v1**. Part of
**Sprint 25 — Deployment Hardening**.

These are **deliberate design decisions** for a single-deployable modular monolith, not
defects. Each entry notes the behavior, its rationale, and the operational implication.

## Single-instance / non-distributed coordination

The following mechanisms are **instance-local** and are not coordinated across multiple
instances. When running more than one instance, expect per-instance behavior.

| Area | Behavior | Implication |
|------|----------|-------------|
| Rate limiting | Sliding-window counters are per instance | Effective limit scales with instance count; not a global cap |
| Health cache | Health results cached per instance (`Observability`) | Different instances may report slightly different cache windows |
| Outbox heartbeat / lock | Lock and heartbeat are per instance; no distributed lock | Outbox processing is designed to run safely per instance without cluster-wide coordination |

- **No distributed lock.** The application does not use a cross-instance lock service.
  (The migration `Apply` path uses a PostgreSQL advisory lock only for schema migration
  coordination — see [../deployment/configuration-reference.md](../deployment/configuration-reference.md).)
- **No cluster-wide coordination.** There is no leader election or shared coordination
  layer; instances operate independently.

## Observability

- **No external observability exporter** is bundled (no OTLP/vendor metrics/trace
  exporter). Observability is via structured logs and Admin operations endpoints
  ([../api/operations.md](../api/operations.md)).

## Backups

- **No automated backup** is included. Database backup, retention, and restore are the
  operator/hosting responsibility — see
  [../deployment/backup-and-restore.md](../deployment/backup-and-restore.md).

## Data access patterns

- **Offset pagination may shift** under concurrent inserts: an item can appear to move
  between pages if the underlying set changes during paging. This is expected for
  offset-based pagination; see [../api/pagination.md](../api/pagination.md).

## API compatibility

- **Legacy `/api/...` routes remain supported** for backward compatibility, mapping to the
  same v1 handlers as `/api/v1/...`. New integrations should prefer the versioned prefix
  and `/health/*` probes; there is no forced migration deadline. See
  [../api/versioning.md](../api/versioning.md).

## OpenAPI exposure

- **OpenAPI is disabled in Production by default** (`OpenApi:EnableInProduction=false`),
  along with admin-document exposure and "Try It Out". This is intentional to minimize
  production surface; specs are still generated and exported as build artifacts.

## Frontend integration (Sprint 26 observations)

- **Frontend token storage uses browser `localStorage`.** The Next.js frontend stores the
  JWT session in `localStorage`. This is readable by same-origin scripts, so XSS hygiene is
  critical. A future hardening step is to move to a secure server-managed / HttpOnly cookie
  via a backend-for-frontend. See [../../../site/docs/authentication-security.md](../../../site/docs/authentication-security.md).
- **No refresh tokens.** There is no refresh-token flow. When the access token expires the
  user re-authenticates via OTP; the frontend surfaces a `401` as a session-expired state.
- **Frontend canonical routes only.** New frontend code must call the canonical `/api/v1`
  base via the typed API client. An ESLint rule and a source-scan test prevent accidental
  use of unversioned `/api/...` URLs (health probes `/health/*` are exempt).
- **Correlation IDs are support metadata only.** The frontend generates/echoes
  `X-Correlation-ID` for support/debugging; it is never used for authentication and is not
  persisted long-term.

## Deployment (Sprint 26 observations)

- **Reverse proxy is assumed and not auto-configured.** Forwarded-header trust, TLS
  termination, and redirects assume a correctly configured trusted proxy; the proxy is not
  modified by the application. See [../deployment/reverse-proxy-contract.md](../deployment/reverse-proxy-contract.md).
- **Migrations are applied out-of-band.** Normal startup uses `Validate` in Production; the
  controlled `--apply-migrations` command applies schema changes under an advisory lock. Do
  not combine uncontrolled `Apply` with every multi-instance startup.
- **No automated deployment pipeline.** Publishing, hashing, config validation, migration,
  and smoke tests are performed via documented commands, not a CI/CD platform.
- **Staging differs from Production** only in isolation (separate database/credentials) and
  approved test data; it otherwise mirrors Production safety rules. See
  [../deployment/staging-environment.md](../deployment/staging-environment.md).

## Operating within these limitations

- Size rate limits and connection pools with instance count in mind.
- Treat health responses as instance-scoped signals for routing, not cluster state.
- Establish an external backup schedule before go-live.
- For clients, prefer keyset-friendly access patterns where ordering stability matters.

## Related

- [../deployment/production-checklist.md](../deployment/production-checklist.md)
- [../deployment/configuration-reference.md](../deployment/configuration-reference.md)
- [rc-v1-checklist.md](rc-v1-checklist.md)
- [../api/README.md](../api/README.md)
