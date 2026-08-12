"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { KnowledgeGraphVisual } from "@/components/public/home/v2/knowledge-graph-visual";
import {
  AICommandBox,
  AnimatedBackground,
  FeatureGrid,
  GlowButton,
  GradientText,
  PremiumBadge,
  PublicContainer,
  PublicSection,
} from "@/components/ui/public/v2";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";

export function HomeHeroV2() {
  const router = useRouter();
  const [paletteOpen, setPaletteOpen] = useState(false);

  return (
    <PublicSection className="overflow-hidden pb-8 pt-10 sm:pb-10 sm:pt-14 lg:pt-16" bare aria-labelledby="hero-v2-title">
      <AnimatedBackground variant="hero" />
      <PublicContainer size="wide" className="relative grid items-center gap-10 lg:grid-cols-[1.1fr_0.9fr] lg:gap-12">
        <div className="pub-fade-up text-center lg:text-start">
          <PremiumBadge variant="ai" className="mb-4">
            AI Engineering Knowledge Platform
          </PremiumBadge>
          <h1 id="hero-v2-title" className="text-3xl font-extrabold leading-[1.25] tracking-tight sm:text-4xl lg:text-5xl">
            <GradientText as="span" animated className="block">
              دانش مهندسی،
            </GradientText>
            <span className="mt-1 block text-[color:var(--pub-fg)]">با قدرت هوش مصنوعی</span>
          </h1>
          <p className="mx-auto mt-4 max-w-xl text-[14px] leading-8 text-[color:var(--pub-muted)] sm:text-[15px] lg:mx-0">
            مقالات، ابزارها و مسیرهای یادگیری برای توسعه‌دهندگان مدرن
          </p>
          <div className="mt-7 flex flex-wrap items-center justify-center gap-3 lg:justify-start">
            <GlowButton href="/learning">شروع یادگیری</GlowButton>
            <GlowButton
              variant="secondary"
              onClick={() => setPaletteOpen(true)}
              aria-label="جستجوی دانش"
            >
              جستجوی دانش
            </GlowButton>
          </div>
        </div>

        <div className="pub-fade-up pub-fade-up-d2 hidden sm:block">
          <KnowledgeGraphVisual />
        </div>
      </PublicContainer>

      <PublicContainer size="wide" className="relative mt-10 pub-fade-up pub-fade-up-d3">
        <AICommandBox
          onOpenPalette={() => setPaletteOpen(true)}
          onSubmit={(q) => router.push(`/search?q=${encodeURIComponent(q)}`)}
          placeholder="چگونه یک معماری Microservice طراحی کنم؟"
        />
      </PublicContainer>

      <PublicContainer size="wide" className="relative mt-8 pub-fade-up pub-fade-up-d4">
        <FeatureGrid
          items={[
            {
              title: "یادگیری",
              description: "مهندسی نرم‌افزار با مسیر ساخت‌یافته",
              href: "/learning",
              accent: "primary",
            },
            {
              title: "ابزارها",
              description: "کشف ابزارهای توسعه‌دهنده",
              href: "/toolbox",
              accent: "cyan",
            },
            {
              title: "نقشه راه",
              description: "مسیرهای مهندسی مرحله‌به‌مرحله",
              href: "/roadmap",
              accent: "primary",
            },
            {
              title: "دستیار AI",
              description: "پرسش از دانش فنی پلتفرم",
              href: "/learning/assistant",
              accent: "ai",
            },
          ]}
        />
      </PublicContainer>

      <GlobalSearchPalette open={paletteOpen} onOpenChange={setPaletteOpen} />
    </PublicSection>
  );
}
