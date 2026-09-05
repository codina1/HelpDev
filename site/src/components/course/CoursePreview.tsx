"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";

type CoursePreviewProps = {
  course: CourseDetailModel;
  onPlay?: () => void;
};

export function CoursePreview({ course, onPlay }: CoursePreviewProps) {
  return (
    <motion.button
      type="button"
      onClick={onPlay}
      whileHover={{ scale: 1.01 }}
      transition={{ type: "spring", stiffness: 260, damping: 22 }}
      className="group relative block w-full overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#0D1528] text-start shadow-[0_0_48px_rgba(139,92,246,0.28)]"
      aria-label="پخش پیش‌نمایش دوره"
    >
      <div className="relative aspect-[16/9] w-full overflow-hidden">
        <img
          src={course.previewImage}
          alt=""
          className="h-full w-full object-cover transition duration-500 group-hover:scale-[1.03]"
        />
        <span
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_60%_40%,rgba(139,92,246,0.45),transparent_55%)]"
          aria-hidden
        />
        <span
          className="pointer-events-none absolute inset-0 bg-gradient-to-t from-[#050816]/80 via-transparent to-transparent"
          aria-hidden
        />
        <span className="absolute inset-0 flex items-center justify-center">
          <span className="inline-flex h-16 w-16 items-center justify-center rounded-full border border-white/20 bg-black/45 text-white shadow-[0_0_28px_rgba(139,92,246,0.55)] backdrop-blur-md transition group-hover:scale-105">
            <svg viewBox="0 0 24 24" className="ms-0.5 h-7 w-7 fill-current" aria-hidden>
              <path d="M8 5.5v13l11-6.5L8 5.5Z" />
            </svg>
          </span>
        </span>
        <span className="absolute bottom-4 start-4 end-4 text-[13px] font-bold text-white/90">
          {course.previewCaption}
        </span>
      </div>
    </motion.button>
  );
}
