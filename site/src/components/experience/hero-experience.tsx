"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { AIEntryExperience } from "@/components/experience/ai-entry-experience";
import { FeatureShowcase } from "@/components/experience/feature-showcase";
import { KnowledgeGalaxy } from "@/components/experience/knowledge-galaxy";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";
import { Button } from "@/components/ui/ds/button";
import { Badge } from "@/components/ui/ds/badge";
import {
  AnimatedBackground,
  GradientText,
  PublicContainer,
  PublicSection,
} from "@/components/ui/public/v2";

/**
 * Sprint 50E hero — AI engineering platform + interactive knowledge graph.
 */
export function HeroExperience() {
  const router = useRouter();
  const [paletteOpen, setPaletteOpen] = useState(false);

  return (
    <PublicSection
      className="overflow-hidden pb-8 pt-8 sm:pb-10 sm:pt-12 lg:pt-16"
      bare
      aria-labelledby="hero-exp-title"
    >
      <AnimatedBackground variant="hero" />
      <PublicContainer
        size="wide"
        className="relative grid items-center gap-8 lg:grid-cols-[0.95fr_1.05fr] lg:gap-12"
      >
        <div className="ds-slide order-3 lg:order-1">
          <KnowledgeGalaxy
            className="mx-auto"
            onCenterActivate={() => setPaletteOpen(true)}
          />
        </div>

        <div className="ds-slide order-1 space-y-5 text-center lg:order-2 lg:text-start">
          <Badge variant="ai">AI Engineering Platform</Badge>
          <h1
            id="hero-exp-title"
            className="text-3xl font-extrabold leading-[1.3] tracking-tight sm:text-4xl lg:text-[2.85rem]"
          >
            <GradientText as="span" animated className="block">
              هوش مهندسی،
            </GradientText>
            <span className="mt-1 block text-[color:var(--ds-fg)]">برای ساختن سریع‌تر و دقیق‌تر</span>
          </h1>
          <p className="mx-auto max-w-xl text-[14px] leading-8 text-[color:var(--ds-muted)] sm:text-[15px] lg:mx-0">
            گراف دانش تعاملی، Ask HelpDev AI، مسیرهای یادگیری و ابزارهای مهندسی — یک پلتفرم یکپارچه برای تصمیم‌گیری و ساخت.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-3 lg:justify-start">
            <Button href="/learning" size="lg" className="ds-glow">
              شروع یادگیری
            </Button>
            <Button
              variant="secondary"
              size="lg"
              onClick={() => setPaletteOpen(true)}
              aria-label="پرسش از AI"
            >
              پرسش از AI
            </Button>
          </div>
        </div>

        <div className="ds-slide order-2 lg:col-span-2 lg:order-3">
          <AIEntryExperience
            onOpenPalette={() => setPaletteOpen(true)}
            onSubmit={(q) => router.push(`/search?q=${encodeURIComponent(q)}`)}
          />
        </div>
      </PublicContainer>

      <PublicContainer size="wide" className="relative mt-8 ds-fade">
        <FeatureShowcase
          items={[
            {
              title: "دانش مهندسی",
              description: "مقالات و بینش‌های کاربردی",
              href: "/articles",
              accent: "primary",
            },
            {
              title: "ابزارها",
              description: "شتاب‌دهنده گردش‌کار توسعه",
              href: "/toolbox",
              accent: "cyan",
            },
            {
              title: "نقشه راه",
              description: "مسیر ساخت‌یافته مهارت",
              href: "/roadmap",
              accent: "primary",
            },
            {
              title: "جستجوی دانش",
              description: "فرمان سراسری Ctrl+K",
              href: "/search",
              accent: "ai",
            },
          ]}
        />
      </PublicContainer>

      <GlobalSearchPalette open={paletteOpen} onOpenChange={setPaletteOpen} />
    </PublicSection>
  );
}
