"use client";

import { useState } from "react";
import Link from "next/link";
import { motion, AnimatePresence } from "framer-motion";
import type {
  ToolDetailModel,
  ToolFeatureItem,
  ToolRelatedArticle,
  ToolRelatedCourse,
  ToolRelatedRoadmap,
  ToolScreenshotItem,
  ToolVersionItem,
} from "@/data/tool-detail";

export function ToolAbout({ model }: { model: ToolDetailModel }) {
  return (
    <section id="intro" className="scroll-mt-28 rounded-2xl border border-[rgba(139,92,246,0.18)] bg-[#10182D]/9 p-5 sm:p-6">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">درباره {model.name}</h2>
      <p className="mt-3 text-[14.5px] leading-8 text-[#94A3B8]">{model.longDescription}</p>
    </section>
  );
}

export function ToolFeatureGrid({ features }: { features: ToolFeatureItem[] }) {
  return (
    <section id="features" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">ویژگی‌های کلیدی</h2>
      <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
        {features.map((feature, index) => (
          <motion.article
            key={feature.id}
            initial={{ opacity: 0, y: 8 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: index * 0.03 }}
            whileHover={{ y: -2 }}
            className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4 shadow-[0_0_18px_rgba(139,92,246,0.08)]"
          >
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl bg-[#8B5CF6]/15 text-[#E9D5FF]">
              {feature.icon}
            </span>
            <h3 className="mt-3 text-[14px] font-extrabold text-white">{feature.title}</h3>
            <p className="mt-1.5 text-[12.5px] leading-6 text-[#94A3B8]">{feature.description}</p>
          </motion.article>
        ))}
      </div>
    </section>
  );
}

export function ToolGallery({ screenshots }: { screenshots: ToolScreenshotItem[] }) {
  const [active, setActive] = useState<ToolScreenshotItem | null>(null);

  return (
    <section className="mt-8">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">تصاویر محیط ابزار</h2>
      <div className="mt-4 flex gap-3 overflow-x-auto pb-2 [scrollbar-width:thin]">
        {screenshots.map((shot) => (
          <button
            key={shot.id}
            type="button"
            onClick={() => setActive(shot)}
            className="w-[220px] shrink-0 overflow-hidden rounded-2xl border border-white/[0.08] bg-[#070B18] text-start transition hover:border-[#8B5CF6]/45"
          >
            <img src={shot.image} alt="" className="h-28 w-full object-cover" />
            <span className="block px-3 py-2 text-[12px] font-bold text-[#CBD5E1]">{shot.title}</span>
          </button>
        ))}
      </div>

      <AnimatePresence>
        {active ? (
          <motion.div
            className="fixed inset-0 z-[80] flex items-center justify-center bg-black/75 p-4"
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={() => setActive(null)}
            role="dialog"
            aria-modal="true"
            aria-label={active.title}
          >
            <motion.div
              initial={{ scale: 0.96, opacity: 0 }}
              animate={{ scale: 1, opacity: 1 }}
              exit={{ scale: 0.96, opacity: 0 }}
              className="max-h-[85vh] w-full max-w-3xl overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.35)] bg-[#0B1224]"
              onClick={(e) => e.stopPropagation()}
            >
              <img src={active.image} alt={active.title} className="max-h-[70vh] w-full object-contain" />
              <div className="flex items-center justify-between gap-3 px-4 py-3">
                <p className="text-[13px] font-bold text-white">{active.title}</p>
                <button
                  type="button"
                  onClick={() => setActive(null)}
                  className="rounded-lg border border-white/[0.1] px-3 py-1.5 text-[12px] font-bold text-[#CBD5E1]"
                >
                  بستن
                </button>
              </div>
            </motion.div>
          </motion.div>
        ) : null}
      </AnimatePresence>
    </section>
  );
}

export function RelatedLearning({
  resources,
  articles,
  courses,
  roadmaps,
}: {
  resources: ToolRelatedArticle[];
  articles: ToolRelatedArticle[];
  courses: ToolRelatedCourse[];
  roadmaps: ToolRelatedRoadmap[];
}) {
  const hasAny =
    resources.length > 0 || articles.length > 0 || courses.length > 0 || roadmaps.length > 0;

  return (
    <section id="learning" className="mt-8 scroll-mt-28 space-y-8">
      {!hasAny ? (
        <p className="rounded-2xl border border-white/[0.08] bg-[#10182D]/9 px-4 py-6 text-center text-[13px] text-[#94A3B8]">
          محتوای مرتبطی پیدا نشد.
        </p>
      ) : null}

      {resources.length > 0 ? (
        <div>
          <h2 className="text-[18px] font-extrabold text-white">منابع آموزشی مرتبط</h2>
          <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
            {resources.map((item) => (
              <Link
                key={item.id}
                href={item.href}
                className="rounded-2xl border border-white/[0.08] bg-[#10182D]/9 p-4 no-underline transition hover:border-[#8B5CF6]/35"
              >
                <p className="text-[13.5px] font-bold text-white">{item.title}</p>
                <p className="mt-1 text-[11.5px] text-[#64748B]">{item.meta}</p>
              </Link>
            ))}
          </div>
        </div>
      ) : null}

      {articles.length > 0 ? (
        <div>
          <div className="flex items-center justify-between gap-3">
            <h2 className="text-[18px] font-extrabold text-white">مقالات مرتبط</h2>
            <Link href="/articles" className="text-[12px] font-bold text-[#C4B5FD]">
              مشاهده همه مقالات
            </Link>
          </div>
          <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
            {articles.map((item) => (
              <Link
                key={item.id}
                href={item.href}
                className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4 no-underline transition hover:border-[#8B5CF6]/35"
              >
                <p className="text-[13.5px] font-bold text-white">{item.title}</p>
                <p className="mt-1 text-[11.5px] text-[#64748B]">{item.meta}</p>
              </Link>
            ))}
          </div>
        </div>
      ) : null}

      {courses.length > 0 ? (
        <div>
          <h2 className="text-[18px] font-extrabold text-white">دوره‌های مرتبط</h2>
          <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-2">
            {courses.map((course) => (
              <Link
                key={course.id}
                href={course.href}
                className="flex gap-3 rounded-2xl border border-white/[0.08] bg-[#10182D]/9 p-3 no-underline transition hover:border-[#8B5CF6]/35"
              >
                <img src={course.image} alt="" className="h-16 w-16 rounded-xl object-cover" />
                <span className="min-w-0">
                  <span className="block text-[13px] font-bold text-white">{course.title}</span>
                  <span className="mt-1 block text-[11px] text-[#64748B]">
                    {course.level} · {course.duration}
                  </span>
                </span>
              </Link>
            ))}
          </div>
        </div>
      ) : null}

      {roadmaps.length > 0 ? (
        <div>
          <h2 className="text-[18px] font-extrabold text-white">مسیرهای یادگیری مرتبط</h2>
          <div className="mt-3 grid grid-cols-1 gap-3">
            {roadmaps.map((item) => (
              <Link
                key={item.id}
                href={item.href}
                className="rounded-2xl border border-[rgba(139,92,246,0.25)] bg-[linear-gradient(145deg,rgba(139,92,246,0.15),rgba(11,18,36,0.95))] p-4 no-underline"
              >
                <p className="text-[14px] font-extrabold text-white">{item.title}</p>
                <p className="mt-1 text-[12.5px] text-[#94A3B8]">{item.description}</p>
                <p className="mt-2 text-[11px] font-bold text-[#C4B5FD]">{item.stagesLabel}</p>
              </Link>
            ))}
          </div>
        </div>
      ) : null}
    </section>
  );
}

export function ToolVersions({ versions }: { versions: ToolVersionItem[] }) {
  return (
    <section id="versions" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white">نسخه‌ها</h2>
      <div className="mt-3 overflow-hidden rounded-2xl border border-white/[0.08] bg-[#0B1224]/95">
        <table className="w-full min-w-[420px] border-collapse text-start" dir="rtl">
          <thead>
            <tr className="border-b border-white/[0.08] text-[12px] text-[#64748B]">
              <th className="px-4 py-3 font-bold">نسخه</th>
              <th className="px-4 py-3 font-bold">تاریخ</th>
              <th className="px-4 py-3 font-bold">تغییرات</th>
            </tr>
          </thead>
          <tbody>
            {versions.map((row) => (
              <tr key={row.id} className="border-b border-white/[0.05] last:border-0">
                <td className="px-4 py-3 text-[13px] font-extrabold text-white">
                  {row.version}
                  {row.isLatest ? (
                    <span className="ms-2 rounded-md bg-[#8B5CF6]/2 px-1.5 py-0.5 text-[10px] font-bold text-[#E9D5FF]">
                      آخرین
                    </span>
                  ) : null}
                </td>
                <td className="px-4 py-3 text-[12.5px] text-[#94A3B8]">{row.dateLabel}</td>
                <td className="px-4 py-3 text-[12.5px] text-[#CBD5E1]">{row.summary}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}

export function ToolReviews({ model }: { model: ToolDetailModel }) {
  return (
    <section id="reviews" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white">نظرات کاربران</h2>
      <div className="mt-3 space-y-3">
        {[
          {
            id: "1",
            author: "سارا محمدی",
            comment: `${model.name} یکی از ابزارهای ثابت گردش‌کار روزانه‌ام است.`,
          },
          {
            id: "2",
            author: "علی رضایی",
            comment: "تجربه کاربری تمیز و اکوسیستم قوی؛ پیشنهاد می‌کنم.",
          },
        ].map((review) => (
          <article key={review.id} className="rounded-2xl border border-white/[0.08] bg-[#10182D]/9 p-4">
            <div className="flex items-center justify-between gap-3">
              <p className="text-[13.5px] font-bold text-white">{review.author}</p>
              <p className="text-[12px] font-bold text-[#FBBF24]">
                ★ {model.rating.toLocaleString("fa-IR")}
              </p>
            </div>
            <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">{review.comment}</p>
          </article>
        ))}
      </div>
    </section>
  );
}
