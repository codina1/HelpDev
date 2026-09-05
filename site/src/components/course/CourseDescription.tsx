"use client";

import { motion } from "framer-motion";
import type { CourseDetailModel } from "@/data/course-detail";

const ICON: Record<CourseDetailModel["highlights"][number]["icon"], string> = {
  spark: "✦",
  briefcase: "▣",
  project: "◈",
};

export function CourseDescription({ course }: { course: CourseDetailModel }) {
  return (
    <section id="about" className="scroll-mt-28">
      <h2 className="text-[22px] font-extrabold text-white sm:text-[24px]">درباره این دوره</h2>
      <p className="mt-3 text-[15px] leading-8 text-[#94A3B8] sm:text-[16px]">{course.about}</p>

      <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-3">
        {course.highlights.map((item, index) => (
          <motion.div
            key={item.title}
            initial={{ opacity: 0, y: 10 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true, margin: "-40px" }}
            transition={{ delay: index * 0.06 }}
            whileHover={{ y: -3 }}
            className="rounded-xl border border-[#8B5CF6]/25 bg-[#0D1528]/90 p-4 shadow-[0_0_22px_rgba(139,92,246,0.12)]"
          >
            <span className="inline-flex h-8 w-8 items-center justify-center rounded-lg bg-[#8B5CF6]/15 text-[#C4B5FD]">
              {ICON[item.icon]}
            </span>
            <h3 className="mt-3 text-[14px] font-extrabold text-white">{item.title}</h3>
            <p className="mt-1.5 text-[12.5px] leading-6 text-[#94A3B8]">{item.description}</p>
          </motion.div>
        ))}
      </div>
    </section>
  );
}
