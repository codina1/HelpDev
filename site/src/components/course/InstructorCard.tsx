"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";

export function InstructorCard({ course }: { course: CourseDetailModel }) {
  const { instructor } = course;
  return (
    <section id="instructor" className="mt-10 scroll-mt-28">
      <h2 className="text-[22px] font-extrabold text-white">مدرس دوره</h2>
      <motion.div
        whileHover={{ y: -2 }}
        className="mt-4 rounded-2xl border border-white/[0.08] bg-[#0D1528]/90 p-5 shadow-[0_0_28px_rgba(139,92,246,0.12)]"
      >
        <div className="flex flex-col items-start gap-4 sm:flex-row sm:items-center">
          <div className="inline-flex h-16 w-16 shrink-0 items-center justify-center overflow-hidden rounded-full border border-[#8B5CF6]/4 bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[18px] font-extrabold text-white shadow-[0_0_22px_rgba(139,92,246,0.35)]">
            {instructor.avatarUrl ? (
              <img src={instructor.avatarUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              instructor.initials
            )}
          </div>
          <div className="min-w-0 flex-1">
            <h3 className="text-[17px] font-extrabold text-white">{instructor.name}</h3>
            <p className="mt-0.5 text-[13px] font-semibold text-[#A78BFA]">{instructor.role}</p>
            <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">{instructor.bio}</p>
            <div className="mt-3 flex gap-2">
              {["LinkedIn", "GitHub", "X"].map((label) => (
                <a
                  key={label}
                  href="#"
                  className="inline-flex h-8 items-center rounded-lg border border-white/[0.08] bg-[#070B18] px-2.5 text-[11px] font-bold text-[#94A3B8] transition hover:border-[#8B5CF6]/4 hover:text-white"
                >
                  {label}
                </a>
              ))}
            </div>
          </div>
        </div>
      </motion.div>
    </section>
  );
}

export function Requirements({ course }: { course: CourseDetailModel }) {
  return (
    <section id="requirements" className="mt-8 scroll-mt-28">
      <h2 className="text-[22px] font-extrabold text-white">پیش‌نیازهای دوره</h2>
      <div className="mt-4 rounded-2xl border border-white/[0.08] bg-[#0D1528]/90 p-5">
        <ul className="grid grid-cols-1 gap-2 sm:grid-cols-2">
          {course.requirements.map((item) => (
            <li
              key={item}
              className="flex items-center gap-2 rounded-xl border border-white/[0.05] bg-[#070B18]/50 px-3 py-2.5 text-[13.5px] font-semibold text-[#E5E7EB]"
            >
              <span className="inline-flex h-5 w-5 items-center justify-center rounded-full bg-[#2563EB]/20 text-[11px] text-[#93C5FD]">
                ✓
              </span>
              {item}
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
