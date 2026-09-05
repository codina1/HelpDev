"use client";

import { motion } from "framer-motion";
import { COURSE_DETAIL_TABS, type CourseDetailTabId } from "@/data/course-detail";

type CourseTabsProps = {
  active: CourseDetailTabId;
  onChange: (id: CourseDetailTabId) => void;
};

export function CourseTabs({ active, onChange }: CourseTabsProps) {
  return (
    <div
      className="overflow-x-auto rounded-xl border border-white/[0.08] bg-[#0D1528]/80 p-1.5 backdrop-blur-xl [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="rtl"
    >
      <div className="flex min-w-max items-center gap-1" role="tablist" aria-label="بخش‌های دوره">
        {COURSE_DETAIL_TABS.map((tab) => {
          const isActive = tab.id === active;
          return (
            <button
              key={tab.id}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => onChange(tab.id)}
              className={[
                "relative inline-flex h-10 items-center rounded-lg px-3.5 text-[12.5px] font-bold transition",
                isActive
                  ? "text-white"
                  : "text-[#94A3B8] hover:bg-white/[0.04] hover:text-white",
              ].join(" ")}
            >
              {isActive ? (
                <motion.span
                  layoutId="course-tab-glow"
                  className="absolute inset-0 rounded-lg bg-[#8B5CF6]/20 shadow-[0_0_18px_rgba(139,92,246,0.35)]"
                  transition={{ type: "spring", stiffness: 320, damping: 28 }}
                />
              ) : null}
              <span className="relative z-[1]">{tab.label}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
