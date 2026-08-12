import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";
import {
  structuralRoadmapStatuses,
  type RoadmapStepStatus,
} from "@/lib/public/display-meta";

export type RoadmapCardProNode = {
  label: string;
  status?: RoadmapStepStatus;
};

export type RoadmapCardProProps = {
  title: string;
  href: string;
  level?: string | null;
  nodes?: RoadmapCardProNode[];
  summary?: string | null;
  className?: string;
};

const STATUS_LABEL: Record<RoadmapStepStatus, string> = {
  completed: "تکمیل",
  current: "جاری",
  unlocked: "باز",
  locked: "قفل",
};

/**
 * Premium roadmap card — levels, structural progress, locked/unlocked steps, completion chrome.
 */
export function RoadmapCardPro({
  title,
  href,
  level,
  nodes = [],
  summary,
  className = "",
}: RoadmapCardProProps) {
  const steps =
    nodes.length > 0
      ? nodes.slice(0, 5)
      : [{ label: "شروع" }, { label: "مهارت" }, { label: "پروژه" }, { label: "تخصص" }];
  const defaults = structuralRoadmapStatuses(steps.length);
  const resolved = steps.map((node, index) => ({
    label: node.label,
    status: node.status ?? defaults[index] ?? ("locked" as RoadmapStepStatus),
  }));
  const stepCount = resolved.length;
  const completedCount = resolved.filter((s) => s.status === "completed").length;

  return (
    <Link
      href={href}
      className={["focus-ring group block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}
    >
      <Card variant="glass" className="flex h-full flex-col gap-4">
        <div className="flex flex-wrap items-center gap-2">
          <Badge variant="success">نقشه راه</Badge>
          {level ? <Badge variant="outline">{level}</Badge> : null}
          <span className="text-[11px] text-[color:var(--ds-muted)]">
            {stepCount.toLocaleString("fa-IR")} گام
          </span>
          <Badge variant="secondary">
            {completedCount.toLocaleString("fa-IR")}/{stepCount.toLocaleString("fa-IR")} تکمیل ساختاری
          </Badge>
        </div>

        <h3 className="text-base font-extrabold text-[color:var(--ds-fg)] group-hover:text-emerald-200">
          {title}
        </h3>
        {summary ? (
          <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{summary}</p>
        ) : null}

        <div aria-label="نمای پیشرفت ساخت‌یافته مسیر">
          <div className="mb-2 flex justify-between text-[10px] font-semibold text-[color:var(--ds-muted)]">
            <span>سطح مسیر</span>
            <span>پیش‌نمای ساختاری</span>
          </div>
          <div className="flex gap-1" aria-hidden>
            {resolved.map((step, index) => (
              <span
                key={index}
                className={[
                  "h-1.5 flex-1 rounded-full transition-colors",
                  step.status === "completed"
                    ? "bg-[color:var(--ds-success)] shadow-[0_0_10px_color-mix(in_srgb,var(--ds-success)_45%,transparent)]"
                    : step.status === "current"
                      ? "bg-[color:var(--ds-primary)] shadow-[0_0_10px_var(--ds-shadow-glow)]"
                      : step.status === "unlocked"
                        ? "bg-[color:color-mix(in_srgb,var(--ds-secondary)_55%,transparent)]"
                        : "bg-[color:color-mix(in_srgb,var(--ds-fg)_12%,transparent)]",
                ].join(" ")}
              />
            ))}
          </div>
        </div>

        <ol className="mt-auto space-y-2" aria-label={`مراحل ${title}`}>
          {resolved.map((node, index) => {
            const locked = node.status === "locked";
            return (
              <li
                key={`${node.label}-${index}`}
                className={[
                  "flex items-center gap-2 text-[13px] font-semibold",
                  locked
                    ? "text-[color:var(--ds-muted)] opacity-70"
                    : "text-[color:var(--ds-fg)]/90",
                ].join(" ")}
              >
                <span
                  className={[
                    "flex h-6 w-6 items-center justify-center rounded-full border text-[10px]",
                    node.status === "completed"
                      ? "border-[color:var(--ds-success)] bg-[color:color-mix(in_srgb,var(--ds-success)_18%,transparent)] text-[color:var(--ds-success)]"
                      : node.status === "current"
                        ? "border-[color:color-mix(in_srgb,var(--ds-primary)_55%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-primary)_18%,transparent)] text-[color:var(--ds-primary)]"
                        : locked
                          ? "border-[color:var(--ds-border)] bg-transparent text-[color:var(--ds-muted)]"
                          : "border-[color:color-mix(in_srgb,var(--ds-secondary)_45%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-secondary)_12%,transparent)] text-[color:var(--ds-secondary)]",
                  ].join(" ")}
                  aria-hidden
                >
                  {node.status === "completed" ? "✓" : locked ? "—" : index + 1}
                </span>
                <span className="flex-1">{node.label}</span>
                <span className="text-[10px] font-bold text-[color:var(--ds-muted)]">
                  {STATUS_LABEL[node.status]}
                </span>
              </li>
            );
          })}
        </ol>
      </Card>
    </Link>
  );
}
