"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArticlesContainer } from "@/components/articles/articles-container";
import {
  RelatedLearning,
  ToolAbout,
  ToolFeatureGrid,
  ToolGallery,
  ToolReviews,
  ToolVersions,
} from "@/components/tool-detail/ToolContent";
import { ToolHero } from "@/components/tool-detail/ToolHero";
import { ToolSidebar } from "@/components/tool-detail/ToolSidebar";
import { ToolTabs } from "@/components/tool-detail/ToolTabs";
import type { ToolDetailModel, ToolDetailTabId } from "@/data/tool-detail";

type ToolDetailViewProps = {
  model: ToolDetailModel;
};

const TAB_IDS: Record<ToolDetailTabId, string> = {
  intro: "intro",
  features: "features",
  learning: "learning",
  reviews: "reviews",
  versions: "versions",
  similar: "similar",
};

export function ToolDetailView({ model }: ToolDetailViewProps) {
  const [activeTab, setActiveTab] = useState<ToolDetailTabId>("intro");

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
          "intro") as ToolDetailTabId;
        setActiveTab(tab);
      },
      { rootMargin: "-20% 0px -55% 0px", threshold: [0.15, 0.4] },
    );
    nodes.forEach((n) => observer.observe(n));
    return () => observer.disconnect();
  }, []);

  function onTabChange(tab: ToolDetailTabId) {
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
          <ToolHero model={model} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[minmax(0,1fr)_300px] xl:gap-7"
        >
          <div className="min-w-0 space-y-5" dir="rtl">
            <ToolTabs active={activeTab} onChange={onTabChange} />
            <ToolAbout model={model} />
            <ToolFeatureGrid features={model.features} />
            <ToolGallery screenshots={model.screenshots} />
            <RelatedLearning
              resources={model.learningResources}
              articles={model.relatedArticles}
              courses={model.relatedCourses}
              roadmaps={model.relatedRoadmaps}
            />
            <ToolVersions versions={model.versions} />
            <ToolReviews model={model} />
            <div className="xl:hidden">
              <ToolSidebar model={model} />
            </div>
          </div>

          <aside className="hidden xl:sticky xl:top-20 xl:block xl:self-start" dir="rtl">
            <ToolSidebar model={model} />
          </aside>
        </div>
      </ArticlesContainer>
    </div>
  );
}
