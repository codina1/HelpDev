import { ApiClientError } from "@/lib/api/errors";
import { Button } from "@/components/ui/ds/button";

type ErrorStateProps = {
  error?: unknown;
  title?: string;
  onRetry?: () => void;
  className?: string;
};

export function ErrorState({
  error,
  title = "خطایی رخ داد",
  onRetry,
  className = "",
}: ErrorStateProps) {
  const message = toSafeMessage(error);
  const correlationId = error instanceof ApiClientError ? error.correlationId : null;

  return (
    <div
      dir="rtl"
      role="alert"
      className={[
        "flex flex-col items-center gap-4 rounded-[var(--ds-radius-xl)] border border-[color:color-mix(in_srgb,var(--ds-danger)_30%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-danger)_8%,transparent)] px-6 py-10 text-center",
        className,
      ].join(" ")}
    >
      <div className="space-y-1">
        <h2 className="text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h2>
        <p className="mx-auto max-w-md text-[13px] leading-6 text-[color:var(--ds-muted)]">{message}</p>
      </div>
      {correlationId ? (
        <p className="text-[11px] text-[color:var(--ds-muted)]">
          کد پیگیری:{" "}
          <span dir="ltr" className="font-mono text-[color:var(--ds-fg)]/70">
            {correlationId}
          </span>
        </p>
      ) : null}
      {onRetry ? (
        <Button variant="secondary" size="sm" onClick={onRetry}>
          تلاش مجدد
        </Button>
      ) : null}
    </div>
  );
}

function toSafeMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    if (error.isNetworkError) return "اتصال به سرور برقرار نشد. اتصال اینترنت را بررسی کنید.";
    if (error.isForbidden) return "برای انجام این عملیات دسترسی کافی ندارید.";
    if (error.isUnauthorized) return "برای ادامه وارد حساب کاربری شوید.";
    if (error.isServerError) return "خطای داخلی سرور رخ داد. لطفاً بعداً تلاش کنید.";
    return error.message || "درخواست ناموفق بود.";
  }
  return "مشکلی پیش آمد. لطفاً دوباره تلاش کنید.";
}
