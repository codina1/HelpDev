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
      className="rounded-2xl border border-white/10 bg-white/[0.03] p-4"
      aria-label={`${title}: ${clamped} درصد`}
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <h3 className="text-[14px] font-bold text-white">{title}</h3>
          {description ? <p className="mt-1 text-[12px] text-slate-400">{description}</p> : null}
        </div>
        <div className="text-end">
          <p className="text-[18px] font-extrabold text-emerald-300">{clamped}٪</p>
          {status ? <p className="text-[11px] text-slate-400">{status}</p> : null}
        </div>
      </div>
      <div
        className="mt-3 h-2 overflow-hidden rounded-full bg-white/10"
        role="progressbar"
        aria-valuenow={clamped}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={`پیشرفت ${clamped} درصد`}
      >
        <div
          className="h-full rounded-full bg-emerald-400/80"
          style={{ width: `${clamped}%` }}
        />
      </div>
    </article>
  );
}
