# Configuration Reference

Complete catalog of HelpDev backend configuration settings, bound via
`Microsoft.Extensions.Configuration`. Part of **Sprint 25 — Deployment Hardening**.

Each setting lists its type, default, Production requirement, allowed range,
secret classification, and restart requirement.

> **All settings require a process restart to take effect.** The application binds and
> validates configuration once at startup (`ValidateOnStart`) and does not hot-reload
> operational settings. The "Restart required" column is therefore **Yes** for every row
> and is omitted from the tables for brevity.

## Configuration source priority

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. Environment variables
4. Command-line arguments
5. External secret provider (only if configured by the host)

For the environment-variable form of each key, see
[environment-variables.md](environment-variables.md).

## Database

Section: `Database`, `Database:Postgres`, and `ConnectionStrings`.

| Key | Type | Default | Production requirement | Allowed range | Secret |
|-----|------|---------|------------------------|---------------|--------|
| `ConnectionStrings:DefaultConnection` | string | — | **Required** (no default in Production) | Valid Npgsql string | **Yes** |
| `Database:MigrationMode` | enum | `Validate` (Prod) / `Apply` (Dev, Test) | Recommend `Validate` | `None`, `Validate`, `Apply` | No |
| `Database:MigrationLockTimeoutSeconds` | int | `60` | Optional | `1`–`600` | No |
| `Database:SeedMode` | enum | `None` (Prod) / `DevelopmentDemo` (Dev) | Must be `None` or `RequiredSystemData`; `DevelopmentDemo` **forbidden** | `None`, `RequiredSystemData`, `DevelopmentDemo` | No |
| `Database:Postgres:CommandTimeoutSeconds` | int | `30` | Optional | `1`–`600` | No |
| `Database:Postgres:ConnectionTimeoutSeconds` | int | `15` | Optional | `1`–`300` | No |
| `Database:Postgres:MinPoolSize` | int | `0` | Optional | `>= 0` | No |
| `Database:Postgres:MaxPoolSize` | int | `50` | Optional | `1`–`500` | No |
| `Database:Postgres:KeepAliveSeconds` | int | `30` | Optional | `0`–`600` | No |
| `Database:Postgres:EnableRetryOnFailure` | bool | `false` | Keep `false` unless retry-aware | `true`/`false` | No |
| `Database:Postgres:MaxRetryCount` | int | `5` | Optional | `0`–`20` | No |
| `Database:Postgres:MaxRetryDelaySeconds` | int | `10` | Optional | `1`–`120` | No |

**Notes**

- `MigrationMode=Validate` **fails startup** if pending migrations exist.
- `MigrationMode=Apply` acquires a PostgreSQL advisory lock (key `4207770001`) bounded by
  `MigrationLockTimeoutSeconds`.
- `EnableRetryOnFailure` is **off by default** because the application uses explicit,
  service-owned transactions. Enabling it requires execution-strategy-aware transaction
  handling.
- Recommended `MaxPoolSize`: **50–100**, depending on the PostgreSQL `max_connections`
  allocation for this application.

## JWT

Section: `Jwt`.

| Key | Type | Default | Production requirement | Allowed range | Secret |
|-----|------|---------|------------------------|---------------|--------|
| `Jwt:Secret` | string | — | **Required** | ≥ 32 chars, not a placeholder, ≠ `Security:PartitionHashKey` | **Yes** |
| `Jwt:Issuer` | string | `HelpDev` | Optional | Non-empty | No |
| `Jwt:Audience` | string | `HelpDev.Client` | Optional | Non-empty | No |
| `Jwt:ExpirationMinutes` | int | `60` | Optional | `1`–`1440` | No |

**Placeholder blocklist** (rejected for `Jwt:Secret`): `changeme`, `secret`, `password`,
`test`, `dev-secret`, `your-secret-here`, `replace-me`, `change_in_production`,
`dev_secret`.

## OTP

OTP behavior is governed by the JWT auth flow and dedicated **rate-limiting policies**
(`OtpRequest`, `OtpVerify`) rather than a standalone OTP configuration section. Tuning is
performed through the `RateLimiting` section (see below and
[../api/rate-limits.md](../api/rate-limits.md)). Token lifetime issued after OTP
verification is controlled by `Jwt:ExpirationMinutes`.

## Security

Section: `Security`.

| Key | Type | Default | Production requirement | Allowed range | Secret |
|-----|------|---------|------------------------|---------------|--------|
| `Security:PartitionHashKey` | string | — | **Required** | ≥ 32 chars, not a placeholder, ≠ `Jwt:Secret` | **Yes** |
| `Security:RequireHttpsMetadata` | bool | — | **Must be `true`** | `true`/`false` | No |
| `Security:AllowedCorsOrigins` | string[] | — | See CORS section | Absolute URIs | No |
| `Security:DefaultRequestBodyLimitBytes` | int | (policy default) | Positive | `> 0` | No |
| `Security:MaxJsonRequestBodyLimitBytes` | int | (policy default) | Positive | `> 0` | No |

## CORS

Section: `Security:AllowedCorsOrigins` (fallback `Cors:FrontendOrigins`). Environment
array form: `Cors__AllowedOrigins__0` / `Security__AllowedCorsOrigins__0`.

| Rule | Requirement |
|------|-------------|
| Wildcard `*` | **Not allowed** |
| URI form | Absolute `http`/`https`, **no path** |
| Scheme in Production | **HTTPS required** (`localhost` only for controlled envs) |
| Secret | No |

## Reverse proxy

Section: `ReverseProxy`.

| Key | Type | Default | Production requirement | Allowed range | Secret |
|-----|------|---------|------------------------|---------------|--------|
| `ReverseProxy:Enabled` | bool | `false` | Optional | `true`/`false` | No |
| `ReverseProxy:TrustedProxyAddresses` | string[] | — | Required if `Enabled` in Prod (address **or** network) | Valid IPs | No |
| `ReverseProxy:TrustedProxyNetworks` | string[] | — | Required if `Enabled` in Prod (address **or** network) | Valid CIDR | No |
| `ReverseProxy:ForwardLimit` | int | `1` | Optional | `1`–`8` | No |
| `ReverseProxy:RequireForwardedProto` | bool | `true` | Optional | `true`/`false` | No |
| `ReverseProxy:RequireKnownProxyConfiguration` | bool | `true` | Optional | `true`/`false` | No |

**Note:** When `ReverseProxy:Enabled=true` in Production, **at least one** trusted proxy
address or trusted proxy network is required. Legacy key
`Security:TrustedProxyAddresses` is still honored; env form
`ForwardedHeaders__TrustedProxyAddresses__0`.

### HTTPS / HSTS

Section: `Https`. HSTS and HTTPS redirection apply when the environment is **not
Development**.

| Key | Type | Default | Allowed range | Secret |
|-----|------|---------|---------------|--------|
| `Https:RedirectToHttps` | bool | `true` | `true`/`false` | No |
| `Https:EnableHsts` | bool | `true` | `true`/`false` | No |
| `Https:HstsMaxAgeDays` | int | `365` | `0`–`730` | No |
| `Https:HstsIncludeSubDomains` | bool | `true` | `true`/`false` | No |
| `Https:HstsPreload` | bool | `false` | `true`/`false` | No |

## Rate limiting

Section: `RateLimiting`. Sliding-window policies partitioned by authenticated vs
anonymous traffic. Policy windows and permit limits are tunable per environment without a
version bump. Full policy catalog: [../api/rate-limits.md](../api/rate-limits.md).

| Property | Notes |
|----------|-------|
| Policies | `GeneralApi`, `OtpRequest`, `OtpVerify`, `Search`, `ToolboxExecution`, `PromptRender`, `PublicContentRead`, `AdminMutation`, `Authentication` |
| Scope | **Instance-local** (not cluster-wide) — see [../release/known-limitations-v1.md](../release/known-limitations-v1.md) |
| Partition keying | Uses `Security:PartitionHashKey` |
| Secret | No (policy values); key material is secret |

## Request sizes

Section: `Security` (limits) — see [../api/README.md](../api/README.md) for per-area caps.

| Key | Type | Default | Requirement | Secret |
|-----|------|---------|-------------|--------|
| `Security:DefaultRequestBodyLimitBytes` | int | policy default | Positive | No |
| `Security:MaxJsonRequestBodyLimitBytes` | int | policy default | Positive | No |

Oversized bodies return **413** with `code: security_request_too_large`.

## Observability

Section: `Observability`. Controls health cache duration and subsystem thresholds used by
admin health diagnostics.

| Property | Notes |
|----------|-------|
| Health cache | Cache duration in seconds; **instance-local** cache |
| Thresholds | Subsystem status thresholds for detailed admin health |
| External exporter | **None bundled** (no OTLP/vendor exporter) |
| Secret | No |

## OpenAPI

Section: `OpenApi`. Disabled in Production by default.

| Key | Type | Default | Production requirement | Secret |
|-----|------|---------|------------------------|--------|
| `OpenApi:Enabled` | bool | (env-dependent) | Optional | No |
| `OpenApi:EnableUi` | bool | (env-dependent) | Optional | No |
| `OpenApi:EnableInProduction` | bool | `false` | Keep `false` unless intentional | No |
| `OpenApi:ExposeAdminDocumentInProduction` | bool | `false` | Keep `false` | No |
| `OpenApi:EnableTryItOutInProduction` | bool | `false` | Keep `false` | No |

## Outbox

Section: `Outbox`. Transactional outbox background processor.

| Key | Type | Default | Production requirement | Secret |
|-----|------|---------|------------------------|--------|
| `Outbox:Enabled` | bool | `true` | Recommend `true` | No |
| `Outbox:BatchSize` | int | `20` | Optional | No |
| `Outbox:PollIntervalSeconds` | int | `5` | Optional | No |
| `Outbox:LockDurationSeconds` | int | `30` | Optional | No |
| `Outbox:MaxAttempts` | int | `10` | Optional | No |

**Note:** The Outbox heartbeat/lock is **instance-local**; there is no distributed lock.

## Logging

Section: `Logging`.

| Key | Type | Default | Production requirement | Secret |
|-----|------|---------|------------------------|--------|
| `Logging:EnableSensitiveDataLogging` | bool | `false` | **Must be `false`** | No |
| `Logging:EnableDetailedErrors` | bool | `false` | **Must be `false`** | No |
| `Logging:LogLevel:Default` | string | `Information` | **Must not be `Debug`/`Trace`** | No |
| `Logging:LogLevel:Microsoft.AspNetCore` | string | (framework default) | Optional | No |

## Release metadata

Section: `Release`. Optional, Admin-only metadata exposed at
`/api/v1/admin/operations/version`. Falls back to assembly metadata when unset.

| Key | Type | Constraint | Env var | Secret |
|-----|------|------------|---------|--------|
| `Release:Version` | string | ≤ 64 chars, safe chars, no line breaks | `RELEASE_VERSION` | No |
| `Release:Commit` | string | ≤ 64 chars, safe chars, no line breaks | `RELEASE_COMMIT` | No |
| `Release:BuildTimestamp` | string | UTC ISO 8601 | `RELEASE_BUILD_TIMESTAMP` | No |
| `Release:Channel` | string | ≤ 32 chars, safe chars, no line breaks | `RELEASE_CHANNEL` | No |

## Shutdown

Section: `Shutdown`.

| Key | Type | Default | Allowed range | Secret |
|-----|------|---------|---------------|--------|
| `Shutdown:TimeoutSeconds` | int | `30` | `1`–`300` | No |

Maps to `HostOptions.ShutdownTimeout`. On exceeding the timeout during shutdown, the
`HostedServiceShutdownTimedOut` event is logged.

## Related

- [environment-variables.md](environment-variables.md)
- [production-checklist.md](production-checklist.md)
- [health-probes.md](health-probes.md)
