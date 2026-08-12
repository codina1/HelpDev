"use client";

import type { LearningRoadmapDto } from "@/lib/api/learning-personalization";

type Props = {
  roadmap: LearningRoadmapDto | null;
  onApprove?: () => void;
  approving?: boolean;
};

export function RoadmapView({ roadmap, onApprove, approving }: Props) {
  if (!roadmap) {
    return <p className="text-sm text-slate-400">نقشه راهی ذخیره نشده است. ابتدا تولید کنید.</p>;
  }

  return (
    <div className="space-y-4" dir="rtl">
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-sm font-semibold text-white">{roadmap.goal}</p>
          <p className="text-xs text-slate-400">وضعیت: {roadmap.status}</p>
        </div>
        {roadmap.status === "Suggested" && onApprove ? (
          <button
            type="button"
            onClick={onApprove}
            disabled={approving}
            className="rounded-xl bg-emerald-600 px-3 py-2 text-xs font-bold text-white disabled:opacity-60"
          >
            {approving ? "..." : "تأیید نقشه راه"}
          </button>
        ) : null}
      </div>
      <ol className="space-y-3">
        {roadmap.steps.map((step) => (
          <li key={step.stepOrder} className="rounded-xl border border-white/10 bg-white/[0.03] p-3">
            <p className="text-sm font-semibold text-white">
              {step.stepOrder}. {step.title}
            </p>
            {step.description ? <p className="mt-1 text-xs text-slate-400">{step.description}</p> : null}
          </li>
        ))}
      </ol>
    </div>
  );
}
