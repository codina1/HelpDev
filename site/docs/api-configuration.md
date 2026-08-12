# API Configuration

How the HelpDev frontend is configured to talk to the backend API. Part of **Sprint 26 —
Frontend Integration & Go-Live Readiness**.

## Base URL contract

The frontend targets the backend's **canonical versioned API**:

```bash
NEXT_PUBLIC_HELPDEV_API_BASE_URL=https://api.example.com/api/v1
```

- **Canonical `/api/v1` only.** New frontend code uses the versioned prefix.
- **No legacy `/api` default for new code.** Do not point new modules at the unversioned
  `/api` routes. Exceptions:
  - `/health/live` and `/health/ready` (unversioned health probes).
  - Explicit legacy-compatibility tests that intentionally exercise `/api` routes.
- **HTTPS required in Production.** The base URL must be `https://` in Production builds.
- **Validated at startup/build.** The value is checked so a missing, non-absolute, or
  non-HTTPS (in Production) URL fails fast rather than producing broken requests at runtime.

## Environment variable rules

- The variable is **public**. Anything prefixed `NEXT_PUBLIC_` is embedded in the client
  bundle and visible to end users.
- **No secrets in public env vars.** Never place API keys, tokens, or credentials in
  `NEXT_PUBLIC_*`. The base URL is a public endpoint, not a secret.
- The API base URL is the only backend endpoint configuration the frontend needs; auth is
  handled per-request via the JWT (see
  [authentication-security.md](authentication-security.md)).

## `.env.local` usage

For local development, set the variable in `site/.env.local` (git-ignored):

```bash
# site/.env.local
NEXT_PUBLIC_HELPDEV_API_BASE_URL=https://localhost:5001/api/v1
```

- `.env.local` overrides values from `.env` for local runs and is not committed.
- In Production/staging, inject the variable through the host's build/runtime environment,
  not a committed file.
- Because the value is baked into the client at build time, changing it requires a rebuild
  (or the platform's runtime-env mechanism for `NEXT_PUBLIC_*`, if used).

## Examples

| Environment | Value |
|-------------|-------|
| Production | `https://api.example.com/api/v1` |
| Staging | `https://api.staging.example.com/api/v1` |
| Local | `https://localhost:5001/api/v1` |

## Related

- [authentication-security.md](authentication-security.md)
- [error-handling.md](error-handling.md)
- [../../api/docs/release/frontend-backend-contract.md](../../api/docs/release/frontend-backend-contract.md)
- [../../api/docs/api/versioning.md](../../api/docs/api/versioning.md)
