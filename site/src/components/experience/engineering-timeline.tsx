import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";
import {
  structuralRoadmapStatuses,
  type RoadmapStepStatus,
} from "@/lib/public/display-meta";

export type TimelineNode = {
  label: string;
  status?: RoadmapStepStatus;
};

type EngineeringTimelineProps = {
  title: string;
  nodes: TimelineNode[];
  badge?: string;
  note?: string;
  level?: string | null;
  className?: string;
};

const STATUS_LABEL: Record<RoadmapStepStatus, string> = {
  completed: "تکمیل",
  current: "جاری",
  unlocked: "باز",
  locked: "قفل",
};

/**
 * Visual engineering path timeline — levels, lock states, completion indicators.
 */
export function EngineeringTimeline({
  title,
  nodes,
  badge = "مسیر",
  note,
  level,
  className = "",
}: EngineeringTimelineProps) {
  const defaults = structuralRoadmapStatuses(nodes.length);
  const resolved = nodes.map((node, index) => ({
    label: node.label,
    status: node.status ?? defaults[index] ?? ("locked" as RoadmapStepStatus),
  }));
  const completed = resolved.filter((n) => n.status === "completed").length;

  return (
    <GlassCard strong gradientBorder className={["p-5 sm:p-6", className].join(" ")}>
      <div className="mb-4 flex flex-wrap items-center gap-2">
        <PremiumBadge variant="success">{badge}</PremiumBadge>
        {level ? <PremiumBadge variant="outline">{level}</PremiumBadge> : null}
        <h3 className="text-base font-extrabold text-[color:var(--pub-fg)]">{title}</h3>
        <span className="text-[11px] font-semibold text-[color:var(--pub-muted)]">
          {completed.toLocaleString("fa-IR")}/{resolved.length.toLocaleString("fa-IR")} تکمیل ساختاری
        </span>
      </div>
      {note ? <p className="mb-5 text-[13px] leading-7 text-[color:var(--pub-muted)]">{note}</p> : null}
      <ol className="flex flex-col items-start gap-1" aria-label={title}>
        {resolved.map((node, index) => {
          const locked = node.status === "locked";
          return (
            <li key={`${node.label}-${index}`} className="flex flex-col items-center gap-1">
              <span
                className={[
                  "exp-card-lift flex min-w-[11rem] items-center justify-between gap-3 rounded-xl border px-4 py-2.5 text-[13px] font-bold",
                  node.status === "completed"
                    ? "border-[color:color-mix(in_srgb,var(--ds-success)_50%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-success)_14%,transparent)] text-[color:var(--pub-fg)]"
                    : node.status === "current"
                      ? "border-[color:color-mix(in_srgb,var(--pub-primary)_50%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_16%,transparent)] text-[color:var(--pub-fg)] shadow-[0_0_18px_var(--pub-glow)]"
                      : locked
                        ? "border-[color:var(--pub-glass-border)] bg-white/[0.02] text-[color:var(--pub-muted)] opacity-75"
                        : "border-[color:color-mix(in_srgb,var(--pub-secondary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-secondary)_12%,transparent)] text-[color:var(--pub-fg)]",
                ].join(" ")}
              >
                <span className="flex items-center gap-2">
                  <span aria-hidden>
                    {node.status === "completed" ? "✓" : locked ? "—" : index + 1}
                  </span>
                  {node.label}
                </span>
                <span className="text-[10px] font-semibold text-[color:var(--pub-muted)]">
                  {STATUS_LABEL[node.status]}
                </span>
              </span>
              {index < resolved.length - 1 ? (
                <span
                  className={
                    resolved[index + 1]?.status === "locked"
                      ? "text-[color:var(--pub-muted)] opacity-50"
                      : "text-[color:var(--pub-secondary)]"
                  }
                  aria-hidden
                >
                  ↓
                </span>
              ) : null}
            </li>
          );
        })}
      </ol>
    </GlassCard>
  );
}
