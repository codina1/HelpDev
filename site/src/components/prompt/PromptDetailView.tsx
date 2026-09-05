"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { PromptExample } from "@/components/prompt/PromptExample";
import { PromptHero } from "@/components/prompt/PromptHero";
import { PromptInfoSidebar } from "@/components/prompt/PromptInfoSidebar";
import { PromptRoadmap } from "@/components/prompt/PromptRoadmap";
import { PromptTabs } from "@/components/prompt/PromptTabs";
import { PromptUsageGuide } from "@/components/prompt/PromptUsageGuide";
import { PromptVersionHistory } from "@/components/prompt/PromptVersionHistory";
import { PromptViewer } from "@/components/prompt/PromptViewer";
import { RelatedArticles, RelatedCourses } from "@/components/prompt/RelatedContent";
import {
  type PromptDetailTabId,
  type PromptDetailViewModel,
} from "@/data/prompt-detail";

type PromptDetailViewProps = {
  model: PromptDetailViewModel;
};

const TAB_IDS: Record<PromptDetailTabId, string> = {
  intro: "intro",
  prompt: "prompt",
  usage: "usage",
  example: "example",
  changelog: "changelog",
};

export function PromptDetailView({ model }: PromptDetailViewProps) {
  const [activeTab, setActiveTab] = useState<PromptDetailTabId>("prompt");
  const { detail } = model;

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
          "prompt") as PromptDetailTabId;
        setActiveTab(tab);
      },
      { rootMargin: "-20% 0px -55% 0px", threshold: [0.2, 0.45] },
    );
    nodes.forEach((n) => observer.observe(n));
    return () => observer.disconnect();
  }, []);

  function onTabChange(tab: PromptDetailTabId) {
    setActiveTab(tab);
    document.getElementById(TAB_IDS[tab])?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  return (
    <div className="bg-[#050816] pb-12 pt-4">
      <ArticlesContainer>
        <nav aria-label="مسیر صفحه" className="mb-4 text-[12px] text-[#94A3B8]" dir="rtl">
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
          <PromptHero model={model} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[minmax(0,1fr)_280px] xl:gap-7"
        >
          <div className="min-w-0 space-y-5" dir="rtl">
            <PromptTabs active={activeTab} onChange={onTabChange} />

            <section id="intro" className="scroll-mt-28 rounded-2xl border border-white/[0.08] bg-[#0B1224]/9 p-5">
              <h2 className="text-[18px] font-extrabold text-white">معرفی</h2>
              <p className="mt-3 text-[14.5px] leading-8 text-[#94A3B8]">{detail.description}</p>
              {detail.tags.length > 0 ? (
                <div className="mt-4 flex flex-wrap gap-1.5">
                  {detail.tags.map((tag) => (
                    <span
                      key={tag}
                      className="rounded-full border border-white/[0.08] bg-white/[0.03] px-2.5 py-1 text-[11px] font-semibold text-[#CBD5E1]"
                    >
                      #{tag}
                    </span>
                  ))}
                </div>
              ) : null}
            </section>

            <PromptViewer content={detail.content} language={model.language} />
            <PromptUsageGuide steps={model.usageSteps} />
            <PromptExample input={model.sampleInput} output={model.sampleOutput} />
            <PromptVersionHistory versions={model.versions} />

            <div className="grid grid-cols-1 gap-4 pt-2 lg:grid-cols-3">
              <RelatedArticles articles={model.relatedArticles} />
              <RelatedCourses courses={model.relatedCourses} />
              <PromptRoadmap roadmap={model.roadmap} />
            </div>
          </div>

          <aside className="xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <PromptInfoSidebar model={model} />
          </aside>
        </div>
      </ArticlesContainer>
    </div>
  );
}
