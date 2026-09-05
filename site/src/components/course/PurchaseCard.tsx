"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";
import { formatStudents, formatToman } from "@/data/course-detail";

type PurchaseCardProps = {
  course: CourseDetailModel;
};

export function PurchaseCard({ course }: PurchaseCardProps) {
  return (
    <motion.aside
      initial={{ opacity: 0, y: 12 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.35 }}
      className="rounded-2xl border border-white/[0.08] bg-[#0D1528]/95 p-5 shadow-[0_0_40px_rgba(139,92,246,0.18)] backdrop-blur-xl"
    >
      <span className="inline-flex items-center rounded-lg border border-[#8B5CF6]/35 bg-[#8B5CF6]/15 px-2.5 py-1 text-[11px] font-bold text-[#E9D5FF]">
        پرطرفدارترین دوره
      </span>

      <div className="mt-4">
        {course.originalPrice ? (
          <p className="text-[13px] text-[#64748B] line-through">
            {formatToman(course.originalPrice)}
          </p>
        ) : null}
        <p className="mt-1 text-[26px] font-extrabold tracking-tight text-white">
          {formatToman(course.price)}
        </p>
        {course.discountPercent ? (
          <span className="mt-2 inline-flex rounded-md bg-emerald-500/15 px-2 py-0.5 text-[11px] font-bold text-emerald-300">
            {course.discountPercent.toLocaleString("fa-IR")}٪ تخفیف
          </span>
        ) : null}
      </div>

      <motion.button
        type="button"
        whileHover={{ scale: 1.02 }}
        whileTap={{ scale: 0.98 }}
        className="mt-5 inline-flex h-11 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] text-[14px] font-bold text-white shadow-[0_0_22px_rgba(139,92,246,0.4)]"
      >
        افزودن به سبد خرید
      </motion.button>

      <motion.button
        type="button"
        whileHover={{ scale: 1.01 }}
        whileTap={{ scale: 0.98 }}
        className="mt-2.5 inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#070B18] text-[13px] font-bold text-[#E5E7EB] transition hover:border-[#8B5CF6]/35"
      >
        <span aria-hidden>⚡</span>
        خرید سریع
      </motion.button>

      <ul className="mt-5 space-y-2.5 border-t border-white/[0.08] pt-4">
        {course.features.map((feature) => (
          <li key={feature} className="flex items-start gap-2 text-[12.5px] text-[#CBD5E1]">
            <span className="mt-0.5 inline-flex h-4 w-4 shrink-0 items-center justify-center rounded-full bg-emerald-500/20 text-[10px] font-bold text-emerald-300">
              ✓
            </span>
            {feature}
          </li>
        ))}
      </ul>

      <div className="mt-5 grid grid-cols-4 gap-2 border-t border-white/[0.08] pt-4">
        {[
          { label: "امتیاز", value: course.rating.toLocaleString("fa-IR") },
          { label: "جلسه", value: course.sessionsCount.toLocaleString("fa-IR") },
          { label: "ساعت", value: `${course.durationHours.toLocaleString("fa-IR")}+` },
          { label: "دانشجو", value: formatStudents(course.studentsCount) },
        ].map((stat) => (
          <div key={stat.label} className="text-center">
            <p className="text-[13px] font-extrabold text-white">{stat.value}</p>
            <p className="mt-0.5 text-[10px] text-[#64748B]">{stat.label}</p>
          </div>
        ))}
      </div>
    </motion.aside>
  );
}
