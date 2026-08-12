"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import {
  clearSession,
  getStoredSession,
  isStoredSessionExpired,
  storeSession,
} from "@/lib/auth-storage";
import { fetchMyProfile, updateMyProfile } from "@/lib/profile-api";
import { ApiClientError } from "@/lib/api/errors";
import type { AuthSession, AuthUser, UpdateProfileRequest } from "@/types/auth";

export type AuthStatus = "unknown" | "anonymous" | "authenticated" | "expired";

type AuthContextValue = {
  user: AuthUser | null;
  token: string | null;
  status: AuthStatus;
  isReady: boolean;
  login: (session: AuthSession) => Promise<void>;
  logout: () => void;
  refreshProfile: () => Promise<void>;
  saveProfile: (request: UpdateProfileRequest) => Promise<AuthUser>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [status, setStatus] = useState<AuthStatus>("unknown");

  // Guards a single in-flight profile load to avoid concurrent duplicate calls.
  const profileLoadingRef = useRef(false);

  const persistUser = useCallback(
    (accessToken: string, nextUser: AuthUser, expiresIn: number) => {
      storeSession({ accessToken, expiresIn, user: nextUser });
      setUser(nextUser);
      setToken(accessToken);
      setStatus("authenticated");
    },
    [],
  );

  const handleExpired = useCallback(() => {
    clearSession();
    setUser(null);
    setToken(null);
    setStatus("expired");
  }, []);

  const loadProfile = useCallback(
    async (accessToken: string, expiresIn: number, fallbackUser: AuthUser | null) => {
      if (profileLoadingRef.current) return;
      profileLoadingRef.current = true;

      try {
        const profile = await fetchMyProfile(accessToken);
        persistUser(accessToken, profile, expiresIn);
      } catch (error) {
        if (error instanceof ApiClientError && error.isUnauthorized) {
          handleExpired();
          return;
        }

        // Network or transient error: keep the last known user if we have one.
        if (fallbackUser) {
          persistUser(accessToken, fallbackUser, expiresIn);
        }
      } finally {
        profileLoadingRef.current = false;
      }
    },
    [persistUser, handleExpired],
  );

  useEffect(() => {
    const session = getStoredSession();
    if (!session) {
      setStatus("anonymous");
      return;
    }

    if (isStoredSessionExpired()) {
      handleExpired();
      return;
    }

    // Optimistically show the cached user, then refresh from the API.
    setToken(session.accessToken);
    setUser(session.user);
    setStatus("authenticated");
    void loadProfile(session.accessToken, session.expiresIn, session.user);
  }, [loadProfile, handleExpired]);

  const login = useCallback(
    async (session: AuthSession) => {
      persistUser(session.accessToken, session.user, session.expiresIn);
      await loadProfile(session.accessToken, session.expiresIn, session.user);
    },
    [persistUser, loadProfile],
  );

  const logout = useCallback(() => {
    clearSession();
    setUser(null);
    setToken(null);
    setStatus("anonymous");
  }, []);

  const refreshProfile = useCallback(async () => {
    if (!token) return;
    await loadProfile(token, user ? getExpiresIn() : getExpiresIn(), user);
  }, [token, user, loadProfile]);

  const saveProfile = useCallback(
    async (request: UpdateProfileRequest) => {
      if (!token) {
        throw new Error("ابتدا وارد حساب کاربری شوید.");
      }

      try {
        const profile = await updateMyProfile(token, request);
        persistUser(token, profile, getExpiresIn());
        return profile;
      } catch (error) {
        if (error instanceof ApiClientError && error.isUnauthorized) {
          handleExpired();
        }
        throw error;
      }
    },
    [token, persistUser, handleExpired],
  );

  const value = useMemo(
    () => ({
      user,
      token,
      status,
      isReady: status !== "unknown",
      login,
      logout,
      refreshProfile,
      saveProfile,
    }),
    [user, token, status, login, logout, refreshProfile, saveProfile],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

// Stored sessions carry expiresIn; reuse a stable default when refreshing an
// already-active session where the exact remaining lifetime is not tracked.
function getExpiresIn(): number {
  const session = getStoredSession();
  return session?.expiresIn ?? 3600;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider");
  }
  return context;
}
