"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import type {
  PromptRelatedArticle,
  PromptRelatedCourse,
  PromptRoadmapCard,
} from "@/data/prompt-detail";

export function RelatedArticles({ articles }: { articles: PromptRelatedArticle[] }) {
  return (
    <section className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">مقالات مرتبط</h2>
      {articles.length === 0 ? (
        <p className="mt-3 text-[12px] text-[#64748B]">مقاله‌ای یافت نشد.</p>
      ) : (
        <ul className="mt-3 space-y-2.5">
          {articles.map((article) => (
            <li key={article.id}>
              <Link
                href={article.href}
                className="group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
              >
                <span className="h-14 w-14 shrink-0 overflow-hidden rounded-lg border border-white/[0.08] bg-[#070B18]">
                  <img src={article.image} alt="" className="h-full w-full object-cover mix-blend-screen" />
                </span>
                <span className="min-w-0">
                  <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                    {article.title}
                  </span>
                  <span className="mt-1 block text-[11px] text-[#64748B]">{article.viewsLabel}</span>
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

export function RelatedCourses({ courses }: { courses: PromptRelatedCourse[] }) {
  return (
    <section className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">دوره‌های مرتبط</h2>
      {courses.length === 0 ? (
        <p className="mt-3 text-[12px] text-[#64748B]">دوره‌ای یافت نشد.</p>
      ) : (
        <ul className="mt-3 space-y-2.5">
          {courses.map((course) => (
            <li key={course.id}>
              <Link
                href={course.href}
                className="group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
              >
                <span className="relative h-14 w-14 shrink-0 overflow-hidden rounded-lg border border-white/[0.08] bg-[#070B18]">
                  <img src={course.image} alt="" className="h-full w-full object-cover mix-blend-screen" />
                  <span className="absolute inset-0 flex items-center justify-center text-white/90">▶</span>
                </span>
                <span className="min-w-0">
                  <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                    {course.title}
                  </span>
                  <span className="mt-1 block text-[11px] text-[#64748B]">{course.durationLabel}</span>
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}

export function PromptRoadmap({ roadmap }: { roadmap: PromptRoadmapCard }) {
  return (
    <motion.section
      whileHover={{ y: -2 }}
      className="overflow-hidden rounded-2xl border border-[#8B5CF6]/25 bg-[linear-gradient(145deg,rgba(139,92,246,0.2),rgba(11,18,36,0.95))] p-5 shadow-[0_0_28px_rgba(139,92,246,0.18)]"
    >
      <p className="text-[11px] font-bold tracking-wide text-[#C4B5FD]">مسیر یادگیری</p>
      <h2 className="mt-1 text-[16px] font-extrabold text-white">{roadmap.title}</h2>
      <p className="mt-2 text-[12.5px] leading-6 text-[#CBD5E1]">{roadmap.description}</p>
      <Link
        href={roadmap.href}
        className="mt-4 inline-flex h-9 items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-4 text-[12.5px] font-bold text-white no-underline shadow-[0_0_16px_rgba(139,92,246,0.35)]"
      >
        {roadmap.ctaLabel}
      </Link>
    </motion.section>
  );
}
