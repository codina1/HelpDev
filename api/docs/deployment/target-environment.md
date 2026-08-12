# Target Environment

Template describing the hosting environment the HelpDev backend is deployed into. Part of
**Sprint 26 — Production Deployment Validation & Go-Live Readiness**.

This document is a **template**. Fill each placeholder with the real value for your
deployment and keep the completed copy with your release evidence (out of source control if
it contains sensitive network details). Use placeholders like `<set-securely>`,
`<proxy-ip>`, and `<version>` — never commit real secrets.

The application runs **Kestrel behind a reverse proxy**. The proxy terminates TLS and
forwards internal HTTP to the app. See
[reverse-proxy-contract.md](reverse-proxy-contract.md) for the exact contract and
[configuration-reference.md](configuration-reference.md) for settings.

## Environment matrix

| Item | Value (placeholder) | Notes |
|------|---------------------|-------|
| Target OS | `<os-and-version>` | Host OS for the API process (e.g. a Linux distribution or Windows Server release). |
| Web server / reverse proxy | `<reverse-proxy-and-version>` | Terminates TLS and forwards to Kestrel (e.g. Nginx, Apache, IIS, or a cloud L7 proxy). |
| Hosting process model | `<service-manager>` | Kestrel supervised as a service (systemd unit, Windows service, or container runtime). Auto-restart on failure. |
| Public API domain | `https://api.<your-domain>` | Public HTTPS hostname clients use. Preserved end-to-end; never exposes the internal host/port. |
| Frontend domain | `https://app.<your-domain>` | Origin of the Next.js frontend; must match the CORS allow-list (`Cors__AllowedOrigins__0`). |
| PostgreSQL host & version | `<db-host>:5432` / PostgreSQL `<version>` | Reachable from the app host only; not publicly exposed. Provider must be PostgreSQL. |
| Certificate termination | `<reverse-proxy>` (edge) | TLS terminates at the proxy; internal hop is HTTP on a private interface. |
| Trusted proxy address | `<proxy-ip>` | Set `ForwardedHeaders__TrustedProxyAddresses__0`; only this address may set `X-Forwarded-*`. |
| Application port | `8080` (internal) | Internal bind (`ASPNETCORE_URLS=http://0.0.0.0:8080`); not reachable from the public internet. |
| Deployment directory | `<deploy-dir>` | Directory containing the published artifact (e.g. `/opt/helpdev/api` or `C:\HelpDev\api`). |
| Service account | `<service-account>` | Least-privilege, non-login account owning the process; read access to config, no shell. |
| Env var injection mechanism | `<secret-mechanism>` | systemd `EnvironmentFile`, container secret store, or external secret provider — never committed. |
| Log destination | `<log-destination>` | Where structured stdout/stderr logs are collected (journald, file, or log shipper). No secrets are logged. |
| Restart mechanism | `<restart-command>` | How the service is restarted (e.g. `systemctl restart helpdev-api`); config changes require a restart. |
| Backup responsibility | `<backup-owner>` | Operator/host owns PostgreSQL backups; no automated backup is bundled. |
| Firewall restrictions | `<firewall-policy>` | Public: 443 to the proxy only. Internal: app port and 5432 restricted to the private network. |

## Notes

- **Process model.** Kestrel is not exposed directly to the internet. It binds the internal
  port and is fronted by the reverse proxy. The supervising service manager is responsible
  for start, stop, restart, and crash recovery.
- **TLS.** HTTPS terminates at the proxy. The app still requires HTTPS metadata
  (`Security__RequireHttpsMetadata=true`) and applies HSTS/redirection outside Development.
- **Trusted proxy.** Forwarded headers are honored **only** from the configured trusted
  proxy address with `ForwardLimit=1`. Misconfiguration here is a security risk — see
  [reverse-proxy-contract.md](reverse-proxy-contract.md).
- **Database reachability.** Readiness (`/health/ready`) depends on PostgreSQL. Keep the
  database on a private interface reachable only by the app host.
- **Secrets.** `ConnectionStrings__DefaultConnection`, `Jwt__Secret`, and
  `Security__PartitionHashKey` are injected by the host mechanism, never committed. See
  [environment-variables.md](environment-variables.md).
- **Restart semantics.** All operational settings bind once at startup (`ValidateOnStart`);
  there is no hot reload. Any configuration change requires a service restart.
- **Backups.** No automated backup ships with the application. Establish a backup and
  restore schedule before go-live — see [backup-and-restore.md](backup-and-restore.md).
- **Single-instance defaults.** Rate limiting, health cache, and the Outbox lock are
  instance-local; there is no distributed coordination. See
  [../release/known-limitations-v1.md](../release/known-limitations-v1.md).

## Related

- [production-config-example.md](production-config-example.md)
- [reverse-proxy-contract.md](reverse-proxy-contract.md)
- [environment-variables.md](environment-variables.md)
- [configuration-reference.md](configuration-reference.md)
- [production-checklist.md](production-checklist.md)
