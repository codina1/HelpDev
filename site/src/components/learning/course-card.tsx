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
      className="flex h-full flex-col rounded-2xl border border-[color:var(--ds-border)] bg-[color:var(--ds-surface)] p-4 transition hover:border-[color:color-mix(in_srgb,var(--ds-primary)_40%,var(--ds-border))]"
    >
      <div className="mb-2 flex flex-wrap items-center gap-2">
        <span className="rounded-md bg-[color:var(--ds-surface-elevated)] px-2 py-0.5 text-[11px] font-semibold text-[color:var(--ds-muted)]">
          {course.status}
        </span>
        {typeof progressPercentage === "number" ? (
          <span
            className="rounded-md bg-[color:color-mix(in_srgb,var(--ds-success)_16%,transparent)] px-2 py-0.5 text-[11px] font-semibold text-[color:var(--ds-success)]"
            aria-label={`پیشرفت ${progressPercentage} درصد`}
          >
            {progressPercentage}٪
          </span>
        ) : null}
      </div>
      <h3 className="flex-1 text-[15px] font-bold text-[color:var(--ds-fg)]">{course.title}</h3>
      {meta ? <p className="mt-2 text-[12px] text-[color:var(--ds-muted)]">{meta}</p> : null}
      <Link
        href={link}
        className="focus-ring mt-4 inline-flex items-center justify-center rounded-xl bg-[color:color-mix(in_srgb,var(--ds-primary)_16%,transparent)] px-3 py-2 text-[13px] font-semibold text-[color:var(--ds-primary-strong)] hover:bg-[color:color-mix(in_srgb,var(--ds-primary)_26%,transparent)]"
      >
        ادامه / مشاهده
      </Link>
    </article>
  );
}
