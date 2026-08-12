import Link from "next/link";
import type { RecommendedLearningItemDto } from "@/lib/api/learning-personalization";

type RecommendationCardProps = {
  item: RecommendedLearningItemDto;
};

export function RecommendationCard({ item }: RecommendationCardProps) {
  const href =
    item.slug != null
      ? `/courses?slug=${encodeURIComponent(item.slug)}`
      : "/learning/assistant";

  return (
    <article
      dir="rtl"
      className="rounded-2xl border border-white/10 bg-white/[0.03] p-4"
    >
      <div className="flex items-center justify-between gap-2">
        <h3 className="text-[14px] font-bold text-white">{item.title}</h3>
        <span className="rounded-md bg-emerald-500/15 px-2 py-0.5 text-[11px] font-semibold text-emerald-300">
          {item.kind}
        </span>
      </div>
      {item.rationale ? (
        <p className="mt-2 text-[12px] leading-6 text-slate-400">{item.rationale}</p>
      ) : null}
      <Link
        href={href}
        className="focus-ring mt-3 inline-flex text-[12px] font-semibold text-violet-300 hover:text-violet-200"
      >
        مشاهده پیشنهاد
      </Link>
    </article>
  );
}
