type ProgressCardProps = {
  title: string;
  progressPercentage: number;
  status?: string;
  description?: string;
};

/** Progress display with text label (not color-only). */
export function ProgressCard({
  title,
  progressPercentage,
  status,
  description,
}: ProgressCardProps) {
  const clamped = Math.max(0, Math.min(100, Math.round(progressPercentage)));

  return (
    <article
      dir="rtl"
      className="rounded-2xl border border-[color:var(--ds-border)] bg-[color:var(--ds-surface)] p-4"
      aria-label={`${title}: ${clamped} درصد`}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="text-[14px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
          {description ? <p className="mt-1 text-[12px] text-[color:var(--ds-muted)]">{description}</p> : null}
        </div>
        <div className="text-end">
          <p className="text-[18px] font-extrabold text-[color:var(--ds-success)]">{clamped}٪</p>
          {status ? <p className="text-[11px] text-[color:var(--ds-muted)]">{status}</p> : null}
        </div>
      </div>
      <div
        className="mt-3 h-2 overflow-hidden rounded-full bg-[color:color-mix(in_srgb,var(--ds-fg)_10%,transparent)]"
        role="progressbar"
        aria-valuenow={clamped}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`پیشرفت ${clamped} درصد`}
      >
        <div
          className="h-full rounded-full bg-[color:var(--ds-success)]"
          style={{ width: `${clamped}%` }}
        />
      </div>
    </article>
  );
}
