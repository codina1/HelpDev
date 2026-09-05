"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";
import { CoursePreview } from "@/components/course/CoursePreview";

type CourseHeroProps = {
  course: CourseDetailModel;
  onPreview?: () => void;
};

function MetaCard({
  label,
  value,
  children,
}: {
  label: string;
  value: string;
  children?: React.ReactNode;
}) {
  return (
    <div className="min-w-[140px] flex-1 rounded-xl border border-white/[0.08] bg-[#0D1528]/90 px-3.5 py-3 shadow-[0_0_18px_rgba(2,6,23,0.35)] backdrop-blur-sm">
      <p className="text-[11px] font-semibold text-[#64748B]">{label}</p>
      <div className="mt-1.5 flex items-center gap-2">
        {children}
        <p className="text-[13px] font-bold text-white">{value}</p>
      </div>
    </div>
  );
}

export function CourseHero({ course, onPreview }: CourseHeroProps) {
  return (
    <div
      dir="ltr"
      className="grid grid-cols-1 items-center gap-6 lg:grid-cols-2 lg:gap-8"
    >
      <div className="order-2 min-w-0 lg:order-1" dir="rtl">
        <span className="inline-flex items-center rounded-lg border border-[#2563EB]/35 bg-[#2563EB]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#93C5FD]">
          {course.category}
        </span>

        <h1 className="mt-3 text-[30px] font-extrabold leading-[1.25] tracking-tight text-white sm:text-[38px] lg:text-[44px]">
          {course.title}{" "}
          {course.titleAccent ? (
            <span className="bg-gradient-to-l from-[#A78BFA] to-[#8B5CF6] bg-clip-text text-transparent drop-shadow-[0_0_18px_rgba(139,92,246,0.55)]">
              {course.titleAccent}
            </span>
          ) : null}
        </h1>

        <p className="mt-3 max-w-xl text-[15px] leading-7 text-[#94A3B8] sm:text-[16px] sm:leading-8">
          {course.description}
        </p>

        <div className="mt-5 flex flex-wrap gap-2.5">
          <MetaCard label="مدرس" value={course.instructor.name}>
            <span className="inline-flex h-7 w-7 items-center justify-center rounded-full border border-[#8B5CF6]/35 bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[10px] font-bold text-white">
              {course.instructor.initials}
            </span>
          </MetaCard>
          <MetaCard
            label="مدت دوره"
            value={`${course.durationHours.toLocaleString("fa-IR")} ساعت`}
          />
          <MetaCard
            label="جلسات"
            value={`${course.sessionsCount.toLocaleString("fa-IR")} جلسه`}
          />
          <MetaCard label="سطح" value={course.levelLabel} />
        </div>

        <div className="mt-5 flex flex-wrap gap-2.5">
          <motion.button
            type="button"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            onClick={onPreview}
            className="inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-5 text-[14px] font-bold text-white shadow-[0_0_24px_rgba(139,92,246,0.4)]"
          >
            <svg viewBox="0 0 24 24" className="h-4 w-4 fill-current" aria-hidden>
              <path d="M8 5.5v13l11-6.5L8 5.5Z" />
            </svg>
            مشاهده پیش‌نمایش
          </motion.button>
          <motion.button
            type="button"
            whileHover={{ scale: 1.02 }}
            whileTap={{ scale: 0.98 }}
            className="inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-white/[0.12] bg-[#0D1528]/80 px-5 text-[14px] font-bold text-[#E5E7EB] backdrop-blur-sm transition hover:border-[#8B5CF6]/4"
          >
            <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" aria-hidden>
              <path
                d="M12 17.3 6.2 20.5l1.2-6.5L2.5 9.6l6.6-.9L12 2.8l2.9 5.9 6.6.9-4.9 4.4 1.2 6.5L12 17.3Z"
                stroke="currentColor"
                strokeWidth="1.6"
                strokeLinejoin="round"
              />
            </svg>
            افزودن به علاقه‌مندی‌ها
          </motion.button>
        </div>
      </div>

      <div className="order-1 lg:order-2">
        <CoursePreview course={course} onPlay={onPreview} />
      </div>
    </div>
  );
}
