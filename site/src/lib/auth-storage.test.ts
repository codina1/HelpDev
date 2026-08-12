import { afterEach, beforeEach, describe, expect, it } from "vitest";
import {
  clearSession,
  getStoredSession,
  isStoredSessionExpired,
  storeSession,
} from "./auth-storage";
import type { AuthSession } from "@/types/auth";

const session: AuthSession = {
  accessToken: "jwt",
  expiresIn: 3600,
  user: {
    id: "1",
    mobile: "09120000000",
    role: "User",
    firstName: "",
    lastName: "",
    displayName: "user",
    email: "",
    profileImageUrl: "",
    expertise: "",
    interests: "",
  },
};

describe("auth-storage", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it("round-trips a stored session", () => {
    storeSession(session);
    expect(getStoredSession()?.accessToken).toBe("jwt");
  });

  it("clears a stored session", () => {
    storeSession(session);
    clearSession();
    expect(getStoredSession()).toBeNull();
  });

  it("reports non-expired for a fresh session", () => {
    storeSession(session);
    expect(isStoredSessionExpired()).toBe(false);
  });

  it("reports expired once past the expiry window", () => {
    storeSession(session);
    const past = Date.now() + session.expiresIn * 1000 + 1_000;
    expect(isStoredSessionExpired(past)).toBe(true);
  });

  it("is backward compatible with a bare AuthSession payload", () => {
    localStorage.setItem("helpdev.auth", JSON.stringify(session));
    expect(getStoredSession()?.accessToken).toBe("jwt");
  });
});
