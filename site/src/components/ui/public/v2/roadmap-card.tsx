import Link from "next/link";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

export type RoadmapNode = {
  label: string;
};

export type RoadmapCardV2Props = {
  title: string;
  href: string;
  nodes?: RoadmapNode[];
  summary?: string | null;
  className?: string;
};

/**
 * Visual roadmap card with timeline nodes (not a flat content card).
 */
export function RoadmapCard({
  title,
  href,
  nodes = [],
  summary,
  className = "",
}: RoadmapCardV2Props) {
  const displayNodes = nodes.length > 0 ? nodes.slice(0, 5) : [{ label: "شروع" }, { label: "مهارت" }, { label: "پروژه" }];

  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--pub-radius)]", className].join(" ")}>
      <GlassCard className="h-full p-5" gradientBorder>
        <div className="mb-4 flex items-center justify-between gap-2">
          <PremiumBadge variant="success">نقشه راه</PremiumBadge>
          <span className="text-[11px] text-[color:var(--pub-muted)]">مسیر مهندسی</span>
        </div>
        <h3 className="text-base font-extrabold text-[color:var(--pub-fg)]">{title}</h3>
        {summary ? (
          <p className="mt-2 line-clamp-2 text-[13px] leading-6 text-[color:var(--pub-muted)]">{summary}</p>
        ) : null}

        <ol className="mt-5 space-y-0" aria-label={`مراحل ${title}`}>
          {displayNodes.map((node, index) => (
            <li key={`${node.label}-${index}`} className="relative flex gap-3 pb-4 last:pb-0">
              {index < displayNodes.length - 1 ? (
                <span
                  className="absolute start-[11px] top-6 h-[calc(100%-12px)] w-px bg-gradient-to-b from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)]/30"
                  aria-hidden
                />
              ) : null}
              <span
                className="relative z-[1] mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full border border-[color:color-mix(in_srgb,var(--pub-primary)_50%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_20%,transparent)] text-[10px] font-bold text-[color:var(--pub-ai-from)]"
                aria-hidden
              >
                {index + 1}
              </span>
              <span className="pt-0.5 text-[13px] font-semibold text-[color:var(--pub-fg)]/90">{node.label}</span>
            </li>
          ))}
        </ol>
      </GlassCard>
    </Link>
  );
}
