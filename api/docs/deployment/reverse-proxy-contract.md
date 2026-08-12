# Reverse Proxy Contract

The exact contract between the edge reverse proxy and the HelpDev backend. Part of
**Sprint 26 — Production Deployment Validation & Go-Live Readiness**.

The application runs Kestrel on an internal HTTP port behind a proxy that terminates TLS.
This document defines what the proxy must do so the app behaves correctly and securely.

> The proxy is **not modified automatically** by the application or its deployment
> commands. The proxy configuration is an operator responsibility; this document is the
> contract it must satisfy.

## Topology

```
Client ──HTTPS(443)──▶ Reverse proxy ──HTTP(8080)──▶ Kestrel (HelpDev.API)
```

- **External:** HTTPS on the public API domain (`https://api.<your-domain>`).
- **Internal:** plain HTTP to the app's internal port (`ASPNETCORE_URLS=http://0.0.0.0:8080`).

## Contract

1. **TLS termination at the edge.** The proxy terminates HTTPS and forwards HTTP to the
   internal port. The internal port is never exposed publicly.
2. **Forwarded headers only from the trusted proxy.** The proxy sets `X-Forwarded-Proto`
   and `X-Forwarded-For`. The app honors them **only** from
   `ForwardedHeaders__TrustedProxyAddresses` with `ForwardedHeaders__ForwardLimit=1`.
   Requests from any other source must not be able to spoof these headers.
3. **`X-Forwarded-Proto: https`** must be sent for terminated HTTPS so the app recognizes
   the original scheme and applies HTTPS-only behavior correctly.
4. **HTTPS redirects work without a loop.** The app redirects HTTP→HTTPS outside
   Development. Because the proxy already terminates TLS and sends
   `X-Forwarded-Proto: https`, the app must see the request as secure and **not** produce a
   redirect loop.
5. **Public host preserved.** The proxy forwards the public `Host` (e.g.
   `api.<your-domain>`). Absolute URLs, redirects, and links must reflect the public host,
   never the internal host or port.
6. **Correlation ID preserved.** The `X-Correlation-ID` response header is passed back to
   the client unmodified. If the client sends one, it flows through; if not, the app
   generates one. See [../api/correlation-id.md](../api/correlation-id.md).
7. **Security headers preserved.** Headers emitted by the app (HSTS and other security
   headers) must reach the client unaltered; the proxy must not strip or overwrite them.
8. **413 responses preserved.** Request-size rejections (`413`,
   `code: security_request_too_large`) must pass through. If the proxy imposes its own body
   limit, align it with the app's limits so behavior is predictable.
9. **429 responses preserved.** Rate-limit responses (`429`,
   `code: security_rate_limit_exceeded`) and their `Retry-After` header must pass through
   unmodified.
10. **Internal host/port not exposed.** No response header, error page, or redirect may
    reveal the internal address or port. Proxy error pages must not leak upstream details.

## Required checks

- [ ] External HTTPS reaches the app; internal HTTP port is not publicly reachable.
- [ ] `X-Forwarded-Proto` / `X-Forwarded-For` are set by the proxy and accepted only from
      the trusted proxy address (`ForwardLimit=1`).
- [ ] `X-Forwarded-Proto: https` is sent for HTTPS requests.
- [ ] HTTP→HTTPS redirect works with **no redirect loop**.
- [ ] The public host is preserved in responses/redirects (no internal host/port leakage).
- [ ] `X-Correlation-ID` response header is preserved end-to-end.
- [ ] HSTS and security headers reach the client unaltered.
- [ ] `413` responses pass through; proxy body limit aligned with app limits.
- [ ] `429` responses and `Retry-After` pass through unmodified.
- [ ] Proxy error pages do not reveal upstream host/port.

## Configuration references

- Enable and trust the proxy: `ForwardedHeaders__Enabled=true`,
  `ForwardedHeaders__TrustedProxyAddresses__0=<proxy-ip>`,
  `ForwardedHeaders__ForwardLimit=1` (see
  [production-config-example.md](production-config-example.md)).
- `ReverseProxy:RequireForwardedProto` and `ReverseProxy:RequireKnownProxyConfiguration`
  default to `true` — see [configuration-reference.md](configuration-reference.md).

## Related

- [target-environment.md](target-environment.md)
- [production-config-example.md](production-config-example.md)
- [../api/correlation-id.md](../api/correlation-id.md)
- [../api/rate-limits.md](../api/rate-limits.md)
- [health-probes.md](health-probes.md)
