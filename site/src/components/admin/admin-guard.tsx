"use client";

import { useEffect, type ReactNode } from "react";
import { usePathname, useRouter } from "next/navigation";
import { useAuth, evaluateRouteAccess } from "@/components/auth";
import { buildAdminLoginUrl, isAdminPath } from "@/lib/admin/routes";
import { AdminAccessDenied } from "@/components/admin/feedback/admin-access-denied";

/**
 * Dedicated Admin route guard.
 *
 * States: loading → centered skeleton (prevents protected-page flash);
 * unauthenticated/expired → redirect to login carrying a safe return URL;
 * forbidden → access-denied page; allowed → render the Admin shell.
 *
 * This is a UX gate only. Every Admin API call is still authorized by the
 * backend, so hiding the shell never grants or protects data on its own.
 */
export function AdminGuard({ children }: { children: ReactNode }) {
  const { status, user } = useAuth();
  const router = useRouter();
  const pathname = usePathname();

  const access = evaluateRouteAccess({
    status,
    role: user?.role,
    requireAdmin: true,
  });

  useEffect(() => {
    if (access !== "unauthenticated") return;
    const returnUrl = isAdminPath(pathname) ? pathname : undefined;
    router.replace(buildAdminLoginUrl(returnUrl ?? "/admin"));
  }, [access, pathname, router]);

  if (access === "allowed") {
    return <>{children}</>;
  }

  if (access === "forbidden") {
    return <AdminAccessDenied />;
  }

  // loading or (transient) unauthenticated while the redirect is in flight.
  return (
    <div className="adm-app flex min-h-dvh items-center justify-center">
      <div className="flex flex-col items-center gap-3" role="status" aria-live="polite">
        <div className="adm-skeleton h-10 w-10 rounded-full" />
        <p className="adm-muted text-[13px]">
          {access === "unauthenticated"
            ? "در حال انتقال به صفحه ورود..."
            : "در حال بررسی دسترسی..."}
        </p>
      </div>
    </div>
  );
}
