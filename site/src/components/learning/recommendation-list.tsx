"use client";

import type { LearningRecommendationDto } from "@/lib/api/learning-personalization";

type Props = {
  data: LearningRecommendationDto | null;
};

export function RecommendationList({ data }: Props) {
  if (!data) {
    return <p className="text-sm text-slate-400">هنوز پیشنهادی دریافت نشده است.</p>;
  }

  return (
    <div className="space-y-4" dir="rtl">
      <p className="text-sm leading-7 text-slate-300">{data.reason}</p>
      <ul className="space-y-3">
        {data.recommendedItems.map((item) => (
          <li
            key={`${item.kind}-${item.courseId ?? item.title}`}
            className="rounded-xl border border-white/10 bg-white/[0.03] p-3"
          >
            <div className="flex items-center justify-between gap-2">
              <p className="text-sm font-semibold text-white">{item.title}</p>
              <span className="text-[11px] text-emerald-300">{item.kind}</span>
            </div>
            {item.rationale ? <p className="mt-1 text-xs text-slate-400">{item.rationale}</p> : null}
          </li>
        ))}
      </ul>
      <div>
        <p className="mb-2 text-xs font-semibold text-slate-400">گام‌های بعدی</p>
        <ol className="list-decimal space-y-1 pr-5 text-sm text-slate-300">
          {data.nextSteps.map((step) => (
            <li key={step}>{step}</li>
          ))}
        </ol>
      </div>
    </div>
  );
}
