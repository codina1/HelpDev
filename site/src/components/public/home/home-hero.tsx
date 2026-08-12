"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Badge } from "@/components/ui/public/badge";
import { Container } from "@/components/ui/public/container";
import { GradientHeading } from "@/components/ui/public/gradient-heading";
import { SearchBox } from "@/components/ui/public/search-box";
import { Section } from "@/components/ui/public/section";

const QUICK_LINKS = [
  { href: "/articles", label: "مقالات" },
  { href: "/toolbox", label: "ابزارها" },
  { href: "/roadmap", label: "نقشه راه" },
  { href: "/learning/assistant", label: "دستیار AI" },
] as const;

export function HomeHero() {
  const router = useRouter();
  const [query, setQuery] = useState("");

  return (
    <Section
      className="relative overflow-hidden pb-6 pt-12 sm:pb-8 sm:pt-16 lg:pt-20"
      aria-labelledby="home-hero-title"
    >
      <div
        className="pointer-events-none absolute inset-0 -z-10 bg-[radial-gradient(ellipse_70%_50%_at_50%_-10%,color-mix(in_srgb,var(--accent)_22%,transparent),transparent_60%)]"
        aria-hidden
      />
      <div className="mx-auto max-w-3xl text-center">
        <Badge variant="ai" className="mb-4">
          AI Engineering Knowledge Platform
        </Badge>
        <GradientHeading
          as="h1"
          id="home-hero-title"
          tone="hero"
          subtitle="مرجع مهندسی نرم‌افزار و هوش مصنوعی — مقالات عمیق، ابزارهای کاربردی، نقشه راه یادگیری و دستیار هوشمند."
        >
          دانش مهندسی، با سرعت AI
        </GradientHeading>

        <div className="mx-auto mt-8 max-w-xl">
          <SearchBox
            size="lg"
            value={query}
            onChange={setQuery}
            onSubmit={(q) => {
              if (q) router.push(`/search?q=${encodeURIComponent(q)}`);
              else router.push("/search");
            }}
            placeholder="چه می‌خواهید یاد بگیرید؟"
            shortcutHint="Ctrl K"
            aria-label="جستجوی هوشمند در خانه"
          />
        </div>

        <div className="mt-6 flex flex-wrap items-center justify-center gap-2">
          {QUICK_LINKS.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="focus-ring rounded-full border border-[color:var(--border-strong)] bg-[color:var(--surface)] px-3.5 py-1.5 text-[12px] font-semibold text-[color:var(--muted)] transition hover:border-[color:color-mix(in_srgb,var(--accent)_40%,transparent)] hover:text-[color:var(--foreground)]"
            >
              {link.label}
            </Link>
          ))}
        </div>
      </div>
    </Section>
  );
}

/** Compact AI search strip under hero — opens /search or command palette via Ctrl+K hint. */
export function HomeAiSearch() {
  const router = useRouter();
  const [query, setQuery] = useState("");

  return (
    <Section className="py-4 sm:py-6" aria-labelledby="home-ai-search-title" bare>
      <Container>
        <div className="rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)]/90 p-4 sm:p-5">
          <div className="mb-3 flex flex-wrap items-end justify-between gap-2">
            <div>
              <h2 id="home-ai-search-title" className="text-base font-bold text-[color:var(--foreground)]">
                جستجوی هوشمند
              </h2>
              <p className="mt-1 text-[13px] text-[color:var(--muted)]">
                مقالات، ابزارها، نقشه راه و دوره‌ها — با Search API
              </p>
            </div>
            <Badge variant="accent">Semantic ready</Badge>
          </div>
          <SearchBox
            value={query}
            onChange={setQuery}
            onSubmit={(q) => {
              router.push(q ? `/search?q=${encodeURIComponent(q)}` : "/search");
            }}
            placeholder="مثلاً: RAG، ASP.NET، roadmap فرانت‌اند..."
            aria-label="جستجوی هوشمند"
          />
        </div>
      </Container>
    </Section>
  );
}
