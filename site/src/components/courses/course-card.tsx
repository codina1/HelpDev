"use client";

import { useState } from "react";
import { formatCoursePrice } from "@/data/courses";
import type { Course } from "@/types";

type CourseCardProps = {
  course: Course;
};

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 8v4.2l2.5 1.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function TagIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M4 11.4V4.8h6.6l8.6 8.6-6.6 6.6L4 11.4Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
      <circle cx="8.2" cy="8.2" r="1.3" fill="currentColor" />
    </svg>
  );
}

function BookmarkIcon({ className, filled }: { className?: string; filled?: boolean }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill={filled ? "currentColor" : "none"} aria-hidden>
      <path d="M7 4.5h10a1 1 0 0 1 1 1V20l-6-3.2L6 20V5.5a1 1 0 0 1 1-1Z" stroke="currentColor" strokeWidth="1.8" strokeLinejoin="round" />
    </svg>
  );
}

const BADGE_STYLE: Record<string, string> = {
  مبتدی: "bg-[#7C3AED] text-white",
  متوسط: "bg-[#6D28D9] text-white",
  پیشرفته: "bg-[#4F46E5] text-white",
  جدید: "bg-gradient-to-l from-[#7C3AED] to-[#3B82F6] text-white",
};

/** Catalog card — cover · badge · bookmark · title · description · duration · price. */
export function CourseCard({ course }: CourseCardProps) {
  const [saved, setSaved] = useState(false);
  const isFree = course.price === 0;

  return (
    <article
      className="group flex h-full min-w-0 flex-col overflow-hidden rounded-[16px] border border-white/[0.07] bg-[#0B1120] shadow-[0_4px_16px_rgba(2,6,23,0.25)] transition duration-200 hover:border-[rgba(168,85,247,0.3)]"
      dir="rtl"
    >
      <div className="relative h-[140px] shrink-0 overflow-hidden bg-[#060914]">
        <img
          src={course.image}
          alt=""
          width={360}
          height={140}
          loading="lazy"
          decoding="async"
          className="h-full w-full object-cover transition duration-500 group-hover:scale-[1.04]"
        />
        <span
          className={[
            "absolute right-3 top-3 inline-flex items-center rounded-md px-2.5 py-[4px] text-[11px] font-bold",
            BADGE_STYLE[course.levelLabel] ?? "bg-[#7C3AED] text-white",
          ].join(" ")}
        >
          {course.levelLabel}
        </span>
        <button
          type="button"
          onClick={() => setSaved((current) => !current)}
          aria-pressed={saved}
          aria-label={saved ? "حذف از ذخیره‌ها" : "افزودن به ذخیره‌ها"}
          className={[
            "absolute left-3 top-3 inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/[0.1] bg-[#0B1120]/70 backdrop-blur-sm transition",
            saved ? "text-[#A855F7]" : "text-[#94A3B8] hover:text-white",
          ].join(" ")}
        >
          <BookmarkIcon className="h-4 w-4" filled={saved} />
        </button>
      </div>

      <div className="flex min-w-0 flex-1 flex-col gap-2 p-4">
        <h3 className="line-clamp-2 text-[14px] font-extrabold leading-6 text-white">
          {course.title}
        </h3>
        <p className="line-clamp-2 text-[12.5px] leading-[21px] text-[#8B98AC]">
          {course.description}
        </p>

        <div className="mt-auto flex items-center justify-between gap-2 border-t border-white/[0.06] pt-3 text-[11.5px] font-semibold">
          <span
            className={[
              "inline-flex items-center gap-1 whitespace-nowrap",
              isFree ? "text-[#34D399]" : "text-[#22D3EE]",
            ].join(" ")}
          >
            <TagIcon className="h-3.5 w-3.5 shrink-0" />
            <bdi>{formatCoursePrice(course.price)}</bdi>
          </span>
          <span className="inline-flex items-center gap-1 whitespace-nowrap text-[#64748B]">
            <ClockIcon className="h-3.5 w-3.5 shrink-0 text-[#7C3AED]" />
            <bdi>{course.duration}</bdi>
          </span>
        </div>
      </div>
    </article>
  );
}
