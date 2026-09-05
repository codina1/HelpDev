"use client";

import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import type { CourseCurriculumSection } from "@/data/course-detail";
import { sectionStats } from "@/data/course-detail";

type CurriculumAccordionProps = {
  sections: CourseCurriculumSection[];
};

export function CurriculumAccordion({ sections }: CurriculumAccordionProps) {
  const [openId, setOpenId] = useState<string | null>(sections[0]?.id ?? null);

  return (
    <section id="curriculum" className="mt-10 scroll-mt-28">
      <h2 className="text-[22px] font-extrabold text-white sm:text-[24px]">سرفصل‌های دوره</h2>

      <div className="mt-4 space-y-2.5">
        {sections.map((section, index) => {
          const open = openId === section.id;
          const stats = sectionStats(section);
          return (
            <div
              key={section.id}
              className="overflow-hidden rounded-xl border border-white/[0.08] bg-[#0D1528]/85"
            >
              <button
                type="button"
                aria-expanded={open}
                onClick={() => setOpenId(open ? null : section.id)}
                className="flex w-full items-center gap-3 px-3.5 py-3.5 text-start transition hover:bg-white/[0.03]"
              >
                <span className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-[#8B5CF6]/20 text-[13px] font-extrabold text-[#E9D5FF] shadow-[0_0_14px_rgba(139,92,246,0.3)]">
                  {(index + 1).toLocaleString("fa-IR")}
                </span>
                <span className="min-w-0 flex-1">
                  <span className="block text-[14px] font-bold text-white">{section.title}</span>
                  <span className="mt-0.5 block text-[11.5px] text-[#64748B]">
                    {stats.lessons.toLocaleString("fa-IR")} جلسه / {stats.hours.toLocaleString("fa-IR")}{" "}
                    ساعت
                  </span>
                </span>
                <motion.span
                  animate={{ rotate: open ? 180 : 0 }}
                  className="text-[#94A3B8]"
                  aria-hidden
                >
                  ▾
                </motion.span>
              </button>

              <AnimatePresence initial={false}>
                {open ? (
                  <motion.div
                    initial={{ height: 0, opacity: 0 }}
                    animate={{ height: "auto", opacity: 1 }}
                    exit={{ height: 0, opacity: 0 }}
                    transition={{ duration: 0.22 }}
                    className="overflow-hidden border-t border-white/[0.06]"
                  >
                    <ul className="space-y-1 px-3.5 py-3">
                      {section.lessons.map((lesson) => (
                        <li
                          key={lesson.id}
                          className="flex items-center justify-between gap-3 rounded-lg px-2 py-2 text-[13px] text-[#CBD5E1] hover:bg-white/[0.03]"
                        >
                          <span className="inline-flex min-w-0 items-center gap-2">
                            <span className="text-[#8B5CF6]" aria-hidden>
                              ▶
                            </span>
                            <span className="truncate">{lesson.title}</span>
                            {lesson.isPreview ? (
                              <span className="rounded bg-[#2563EB]/20 px-1.5 py-0.5 text-[10px] font-bold text-[#93C5FD]">
                                رایگان
                              </span>
                            ) : null}
                          </span>
                          <span className="shrink-0 text-[11px] text-[#64748B]">
                            {lesson.durationMinutes.toLocaleString("fa-IR")} دقیقه
                          </span>
                        </li>
                      ))}
                    </ul>
                  </motion.div>
                ) : null}
              </AnimatePresence>
            </div>
          );
        })}
      </div>
    </section>
  );
}
