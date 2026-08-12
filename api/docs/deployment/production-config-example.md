# Production Configuration Example

Annotated walkthrough of the committed environment template
`deploy/env/helpdev.production.env.example`. Part of **Sprint 26 — Production Deployment
Validation & Go-Live Readiness**.

The template contains **placeholders only**. Copy it to a secure location outside the
repository, replace every `<placeholder>` with a real value injected by your host's secret
mechanism, and validate before starting the server. Never commit real credentials.

For the full settings catalog see [configuration-reference.md](configuration-reference.md)
and [environment-variables.md](environment-variables.md).

## Safety rules (enforced at startup)

- `Jwt__Secret` and `Security__PartitionHashKey` **must be different** values, each **≥ 32
  characters**, and must not be on the placeholder blocklist.
- CORS origins **must be absolute HTTPS URIs with no path** in Production; no wildcards.
- **No deterministic/exposed OTP** in Production (`Auth:ExposeOtpInResponse=false`); never
  enable a deterministic OTP provider.
- `Database__MigrationMode=Validate` for normal startup; use the controlled
  `--apply-migrations` command to change schema.
- `Database__SeedMode=None`; `DevelopmentDemo` is forbidden in Production.
- `Security__RequireHttpsMetadata=true`; HTTPS redirection and HSTS enabled.

## Variable-by-variable

### Hosting

- **`ASPNETCORE_ENVIRONMENT=Production`** — selects Production hardening rules and
  `appsettings.Production.json`. Must be `Production` (or a Staging value that applies the
  same safety rules) for go-live.
- **`ASPNETCORE_URLS=http://0.0.0.0:8080`** — internal HTTP bind. The reverse proxy
  terminates TLS and forwards here. This port is not exposed publicly.

### Database

- **`ConnectionStrings__DefaultConnection=<set-securely>`** — PostgreSQL (Npgsql)
  connection string. **Secret.** Required in Production; readiness depends on it. Never
  logged. Prefer `SSL Mode=Require`.

### Authentication (JWT)

- **`Jwt__Secret=<minimum-32-character-secret>`** — signing key for issued JWTs. **Secret,
  ≥ 32 chars, distinct from the partition key.** Rejected if a known placeholder.
- **`Jwt__Issuer=<issuer>`** — `iss` claim (default `HelpDev`).
- **`Jwt__Audience=<audience>`** — `aud` claim (default `HelpDev.Client`).

### Security

- **`Security__PartitionHashKey=<different-minimum-32-character-secret>`** — HMAC key that
  partitions rate-limit buckets. **Secret, ≥ 32 chars, MUST differ from `Jwt__Secret`.**
  Reusing the JWT secret here fails validation.
- **`Security__RequireHttpsMetadata=true`** — rejects tokens without HTTPS metadata. Must
  stay `true` in Production.

### CORS

- **`Cors__AllowedOrigins__0=https://frontend.example.com`** — exact browser origin of the
  frontend. HTTPS only in Production, no path, no wildcard. Add `__1`, `__2`, … for more
  origins. Must match the deployed frontend domain.

### Reverse proxy / forwarded headers

- **`ForwardedHeaders__Enabled=true`** — enable only when running behind a trusted proxy
  that terminates TLS.
- **`ForwardedHeaders__TrustedProxyAddresses__0=<proxy-ip>`** — the only address permitted
  to set `X-Forwarded-*`. Required when the proxy is enabled in Production.
- **`ForwardedHeaders__ForwardLimit=1`** — number of proxies between client and app
  (usually 1). See [reverse-proxy-contract.md](reverse-proxy-contract.md).

### HTTPS / HSTS

- **`Https__EnableRedirection=true`** — redirect HTTP to HTTPS outside Development.
- **`Https__EnableHsts=true`** — send HSTS. Tune max-age/subdomains/preload via the `Https`
  section if needed.

### Database startup policy

- **`Database__MigrationMode=Validate`** — normal startup validates schema and **fails if
  pending migrations exist**. Do not use `Apply` for routine startup.
- **`Database__SeedMode=None`** — no demo seeding in Production. `DevelopmentDemo` is
  forbidden and fails startup.

### Outbox

- **`Outbox__Enabled=true`** — enables the transactional outbox processor (drives eventual
  consistency for Search). Recommended `true`.

### OpenAPI

- **`OpenApi__EnableInProduction=false`** — Swagger/OpenAPI disabled in Production.
- **`OpenApi__ExposeAdminDocumentInProduction=false`** — keep the admin document hidden.
- **`OpenApi__EnableTryItOutInProduction=false`** — keep "Try It Out" disabled.

### Release metadata (Admin-only, sanitized)

- **`RELEASE_VERSION=<version>`**, **`RELEASE_COMMIT=<commit>`**,
  **`RELEASE_BUILD_TIMESTAMP=<utc-timestamp>`**, **`RELEASE_CHANNEL=production`** — bounded,
  safe-character values surfaced at `/api/v1/admin/operations/version` and in the release
  manifest. Not secret; fall back to assembly metadata when unset.

## Validate before starting the server

Run the offline validator against the target configuration. It loads Production config,
runs options validation and the `ProductionSafetyValidator`, and **does not start the HTTP
server or hosted services**. It prints only safe messages (no secrets, connection strings,
or SQL).

```bash
# Exit 0 = valid, non-zero (1) = invalid
dotnet HelpDev.API.dll --validate-production-config
```

Add a non-mutating database check (verifies the provider is PostgreSQL and reports
applied/pending migration counts):

```bash
dotnet HelpDev.API.dll --validate-production-config --validate-database
```

A pending-migration count greater than zero means normal `Validate` startup will fail;
apply migrations through the controlled command first (see
[publish-artifact.md](publish-artifact.md) and [release-runbook.md](release-runbook.md)).

## Related

- [environment-variables.md](environment-variables.md)
- [configuration-reference.md](configuration-reference.md)
- [reverse-proxy-contract.md](reverse-proxy-contract.md)
- [production-checklist.md](production-checklist.md)
- [../release/go-live-checklist.md](../release/go-live-checklist.md)
