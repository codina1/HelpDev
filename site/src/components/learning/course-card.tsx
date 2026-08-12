import Link from "next/link";
import type { CourseSummaryDto } from "@/lib/api/learning";

type CourseCardProps = {
  course: CourseSummaryDto;
  href?: string;
  meta?: string;
  progressPercentage?: number;
};

/** Learning-surface course card backed by API course summaries (not static catalog). */
export function CourseCard({
  course,
  href,
  meta,
  progressPercentage,
}: CourseCardProps) {
  const link = href ?? (course.slug ? `/courses?slug=${encodeURIComponent(course.slug)}` : "/courses");

  return (
    <article
      dir="rtl"
      className="flex h-full flex-col rounded-2xl border border-white/10 bg-white/[0.03] p-4 transition hover:border-violet-500/30"
    >
      <div className="mb-2 flex flex-wrap items-center gap-2">
        <span className="rounded-md bg-white/5 px-2 py-0.5 text-[11px] font-semibold text-slate-300">
          {course.status}
        </span>
        {typeof progressPercentage === "number" ? (
          <span
            className="rounded-md bg-emerald-500/15 px-2 py-0.5 text-[11px] font-semibold text-emerald-300"
            aria-label={`پیشرفت ${progressPercentage} درصد`}
          >
            {progressPercentage}٪
          </span>
        ) : null}
      </div>
      <h3 className="flex-1 text-[15px] font-bold text-white">{course.title}</h3>
      {meta ? <p className="mt-2 text-[12px] text-slate-400">{meta}</p> : null}
      <Link
        href={link}
        className="focus-ring mt-4 inline-flex items-center justify-center rounded-xl bg-violet-500/20 px-3 py-2 text-[13px] font-semibold text-violet-200 hover:bg-violet-500/30"
      >
        ادامه / مشاهده
      </Link>
    </article>
  );
}
