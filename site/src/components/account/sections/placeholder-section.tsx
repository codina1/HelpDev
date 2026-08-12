import Link from "next/link";

type PlaceholderSectionProps = {
  title: string;
  description: string;
  actionHref?: string;
  actionLabel?: string;
};

export function PlaceholderSection({
  title,
  description,
  actionHref,
  actionLabel,
}: PlaceholderSectionProps) {
  return (
    <div className="dash-card p-6">
      <h2 className="text-[18px] font-bold text-white">{title}</h2>
      <p className="ui-body mt-3">{description}</p>

      {actionHref && actionLabel && (
        <Link
          href={actionHref}
          className="focus-ring mt-6 inline-flex rounded-xl border border-white/10 bg-white/[0.04] px-4 py-2.5 text-[13px] font-semibold text-slate-200 transition-colors hover:border-violet-500/30 hover:bg-violet-500/10"
        >
          {actionLabel}
        </Link>
      )}
    </div>
  );
}
