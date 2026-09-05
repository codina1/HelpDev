"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { RoadmapAbout, RoadmapTimeline } from "@/components/roadmap-detail/RoadmapTimeline";
import { RoadmapHero } from "@/components/roadmap-detail/RoadmapHero";
import {
  RoadmapCertificateBanner,
  RoadmapCommunityCard,
  RoadmapInfoSidebar,
  RoadmapProgressCard,
  RoadmapRelatedList,
  RoadmapResourcesCard,
  RoadmapSocialProof,
  RoadmapStepsNav,
} from "@/components/roadmap-detail/RoadmapSidebars";
import { RoadmapTabs } from "@/components/roadmap-detail/RoadmapTabs";
import type { RoadmapDetailModel, RoadmapDetailTabId } from "@/data/roadmap-detail";

type RoadmapDetailViewProps = {
  model: RoadmapDetailModel;
};

const TAB_IDS: Record<RoadmapDetailTabId, string> = {
  intro: "intro",
  steps: "steps",
  projects: "projects",
  resources: "resources",
  faq: "faq",
  reviews: "reviews",
};

export function RoadmapDetailView({ model }: RoadmapDetailViewProps) {
  const [activeTab, setActiveTab] = useState<RoadmapDetailTabId>("intro");

  useEffect(() => {
    const nodes = Object.values(TAB_IDS)
      .map((id) => document.getElementById(id))
      .filter((n): n is HTMLElement => Boolean(n));
    if (nodes.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((e) => e.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio);
        const id = visible[0]?.target?.id;
        if (!id) return;
        const tab = (Object.entries(TAB_IDS).find(([, v]) => v === id)?.[0] ??
          "intro") as RoadmapDetailTabId;
        setActiveTab(tab);
      },
      { rootMargin: "-20% 0px -55% 0px", threshold: [0.15, 0.4] },
    );
    nodes.forEach((n) => observer.observe(n));
    return () => observer.disconnect();
  }, []);

  function onTabChange(tab: RoadmapDetailTabId) {
    setActiveTab(tab);
    document.getElementById(TAB_IDS[tab])?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  return (
    <div className="bg-[#050816] pb-12 pt-4">
      <ArticlesContainer>
        <nav aria-label="مسیر صفحه" className="mb-5 text-[12px] text-[#94A3B8]" dir="rtl">
          <ol className="flex flex-wrap items-center gap-1.5">
            {model.breadcrumb.map((item, index) => {
              const last = index === model.breadcrumb.length - 1;
              return (
                <li key={`${item.label}-${index}`} className="inline-flex items-center gap-1.5">
                  {index > 0 ? <span className="text-[#64748B]">›</span> : null}
                  {item.href && !last ? (
                    <Link href={item.href} className="hover:text-[#C4B5FD]">
                      {item.label}
                    </Link>
                  ) : (
                    <span className={last ? "text-[#CBD5E1]" : ""}>{item.label}</span>
                  )}
                </li>
              );
            })}
          </ol>
        </nav>

        <div className="mb-6">
          <RoadmapHero model={model} onStart={() => onTabChange("steps")} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[260px_minmax(0,1fr)_280px] xl:gap-7"
        >
          {/* LEFT: progress / steps nav / resources */}
          <aside className="order-3 space-y-4 xl:order-1 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <div className="hidden xl:block">
              <RoadmapStepsNav steps={model.steps} />
            </div>
            <RoadmapProgressCard model={model} />
            <div className="hidden xl:block">
              <RoadmapResourcesCard resources={model.resources} />
            </div>
            <RoadmapCommunityCard />
          </aside>

          {/* CENTER */}
          <div className="order-1 min-w-0 space-y-5 xl:order-2" dir="rtl">
            <RoadmapTabs active={activeTab} onChange={onTabChange} />
            <RoadmapAbout model={model} />
            <RoadmapTimeline steps={model.steps} />

            <section id="projects" className="mt-8 scroll-mt-28">
              <h2 className="text-[18px] font-extrabold text-white">پروژه‌ها</h2>
              <div className="mt-3 grid grid-cols-1 gap-3 sm:grid-cols-3">
                {model.projects.map((project) => (
                  <article
                    key={project.id}
                    className="rounded-2xl border border-white/[0.08] bg-[#10182D]/9 p-4"
                  >
                    <h3 className="text-[14px] font-extrabold text-white">{project.title}</h3>
                    <p className="mt-2 text-[12.5px] leading-6 text-[#94A3B8]">{project.description}</p>
                  </article>
                ))}
              </div>
            </section>

            <section id="resources" className="mt-8 scroll-mt-28 xl:hidden">
              <RoadmapResourcesCard resources={model.resources} />
            </section>

            <section id="faq" className="mt-8 scroll-mt-28 space-y-2">
              <h2 className="text-[18px] font-extrabold text-white">سوالات متداول</h2>
              {model.faq.map((item) => (
                <details
                  key={item.id}
                  className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/9 p-4 open:border-[rgba(139,92,246,0.3)]"
                >
                  <summary className="cursor-pointer text-[13.5px] font-bold text-white">
                    {item.question}
                  </summary>
                  <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">{item.answer}</p>
                </details>
              ))}
            </section>

            <section id="reviews" className="mt-8 scroll-mt-28">
              <h2 className="text-[18px] font-extrabold text-white">نظرات</h2>
              <div className="mt-3 space-y-3">
                {model.reviews.map((review) => (
                  <article
                    key={review.id}
                    className="rounded-2xl border border-white/[0.08] bg-[#10182D]/9 p-4"
                  >
                    <div className="flex items-center justify-between gap-3">
                      <p className="text-[13.5px] font-bold text-white">{review.author}</p>
                      <p className="text-[12px] font-bold text-[#FBBF24]">
                        ★ {review.rating.toLocaleString("fa-IR")}
                      </p>
                    </div>
                    <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">{review.comment}</p>
                  </article>
                ))}
              </div>
            </section>
          </div>

          {/* RIGHT: info / related / certificate */}
          <aside className="order-2 space-y-4 xl:order-3 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <RoadmapInfoSidebar model={model} />
            <RoadmapSocialProof model={model} />
            <RoadmapRelatedList items={model.related} />
            <RoadmapCertificateBanner />
          </aside>
        </div>
      </ArticlesContainer>
    </div>
  );
}
