"use client";

type AiLoadingStateProps = {
  label?: string;
};

/** Inline loading indicator for AI tasks — no fake progress percentages. */
export function AiLoadingState({ label = "در حال تولید پیشنهاد…" }: AiLoadingStateProps) {
  return (
    <div
      role="status"
      aria-live="polite"
      className="adm-subtle flex items-center gap-2 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-3 text-[12px]"
    >
      <span
        aria-hidden
        className="h-3.5 w-3.5 animate-spin rounded-full border-2 border-current border-t-transparent"
      />
      {label}
    </div>
  );
}
