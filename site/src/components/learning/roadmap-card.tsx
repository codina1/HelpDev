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
      className="rounded-2xl border border-white/10 bg-gradient-to-br from-violet-500/10 to-transparent p-4"
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold text-violet-300">نقشه راه</p>
          <h3 className="mt-1 text-[15px] font-bold text-white">{roadmap.goal}</h3>
        </div>
        <span
          className="rounded-md bg-white/10 px-2 py-0.5 text-[11px] font-semibold text-slate-200"
          aria-label={`وضعیت: ${roadmap.status}`}
        >
          {roadmap.status}
        </span>
      </div>
      <p className="mt-2 text-[12px] text-slate-400">{stepCount} گام پیشنهادی</p>
      <Link
        href="/learning/assistant"
        className="focus-ring mt-3 inline-flex text-[12px] font-semibold text-violet-300 hover:text-violet-200"
      >
        مشاهده نقشه راه
      </Link>
    </article>
  );
}
