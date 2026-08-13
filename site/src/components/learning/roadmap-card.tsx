import Link from "next/link";
import type { LearningRoadmapDto } from "@/lib/api/learning-personalization";

type RoadmapCardProps = {
  roadmap: LearningRoadmapDto;
};

export function RoadmapCard({ roadmap }: RoadmapCardProps) {
  const stepCount = roadmap.steps.length;

  return (
    <article
      dir="rtl"
      className="rounded-2xl border border-[color:var(--ds-border-strong)] bg-gradient-to-br from-[color:color-mix(in_srgb,var(--ds-primary)_14%,transparent)] to-[color:var(--ds-surface)] p-4"
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold text-[color:var(--ds-primary-strong)]">نقشه راه</p>
          <h3 className="mt-1 text-[15px] font-bold text-[color:var(--ds-fg)]">{roadmap.goal}</h3>
        </div>
        <span
          className="rounded-md bg-[color:var(--ds-surface-elevated)] px-2 py-0.5 text-[11px] font-semibold text-[color:var(--ds-fg)]"
          aria-label={`وضعیت: ${roadmap.status}`}
        >
          {roadmap.status}
        </span>
      </div>
      <p className="mt-2 text-[12px] text-[color:var(--ds-muted)]">{stepCount} گام پیشنهادی</p>
      <Link
        href="/learning/assistant"
        className="focus-ring mt-3 inline-flex text-[12px] font-semibold text-[color:var(--ds-primary-strong)] hover:text-[color:var(--ds-primary)]"
      >
        مشاهده نقشه راه
      </Link>
    </article>
  );
}
