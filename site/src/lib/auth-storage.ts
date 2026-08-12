import type { AuthSession } from "@/types/auth";

/**
 * Token storage.
 *
 * Current model: the session (JWT + minimal profile) is kept in browser
 * localStorage. This is a documented limitation (see
 * `site/docs/authentication-security.md`): it is readable by same-origin
 * scripts, so XSS hygiene is critical. There are no refresh tokens; when the
 * access token expires the user re-authenticates via OTP.
 */

const STORAGE_KEY = "helpdev.auth";

type StoredAuth = {
  session: AuthSession;
  storedAt: number;
};

function readStored(): StoredAuth | null {
  if (typeof window === "undefined") return null;

  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) return null;

    const parsed = JSON.parse(raw) as Partial<StoredAuth> & Partial<AuthSession>;

    // Backward compatibility with the previous shape (a bare AuthSession).
    if (parsed && typeof parsed === "object" && "accessToken" in parsed && !("session" in parsed)) {
      return { session: parsed as AuthSession, storedAt: Date.now() };
    }

    if (parsed && (parsed as StoredAuth).session) {
      return parsed as StoredAuth;
    }

    return null;
  } catch {
    return null;
  }
}

export function getStoredSession(): AuthSession | null {
  return readStored()?.session ?? null;
}

/** Returns true when the stored access token has passed its expiry window. */
export function isStoredSessionExpired(now: number = Date.now()): boolean {
  const stored = readStored();
  if (!stored) return false;

  const expiresInMs = (stored.session.expiresIn ?? 0) * 1000;
  if (expiresInMs <= 0) return false;

  return now >= stored.storedAt + expiresInMs;
}

export function storeSession(session: AuthSession): void {
  if (typeof window === "undefined") return;
  const payload: StoredAuth = { session, storedAt: Date.now() };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(payload));
}

export function clearSession(): void {
  if (typeof window === "undefined") return;
  localStorage.removeItem(STORAGE_KEY);
}
