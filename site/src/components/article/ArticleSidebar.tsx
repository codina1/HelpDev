"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import type {
  ArticleRelatedCard,
  ArticleRelatedCourse,
  ArticleRelatedNewsItem,
  ArticleRoadmapCta,
  ArticleDetailViewModel,
} from "@/data/article-detail";
import type { TocHeading } from "@/lib/public/content-helpers";
import { ArticleInfo } from "@/components/article/ArticleInfo";
import { ArticleShare } from "@/components/article/ArticleShare";
import { NewsletterCard } from "@/components/article/NewsletterCard";

export function ArticleToc({
  headings,
  hub = "articles",
  collapsible = false,
}: {
  headings: TocHeading[];
  hub?: "news" | "articles";
  collapsible?: boolean;
}) {
  const [activeId, setActiveId] = useState(headings[0]?.id ?? "");
  const [open, setOpen] = useState(!collapsible);
  const title = hub === "news" ? "در این خبر" : "در این مقاله";

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
      aria-label={title}
    >
      {collapsible ? (
        <button
          type="button"
          className="focus-ring flex w-full items-center justify-between text-[13px] font-extrabold text-white"
          aria-expanded={open}
          onClick={() => setOpen((v) => !v)}
        >
          {title}
          <span aria-hidden className="text-[#94A3B8]">
            {open ? "−" : "+"}
          </span>
        </button>
      ) : (
        <p className="mb-3 text-[13px] font-extrabold text-white">{title}</p>
      )}
      {open ? (
        <ol className={collapsible ? "mt-3 space-y-0.5" : "space-y-0.5"}>
          {headings.map((heading) => {
            const active = heading.id === activeId;
            return (
              <li key={heading.id}>
                <a
                  href={`#${heading.id}`}
                  onClick={(event) => {
                    event.preventDefault();
                    document.getElementById(heading.id)?.scrollIntoView({
                      behavior: "smooth",
                      block: "start",
                    });
                    setActiveId(heading.id);
                  }}
                  className={[
                    "focus-ring relative block rounded-lg px-2.5 py-1.5 text-[12.5px] leading-6 transition",
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
      ) : null}
    </nav>
  );
}

export function ArticleRelatedNews({
  items,
  hub = "news",
}: {
  items: ArticleRelatedNewsItem[];
  hub?: "news" | "articles";
}) {
  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">
        {hub === "news" ? "اخبار مرتبط" : "مطالب مرتبط"}
      </h2>
      {items.length === 0 ? (
        <p className="mt-3 text-[12px] text-[#64748B]">موردی یافت نشد.</p>
      ) : (
        <ul className="mt-3 space-y-2.5">
          {items.map((item) => (
            <li key={item.id}>
              <Link
                href={item.href}
                className="focus-ring group flex gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
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

export function ArticleLearningCta({
  course,
  roadmap,
}: {
  course: ArticleRelatedCourse;
  roadmap: ArticleRoadmapCta;
}) {
  return (
    <motion.aside
      whileHover={{ y: -2 }}
      className="overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.28)] bg-[#10182D] shadow-[0_0_24px_rgba(139,92,246,0.14)]"
    >
      <div className={`relative h-24 bg-gradient-to-bl ${course.coverTone}`}>
        {course.image ? (
          <img src={course.image} alt="" className="h-full w-full object-cover mix-blend-screen opacity-90" />
        ) : null}
        <span className="absolute inset-0 bg-gradient-to-t from-[#10182D] via-transparent to-transparent" aria-hidden />
      </div>
      <div className="p-4">
        <p className="text-[11px] font-bold tracking-wide text-[#C4B5FD]">مسیر یادگیری</p>
        <h2 className="mt-1 text-[14px] font-extrabold text-white">{roadmap.title}</h2>
        <p className="mt-1.5 text-[12px] leading-6 text-[#94A3B8]">{roadmap.description}</p>
        <Link
          href={roadmap.href}
          className="focus-ring mt-3 inline-flex h-9 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-3.5 text-[12px] font-bold text-white no-underline shadow-[0_0_14px_rgba(139,92,246,0.35)]"
        >
          {roadmap.ctaLabel}
        </Link>
      </div>
    </motion.aside>
  );
}

export function ArticleSidebarLeft({
  headings,
  relatedNews,
  hub = "articles",
}: {
  headings: TocHeading[];
  relatedNews: ArticleRelatedNewsItem[];
  hub?: "news" | "articles";
}) {
  return (
    <div className="space-y-4">
      <ArticleToc headings={headings} hub={hub} />
      <ArticleRelatedNews items={relatedNews} hub={hub} />
      <NewsletterCard compact />
    </div>
  );
}

export function ArticleSidebarRight({ model }: { model: ArticleDetailViewModel }) {
  return (
    <div className="space-y-4">
      <ArticleInfo model={model} hub={model.hub} />
      <ArticleShare title={model.title} />
      <ArticleLearningCta course={model.relatedCourse} roadmap={model.roadmap} />
    </div>
  );
}

export function ArticleRelated({ cards }: { cards: ArticleRelatedCard[] }) {
  if (cards.length === 0) return null;

  return (
    <section className="mt-8" aria-labelledby="related-content-heading">
      <h2 id="related-content-heading" className="mb-4 text-[18px] font-extrabold text-white">
        مطالب مرتبط
      </h2>
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {cards.map((item) => (
          <Link
            key={item.id}
            href={item.href}
            className="focus-ring group flex h-full flex-col overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.18)] bg-[#10182D]/95 no-underline shadow-[0_0_22px_rgba(2,6,23,0.35)] transition hover:border-[rgba(139,92,246,0.4)]"
          >
            <span className="aspect-[16/10] overflow-hidden bg-[#070B18]">
              <img
                src={item.image}
                alt=""
                className="h-full w-full object-cover transition duration-300 group-hover:scale-[1.03]"
              />
            </span>
            <span className="flex flex-1 flex-col p-4">
              <span className="line-clamp-2 text-[14px] font-extrabold leading-6 text-white group-hover:text-[#E9D5FF]">
                {item.title}
              </span>
              <span className="mt-auto pt-3 text-[11.5px] font-semibold text-[#64748B]">
                {item.viewsLabel} بازدید · {item.readingTime}
              </span>
            </span>
          </Link>
        ))}
      </div>
    </section>
  );
}

/** Alias exports matching requested component names. */
export { ArticleSidebarLeft as ArticleSidebar };
