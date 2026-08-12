import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { ApiClientError } from "@/lib/api/errors";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminErrorStateProps = {
  error?: unknown;
  title?: string;
  onRetry?: () => void;
  showHome?: boolean;
};

/**
 * Safe Admin error surface. Never renders stack traces, raw backend payloads or
 * secrets. Shows a friendly Persian message plus the correlation id (when
 * available) so users can reference it in support.
 */
export function AdminErrorState({
  error,
  title = "خطایی رخ داد",
  onRetry,
  showHome = true,
}: AdminErrorStateProps) {
  const message = toSafeMessage(error);
  const correlationId =
    error instanceof ApiClientError ? error.correlationId : null;

  return (
    <div className="adm-surface flex flex-col items-center gap-4 rounded-xl p-8 text-center">
      <span className="flex h-12 w-12 items-center justify-center rounded-full bg-[var(--adm-danger-soft)] text-[var(--adm-danger)]">
        <AdminIcon name="health" size={24} />
      </span>
      <div className="space-y-1">
        <h2 className="adm-text text-[15px] font-bold">{title}</h2>
        <p className="adm-muted max-w-md text-[13px] leading-6">{message}</p>
      </div>

      {correlationId ? (
        <p className="adm-subtle text-[11px]">
          کد پیگیری:{" "}
          <span dir="ltr" className="font-mono">
            {correlationId}
          </span>
        </p>
      ) : null}

      <div className="flex flex-wrap items-center justify-center gap-2">
        {onRetry ? (
          <button type="button" onClick={onRetry} className="adm-btn adm-btn-primary adm-focus">
            تلاش مجدد
          </button>
        ) : null}
        {showHome ? (
          <Link href={ADMIN_ROUTES.dashboard} className="adm-btn adm-btn-outline adm-focus">
            بازگشت به داشبورد
          </Link>
        ) : null}
      </div>
    </div>
  );
}

function toSafeMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    if (error.isNetworkError) {
      return "اتصال به سرور برقرار نشد. اتصال اینترنت را بررسی کنید.";
    }
    if (error.isForbidden) {
      return "برای انجام این عملیات دسترسی کافی ندارید.";
    }
    if (error.isServerError) {
      return "خطای داخلی سرور رخ داد. لطفاً بعداً تلاش کنید.";
    }
    return error.message || "درخواست ناموفق بود.";
  }

  return "مشکلی پیش آمد. لطفاً دوباره تلاش کنید.";
}
