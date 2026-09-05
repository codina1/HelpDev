"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";

export function LearningSection({ course }: { course: CourseDetailModel }) {
  return (
    <section className="mt-8 scroll-mt-28 rounded-2xl border border-white/[0.08] bg-[#0D1528]/70 p-5 sm:p-6">
      <h2 className="text-[22px] font-extrabold text-white">چه چیزی یاد می‌گیرید؟</h2>

      <div
        dir="ltr"
        className="mt-5 grid grid-cols-1 items-center gap-6 lg:grid-cols-2"
      >
        <motion.div
          whileHover={{ scale: 1.02 }}
          className="relative overflow-hidden rounded-2xl border border-[#8B5CF6]/25 bg-[radial-gradient(circle_at_50%_40%,rgba(139,92,246,0.35),transparent_60%)] p-8"
        >
          <div className="flex aspect-[4/3] flex-col items-center justify-center text-center">
            <span className="text-[42px] font-black tracking-tight text-white drop-shadow-[0_0_24px_rgba(139,92,246,0.7)]">
              React
            </span>
            <p className="mt-2 text-[13px] font-bold text-[#C4B5FD]">Learn · Build · Practice · Get Hired</p>
            <img
              src={course.previewImage}
              alt=""
              className="mt-4 h-24 w-auto object-contain opacity-90 mix-blend-screen"
            />
          </div>
        </motion.div>

        <div dir="rtl">
          <ul className="space-y-2.5">
            {course.learningOutcomes.map((item) => (
              <li
                key={item}
                className="flex items-start gap-2.5 rounded-xl border border-white/[0.06] bg-[#070B18]/60 px-3 py-2.5 text-[13.5px] font-semibold text-[#E5E7EB]"
              >
                <span className="mt-0.5 inline-flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-[#8B5CF6]/20 text-[11px] text-[#C4B5FD]">
                  ✓
                </span>
                {item}
              </li>
            ))}
          </ul>
          <button
            type="button"
            className="mt-4 text-[13px] font-bold text-[#93C5FD] transition hover:text-white"
          >
            همین امروز یادگیری را شروع کنید ←
          </button>
        </div>
      </div>
    </section>
  );
}
