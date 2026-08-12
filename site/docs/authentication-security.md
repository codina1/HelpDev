# Authentication Security

Token storage and session security model for the HelpDev frontend. Part of **Sprint 26 —
Frontend Integration & Go-Live Readiness**.

## Current model: localStorage

The frontend currently stores the issued JWT (and minimal session data) in the browser's
**`localStorage`**. This is a **documented limitation** for v1, chosen for simplicity and
because the backend issues short-lived access tokens with **no refresh tokens**.

- Storage is a single JSON session record keyed under a stable key.
- The token is attached as `Authorization: Bearer <token>` on authenticated requests.

## XSS considerations

`localStorage` is readable by any JavaScript running on the page, so a successful
cross-site scripting (XSS) attack could exfiltrate the token.

- Mitigation depends on strong XSS hygiene: escape/encode all rendered data, avoid
  `dangerouslySetInnerHTML`, keep dependencies patched, and apply a strict Content Security
  Policy at the edge.
- This is the primary reason the migration paths below are recommended for a future version.

## CSRF considerations

Because the token is sent explicitly in the `Authorization` header (not automatically
attached like a cookie), the app is **not** susceptible to classic cookie-based CSRF for API
calls.

- If a future version moves to cookie-based storage, CSRF defenses (SameSite cookies,
  anti-CSRF tokens) become necessary.

## Token expiry

- Access tokens have a bounded lifetime (`Jwt:ExpirationMinutes`, backend default 60).
- On expiry the backend returns `401` (`authentication_required`); the frontend clears the
  stored session and routes the user back to authentication.
- The frontend should treat any `401` on a previously authenticated request as an expired or
  invalid session.

## Logout

- Logout clears the stored session from `localStorage`.
- There is no server-side token revocation list; a token remains valid until it expires.
  Keeping token lifetimes short bounds this exposure.

## Multi-tab behavior

- Because storage is shared per origin, multiple tabs observe the same session.
- Logging out (clearing storage) in one tab should be reflected in others; listen for the
  `storage` event to synchronize session state across tabs.

## Refresh behavior

- **There are no refresh tokens.** The backend does not issue or accept refresh tokens.
- When the access token expires, the user re-authenticates via the OTP flow (mobile → OTP
  request → verify → new JWT). Do not implement silent refresh against non-existent
  endpoints.

## Current limitation (summary)

Storing the JWT in `localStorage` trades XSS resistance for implementation simplicity. It is
acceptable for v1 given short token lifetimes and no refresh tokens, but it is explicitly a
known limitation.

## Recommended future migration paths

These are **recommendations only** and are not implemented in v1:

1. **In-memory token + server-managed secure cookie.** Keep the access token in memory
   (lost on reload) and rely on a secure, `HttpOnly`, `SameSite` server-managed cookie to
   re-establish the session, reducing XSS token theft.
2. **HttpOnly cookie via a BFF (Backend-for-Frontend).** Introduce a thin BFF that holds the
   token server-side and exposes an `HttpOnly` session cookie to the browser, keeping tokens
   out of JavaScript entirely. This requires CSRF protections.

Either path requires backend/BFF support that does not exist today; adopting one is a future
decision, not a v1 change.

## Related

- [api-configuration.md](api-configuration.md)
- [error-handling.md](error-handling.md)
- [../../api/docs/api/authentication.md](../../api/docs/api/authentication.md)
- [../../api/docs/release/known-limitations-v1.md](../../api/docs/release/known-limitations-v1.md)
