type PageLoadingStateProps = {
  label?: string;
  rows?: number;
  className?: string;
};

/** Lightweight loading skeleton for product pages. Token-driven. */
export function PageLoadingState({
  label = "در حال بارگذاری...",
  rows = 4,
  className = "",
}: PageLoadingStateProps) {
  return (
    <div
      dir="rtl"
      className={`space-y-3 ${className}`.trim()}
      role="status"
      aria-live="polite"
      aria-busy="true"
    >
      <span className="sr-only">{label}</span>
      <div className="h-8 w-48 animate-pulse rounded-[var(--ds-radius-md)] bg-[color:color-mix(in_srgb,var(--ds-primary)_18%,transparent)]" />
      {Array.from({ length: rows }).map((_, index) => (
        <div
          key={index}
          className="h-16 w-full animate-pulse rounded-[var(--ds-radius-lg)] bg-[color:color-mix(in_srgb,var(--ds-fg)_6%,transparent)]"
        />
      ))}
    </div>
  );
}
