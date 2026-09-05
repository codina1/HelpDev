"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import type { RelatedCourseCard } from "@/data/course-detail";

export function RelatedCourses({ courses }: { courses: RelatedCourseCard[] }) {
  return (
    <section className="rounded-2xl border border-white/[0.08] bg-[#0D1528]/90 p-4 backdrop-blur-xl">
      <h2 className="text-[14px] font-extrabold text-white">دوره‌های مرتبط</h2>
      <ul className="mt-3 space-y-2.5">
        {courses.map((course) => (
          <li key={course.id}>
            <Link
              href={`/courses/${course.slug}`}
              className="group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span className="relative h-14 w-14 shrink-0 overflow-hidden rounded-lg border border-white/[0.08] bg-[#070B18]">
                <img
                  src={course.image}
                  alt=""
                  className="h-full w-full object-cover mix-blend-screen transition group-hover:scale-105"
                />
              </span>
              <span className="min-w-0 flex-1">
                <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {course.title}
                </span>
                <span className="mt-1 flex flex-wrap items-center gap-2 text-[11px] text-[#64748B]">
                  <span>{course.durationLabel}</span>
                  <span>★ {course.rating.toLocaleString("fa-IR")}</span>
                </span>
                <span className="mt-1 block text-[11.5px] font-bold text-[#C4B5FD]">
                  {course.priceLabel}
                </span>
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}

export function Reviews({
  reviews,
  rating,
}: {
  reviews: import("@/data/course-detail").CourseReview[];
  rating: number;
}) {
  return (
    <section id="reviews" className="mt-10 scroll-mt-28">
      <div className="flex flex-wrap items-end justify-between gap-3">
        <h2 className="text-[22px] font-extrabold text-white">نظرات دانشجویان</h2>
        <p className="text-[13px] font-bold text-[#C4B5FD]">
          میانگین ★ {rating.toLocaleString("fa-IR")} از ۵
        </p>
      </div>
      <div className="mt-4 grid grid-cols-1 gap-3 md:grid-cols-3">
        {reviews.map((review, index) => (
          <motion.article
            key={review.id}
            initial={{ opacity: 0, y: 10 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: index * 0.05 }}
            className="rounded-xl border border-white/[0.08] bg-[#0D1528]/90 p-4"
          >
            <div className="flex items-center justify-between gap-2">
              <p className="text-[13px] font-extrabold text-white">{review.author}</p>
              <p className="text-[11px] text-[#64748B]">{review.dateLabel}</p>
            </div>
            <p className="mt-1 text-[12px] font-bold text-[#FBBF24]">
              {"★".repeat(review.rating)}
              <span className="text-[#334155]">{"★".repeat(Math.max(0, 5 - review.rating))}</span>
            </p>
            <p className="mt-2 text-[12.5px] leading-6 text-[#94A3B8]">{review.comment}</p>
          </motion.article>
        ))}
      </div>
    </section>
  );
}

export function CourseProjects({ projects }: { projects: string[] }) {
  return (
    <section id="projects" className="mt-10 scroll-mt-28">
      <h2 className="text-[22px] font-extrabold text-white">پروژه‌های دوره</h2>
      <ul className="mt-4 grid grid-cols-1 gap-2.5 sm:grid-cols-3">
        {projects.map((project) => (
          <li
            key={project}
            className="rounded-xl border border-[#8B5CF6]/2 bg-[#0D1528]/90 px-4 py-4 text-[13.5px] font-bold text-[#E5E7EB] shadow-[0_0_18px_rgba(139,92,246,0.1)]"
          >
            {project}
          </li>
        ))}
      </ul>
    </section>
  );
}
