"use client";

type AiActionCardProps = {
  title: string;
  description: string;
  disabled?: boolean;
  busy?: boolean;
  onRun: () => void;
};

/** Single controlled AI action — click runs API; never auto-applies. */
export function AiActionCard({
  title,
  description,
  disabled = false,
  busy = false,
  onRun,
}: AiActionCardProps) {
  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 space-y-1">
          <h3 className="adm-text text-[13px] font-bold">{title}</h3>
          <p className="adm-subtle text-[11px] leading-5">{description}</p>
        </div>
        <button
          type="button"
          onClick={onRun}
          disabled={disabled || busy}
          className="adm-btn adm-btn-outline adm-focus shrink-0 px-2.5 py-1 text-[11px]"
        >
          {busy ? "در حال اجرا…" : "اجرا"}
        </button>
      </div>
    </div>
  );
}
