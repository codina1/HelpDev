"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import type {
  ArticleRelatedCourse,
  ArticleRelatedNewsItem,
  ArticleRoadmapCta,
} from "@/data/article-detail";
import type { MarketplaceArticle } from "@/data/articles";
import type { TocHeading } from "@/lib/public/content-helpers";
import { formatViewsShort } from "@/data/article-detail";
import { ArticleInfo } from "@/components/article/ArticleInfo";
import { ArticleShare } from "@/components/article/ArticleShare";
import { NewsletterCard } from "@/components/article/NewsletterCard";
import type { ArticleDetailViewModel } from "@/data/article-detail";

export function ArticleToc({ headings }: { headings: TocHeading[] }) {
  const [activeId, setActiveId] = useState(headings[0]?.id ?? "");

  useEffect(() => {
    if (headings.length === 0) return;
    const nodes = headings
      .map((h) => document.getElementById(h.id))
      .filter((n): n is HTMLElement => Boolean(n));
    if (nodes.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((e) => e.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio);
        if (visible[0]?.target?.id) setActiveId(visible[0].target.id);
      },
      { rootMargin: "-20% 0px -65% 0px", threshold: [0, 0.25, 0.5, 1] },
    );
    nodes.forEach((n) => observer.observe(n));
    return () => observer.disconnect();
  }, [headings]);

  if (headings.length === 0) {
    return (
      <aside className="rounded-2xl border border-dashed border-white/[0.1] bg-[#10182D]/8 p-4 text-[12px] text-[#94A3B8]">
        فهرست مطالب پس از افزودن عناوین نمایش داده می‌شود.
      </aside>
    );
  }

  return (
    <nav
      className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4 shadow-[0_0_28px_rgba(124,58,237,0.08)] backdrop-blur-xl"
      aria-label="در این مقاله"
    >
      <p className="mb-3 text-[13px] font-extrabold text-white">در این مقاله</p>
      <ol className="space-y-0.5">
        {headings.map((heading) => {
          const active = heading.id === activeId;
          return (
            <li key={heading.id}>
              <a
                href={`#${heading.id}`}
                className={[
                  "relative block rounded-lg px-2.5 py-1.5 text-[12.5px] leading-6 transition",
                  active
                    ? "bg-[#8B5CF6]/15 font-bold text-[#E9D5FF]"
                    : "text-[#94A3B8] hover:bg-white/[0.04] hover:text-white",
                ].join(" ")}
              >
                {active ? (
                  <span
                    className="absolute end-1.5 top-1/2 h-1.5 w-1.5 -translate-y-1/2 rounded-full bg-[#8B5CF6] shadow-[0_0_10px_rgba(139,92,246,0.9)]"
                    aria-hidden
                  />
                ) : null}
                {heading.text}
              </a>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}

export function ArticleRelatedNews({ items }: { items: ArticleRelatedNewsItem[] }) {
  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">اخبار مرتبط</h2>
      {items.length === 0 ? (
        <p className="mt-3 text-[12px] text-[#64748B]">موردی یافت نشد.</p>
      ) : (
        <ul className="mt-3 space-y-2.5">
          {items.map((item) => (
            <li key={item.id}>
              <Link
                href={item.href}
                className="group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
              >
                <span className="h-14 w-14 shrink-0 overflow-hidden rounded-lg border border-white/[0.08] bg-[#070B18]">
                  <img src={item.image} alt="" className="h-full w-full object-cover" />
                </span>
                <span className="min-w-0">
                  <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                    {item.title}
                  </span>
                  <span className="mt-1 block text-[11px] text-[#64748B]">{item.dateLabel}</span>
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </aside>
  );
}

export function ArticleCourseCard({ course }: { course: ArticleRelatedCourse }) {
  return (
    <motion.aside
      whileHover={{ y: -2 }}
      className="overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.25)] bg-[#10182D] shadow-[0_0_24px_rgba(139,92,246,0.12)]"
    >
      <div className={`h-28 bg-gradient-to-bl ${course.coverTone}`}>
        {course.image ? (
          <img src={course.image} alt="" className="h-full w-full object-cover mix-blend-screen" />
        ) : null}
      </div>
      <div className="p-4">
        <h2 className="text-[14px] font-extrabold text-white">{course.title}</h2>
        <p className="mt-1.5 text-[12px] leading-6 text-[#94A3B8]">{course.description}</p>
        <Link
          href={course.href}
          className="mt-3 inline-flex h-9 items-center justify-center rounded-xl bg-[#2563EB] px-3.5 text-[12px] font-bold text-white no-underline shadow-[0_0_14px_rgba(37,99,235,0.35)]"
        >
          مشاهده دوره‌ها
        </Link>
      </div>
    </motion.aside>
  );
}

export function ArticleSidebarLeft({
  headings,
  relatedNews,
}: {
  headings: TocHeading[];
  relatedNews: ArticleRelatedNewsItem[];
}) {
  return (
    <div className="space-y-4">
      <ArticleToc headings={headings} />
      <ArticleRelatedNews items={relatedNews} />
      <NewsletterCard compact />
    </div>
  );
}

export function ArticleSidebarRight({ model }: { model: ArticleDetailViewModel }) {
  return (
    <div className="space-y-4">
      <ArticleInfo model={model} />
      <ArticleShare title={model.title} />
      <NewsletterCard />
      <ArticleCourseCard course={model.relatedCourse} />
    </div>
  );
}

export function ArticleRelated({
  articles,
  course,
  roadmap,
}: {
  articles: MarketplaceArticle[];
  course: ArticleRelatedCourse;
  roadmap: ArticleRoadmapCta;
}) {
  return (
    <section className="mt-10 grid grid-cols-1 gap-4 md:grid-cols-3">
      <div className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4 md:col-span-1">
        <h2 className="text-[13px] font-extrabold text-white">مقالات مرتبط</h2>
        <ul className="mt-3 space-y-2.5">
          {articles.slice(0, 2).map((item) => (
            <li key={item.id}>
              <Link
                href={`/articles/${item.slug}`}
                className="group block no-underline"
              >
                <span className="mb-2 block h-28 overflow-hidden rounded-xl border border-white/[0.08] bg-[#070B18]">
                  <img src={item.coverImage} alt="" className="h-full w-full object-cover" />
                </span>
                <span className="line-clamp-2 text-[13px] font-bold text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {item.title}
                </span>
                <span className="mt-1 block text-[11px] text-[#64748B]">
                  {formatViewsShort(item.views)} بازدید · {item.readingMinutes.toLocaleString("fa-IR")} دقیقه
                </span>
              </Link>
            </li>
          ))}
        </ul>
      </div>

      <div className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
        <h2 className="text-[13px] font-extrabold text-white">دوره‌های مرتبط</h2>
        <div className="mt-3">
          <span className={`mb-2 block h-28 overflow-hidden rounded-xl bg-gradient-to-bl ${course.coverTone}`}>
            {course.image ? (
              <img src={course.image} alt="" className="h-full w-full object-cover mix-blend-screen" />
            ) : null}
          </span>
          <p className="text-[13px] font-bold text-white">{course.title}</p>
          <p className="mt-1 text-[11px] text-[#64748B]">{course.durationLabel ?? "—"}</p>
          <Link
            href={course.href}
            className="mt-3 inline-flex h-9 items-center rounded-xl bg-[#2563EB] px-3 text-[12px] font-bold text-white no-underline"
          >
            مشاهده دوره
          </Link>
        </div>
      </div>

      <motion.div
        whileHover={{ y: -2 }}
        className="rounded-2xl border border-[rgba(139,92,246,0.3)] bg-[linear-gradient(145deg,rgba(139,92,246,0.22),rgba(11,18,36,0.95))] p-5 shadow-[0_0_28px_rgba(139,92,246,0.15)]"
      >
        <p className="text-[11px] font-bold tracking-wide text-[#C4B5FD]">مسیر یادگیری</p>
        <h2 className="mt-1 text-[16px] font-extrabold text-white">{roadmap.title}</h2>
        <p className="mt-2 text-[12.5px] leading-6 text-[#CBD5E1]">{roadmap.description}</p>
        <Link
          href={roadmap.href}
          className="mt-4 inline-flex h-9 items-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-4 text-[12.5px] font-bold text-white no-underline"
        >
          {roadmap.ctaLabel}
        </Link>
      </motion.div>
    </section>
  );
}

/** Alias exports matching requested component names. */
export { ArticleSidebarLeft as ArticleSidebar };
