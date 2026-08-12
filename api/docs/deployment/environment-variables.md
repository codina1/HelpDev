# Environment Variables

Operational reference for configuring the HelpDev .NET 8 ASP.NET Core backend through
environment variables. Part of **Sprint 25 — Deployment Hardening**.

Environment variables map to configuration keys using the .NET double-underscore
convention: `Section__Key` binds to `Section:Key`, and `Section__Sub__0` binds to an
array element `Section:Sub:0`.

## Configuration source priority

Values are resolved in the following order (later sources override earlier ones):

1. `appsettings.json`
2. `appsettings.{Environment}.json`
3. **Environment variables**
4. Command-line arguments
5. External secret provider, only if configured by the host

> No cloud secret manager is bundled with the application. No secrets are committed to
> source control. `appsettings.Production.json` contains only non-secret policy defaults
> and placeholders.

## Secret handling

- Provide all secrets (`ConnectionStrings__DefaultConnection`, `Jwt__Secret`,
  `Security__PartitionHashKey`) through environment variables or a host-managed secret
  provider — never in committed files.
- All examples below use **placeholders**. Do not copy example values into any real
  environment.
- Every setting takes effect at process start only; **changing any variable requires a
  restart**.

## Core variables

### `ConnectionStrings__DefaultConnection`

- **Purpose:** PostgreSQL (Npgsql) connection string for the application database.
- **Required:** Yes — **required in Production** (no default in Production).
- **Secret:** Yes.
- **Validation:** Must be a valid Npgsql connection string reachable at startup;
  readiness depends on it.
- **Safe example (no real credentials):**

```bash
ConnectionStrings__DefaultConnection="Host=db.internal;Port=5432;Database=helpdev;Username=helpdev_app;Password=__SET_VIA_SECRET__;SSL Mode=Require;Trust Server Certificate=false"
```

### `Jwt__Secret`

- **Purpose:** Signing key for issued JWT access tokens.
- **Required:** Yes.
- **Secret:** Yes.
- **Validation:**
  - Minimum **32 characters**.
  - Must **not** be a known placeholder: `changeme`, `secret`, `password`, `test`,
    `dev-secret`, `your-secret-here`, `replace-me`, `change_in_production`, `dev_secret`.
  - Must **differ** from `Security__PartitionHashKey`.
- **Safe example:**

```bash
Jwt__Secret="__GENERATE_A_UNIQUE_32PLUS_CHAR_RANDOM_VALUE__"
```

### `Jwt__Issuer`

- **Purpose:** Token issuer (`iss`) claim.
- **Required:** No. Default `HelpDev`.
- **Secret:** No.
- **Safe example:**

```bash
Jwt__Issuer="HelpDev"
```

### `Jwt__Audience`

- **Purpose:** Token audience (`aud`) claim.
- **Required:** No. Default `HelpDev.Client`.
- **Secret:** No.
- **Safe example:**

```bash
Jwt__Audience="HelpDev.Client"
```

### `Security__PartitionHashKey`

- **Purpose:** Keying material for partition hashing (rate limiting / privacy salting).
- **Required:** Yes.
- **Secret:** Yes.
- **Validation:**
  - Minimum **32 characters**.
  - Must **not** be a known placeholder (same list as `Jwt__Secret`).
  - Must **differ** from `Jwt__Secret`.
- **Safe example:**

```bash
Security__PartitionHashKey="__GENERATE_A_SEPARATE_UNIQUE_32PLUS_CHAR_RANDOM_VALUE__"
```

### `Cors__AllowedOrigins__0`

- **Purpose:** Allowed CORS origin (indexed array; add `__1`, `__2`, … for more).
  Binds to `Security:AllowedCorsOrigins` (fallback `Cors:FrontendOrigins`).
- **Required:** Practically required for browser clients; optional otherwise.
- **Secret:** No.
- **Validation:**
  - No wildcard `*`.
  - Must be an absolute `http`/`https` URI **without a path**.
  - **HTTPS required in Production** (`localhost` permitted only for controlled
    environments).
- **Safe example:**

```bash
Cors__AllowedOrigins__0="https://app.example.com"
Cors__AllowedOrigins__1="https://admin.example.com"
```

### `ForwardedHeaders__TrustedProxyAddresses__0`

- **Purpose:** Trusted reverse-proxy address (indexed array). Binds to
  `ReverseProxy:TrustedProxyAddresses` (legacy `Security:TrustedProxyAddresses`).
- **Required:** Required **when `ReverseProxy__Enabled=true` in Production** — at least
  one trusted proxy address or trusted proxy network (CIDR) must be configured.
- **Secret:** No.
- **Validation:** Valid IP addresses; CIDR ranges go in `ReverseProxy:TrustedProxyNetworks`.
- **Safe example:**

```bash
ReverseProxy__Enabled="true"
ForwardedHeaders__TrustedProxyAddresses__0="10.0.0.10"
```

### `OpenApi__EnableInProduction`

- **Purpose:** Enables OpenAPI/Swagger document generation in Production.
- **Required:** No. Default `false` (OpenAPI disabled in Production).
- **Secret:** No.
- **Validation:** Boolean. Keep `false` unless intentionally exposing specs.
- **Safe example:**

```bash
OpenApi__EnableInProduction="false"
```

### `Database__MigrationMode`

- **Purpose:** Controls migration behavior at startup. Enum: `None | Validate | Apply`.
- **Required:** No. **Production default `Validate`**; Development/Testing default `Apply`.
- **Secret:** No.
- **Validation:**
  - `Validate` fails startup if pending migrations exist.
  - `Apply` acquires a PostgreSQL advisory lock (key `4207770001`) with a bounded
    timeout (`Database:MigrationLockTimeoutSeconds`, default 60s).
- **Safe example:**

```bash
Database__MigrationMode="Validate"
```

### `Database__SeedMode`

- **Purpose:** Controls data seeding at startup. Enum:
  `None | RequiredSystemData | DevelopmentDemo`.
- **Required:** No. **Production default `None`**; Development default `DevelopmentDemo`.
- **Secret:** No.
- **Validation:** `DevelopmentDemo` is **forbidden in Production** (fails startup).
- **Safe example:**

```bash
Database__SeedMode="None"
```

### `Outbox__Enabled`

- **Purpose:** Enables the transactional outbox background processor.
- **Required:** No. Default `true`.
- **Secret:** No.
- **Validation:** Boolean.
- **Safe example:**

```bash
Outbox__Enabled="true"
```

## Release metadata variables

Optional, Admin-only release metadata surfaced at
`/api/v1/admin/operations/version`. When unset, values fall back to assembly metadata.
Values are bounded and must use safe characters with no line breaks.

| Variable | Maps to | Constraint |
|----------|---------|------------|
| `RELEASE_VERSION` | `Release:Version` | ≤ 64 chars |
| `RELEASE_COMMIT` | `Release:Commit` | ≤ 64 chars |
| `RELEASE_BUILD_TIMESTAMP` | `Release:BuildTimestamp` | UTC ISO 8601 |
| `RELEASE_CHANNEL` | `Release:Channel` | ≤ 32 chars |

- **Required:** No.
- **Secret:** No.
- **Safe example:**

```bash
RELEASE_VERSION="1.0.0"
RELEASE_COMMIT="0000000000000000000000000000000000000000"
RELEASE_BUILD_TIMESTAMP="2026-07-21T05:00:00Z"
RELEASE_CHANNEL="stable"
```

## Minimal Production environment (illustrative)

```bash
ASPNETCORE_ENVIRONMENT="Production"

ConnectionStrings__DefaultConnection="__SET_VIA_SECRET__"
Jwt__Secret="__SET_VIA_SECRET__"
Security__PartitionHashKey="__SET_VIA_SECRET__"

Jwt__Issuer="HelpDev"
Jwt__Audience="HelpDev.Client"

Cors__AllowedOrigins__0="https://app.example.com"

Security__RequireHttpsMetadata="true"

Database__MigrationMode="Validate"
Database__SeedMode="None"
Outbox__Enabled="true"

OpenApi__EnableInProduction="false"
```

## Related

- [configuration-reference.md](configuration-reference.md) — complete settings catalog
- [production-checklist.md](production-checklist.md) — go-live gate
- [health-probes.md](health-probes.md) — probe endpoints
