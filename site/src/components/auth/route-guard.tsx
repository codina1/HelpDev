"use client";

import type { ReactNode } from "react";
import { useAuth, type AuthStatus } from "@/components/auth/auth-provider";
import type { UserRole } from "@/types/auth";

export type RouteAccess = "loading" | "unauthenticated" | "forbidden" | "allowed";

/**
 * Pure access decision. Client checks improve UX only — backend authorization
 * remains authoritative.
 *
 * - `loading` while auth state is unknown (prevents protected-page flash).
 * - `unauthenticated` when auth is required but the session is anonymous/expired
 *   (maps to a 401 experience: login / session expired).
 * - `forbidden` when Admin is required but the user is not an Admin
 *   (maps to a 403 experience: access denied).
 */
export function evaluateRouteAccess(params: {
  status: AuthStatus;
  role: UserRole | null | undefined;
  requireAuth?: boolean;
  requireAdmin?: boolean;
}): RouteAccess {
  const { status, role, requireAuth, requireAdmin } = params;
  const needsAuth = requireAuth || requireAdmin;

  if (needsAuth && status === "unknown") {
    return "loading";
  }

  if (needsAuth && status !== "authenticated") {
    return "unauthenticated";
  }

  if (requireAdmin && role !== "Admin") {
    return "forbidden";
  }

  return "allowed";
}

export type RouteGuardProps = {
  children: ReactNode;
  requireAuth?: boolean;
  requireAdmin?: boolean;
  loadingFallback?: ReactNode;
  unauthenticatedFallback?: ReactNode;
  forbiddenFallback?: ReactNode;
};

export function RouteGuard({
  children,
  requireAuth,
  requireAdmin,
  loadingFallback = <DefaultMessage text="در حال بررسی دسترسی..." />,
  unauthenticatedFallback = <DefaultMessage text="برای مشاهده این بخش وارد حساب کاربری شوید." />,
  forbiddenFallback = <DefaultMessage text="شما به این بخش دسترسی ندارید." />,
}: RouteGuardProps) {
  const { status, user } = useAuth();

  const access = evaluateRouteAccess({
    status,
    role: user?.role,
    requireAuth,
    requireAdmin,
  });

  switch (access) {
    case "loading":
      return <>{loadingFallback}</>;
    case "unauthenticated":
      return <>{unauthenticatedFallback}</>;
    case "forbidden":
      return <>{forbiddenFallback}</>;
    default:
      return <>{children}</>;
  }
}

function DefaultMessage({ text }: { text: string }) {
  return (
    <div className="flex min-h-dvh items-center justify-center bg-[#080a12] text-[13px] text-slate-400">
      {text}
    </div>
  );
}
