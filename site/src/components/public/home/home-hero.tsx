"use client";

import Link from "next/link";
import { Button } from "@/components/ui/ds/button";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

const HERO_NODES = [
  { id: "articles", label: "Articles", href: "/articles", x: 12, y: 18 },
  { id: "tools", label: "Tools", href: "/toolbox", x: 88, y: 22 },
  { id: "learning", label: "Learning", href: "/learning", x: 14, y: 78 },
  { id: "roadmaps", label: "Roadmaps", href: "/roadmap", x: 86, y: 74 },
] as const;

/**
 * Homepage hero only — RTL copy + AI orb with product nodes.
 * Does not include stats or sections below.
 */
export function HomeHero() {
  return (
    <PublicSection
      className="home-hero overflow-hidden pb-10 pt-8 sm:pb-12 sm:pt-12 lg:pb-16 lg:pt-16"
      bare
      aria-labelledby="home-hero-title"
    >
      <div className="pointer-events-none absolute inset-0" aria-hidden>
        <div
          className="absolute -top-24 left-1/2 h-[420px] w-[720px] -translate-x-1/2 rounded-full blur-3xl"
          style={{
            background:
              "radial-gradient(circle, var(--home-bg-atmosphere-purple), transparent 70%)",
          }}
        />
        <div
          className="absolute -end-16 top-24 h-64 w-64 rounded-full blur-3xl"
          style={{ background: "var(--home-bg-atmosphere-cyan)" }}
        />
        <div
          className="absolute bottom-0 start-0 h-48 w-48 rounded-full blur-3xl"
          style={{ background: "var(--home-bg-atmosphere-blue)" }}
        />
      </div>

      <PublicContainer
        size="wide"
        className="relative grid min-w-0 items-center gap-8 sm:gap-10 lg:grid-cols-2 lg:gap-14"
      >
        <div className="space-y-6 text-center lg:text-start">
          <p
            className="text-[12px] font-semibold tracking-wide"
            style={{ color: "var(--home-cyan)" }}
          >
            AI Engineering Platform
          </p>
          <h1
            id="home-hero-title"
            className="font-extrabold tracking-tight text-[color:var(--home-text)]"
            style={{
              fontSize: "clamp(1.65rem, 7vw, var(--home-display-size))",
              lineHeight: "var(--home-display-leading)",
            }}
          >
            <span
              className="block bg-clip-text text-transparent"
              style={{
                backgroundImage:
                  "linear-gradient(135deg, var(--home-text) 10%, var(--home-purple) 55%, var(--home-cyan))",
              }}
            >
              دانش مهندسی،
            </span>
            <span className="mt-1 block">با قدرت هوش مصنوعی</span>
          </h1>
          <p
            className="mx-auto max-w-xl text-[color:var(--home-text-muted)] lg:mx-0"
            style={{
              fontSize: "var(--home-body-size)",
              lineHeight: "var(--home-body-leading)",
            }}
          >
            مقالات، ابزارها، مسیر یادگیری و نقشه راه را در یک پلتفرم دانش مهندسی
            کنار هم ببینید — برای تصمیم‌گیری سریع‌تر و ساخت دقیق‌تر.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-2 sm:gap-3 lg:justify-start">
            <Button href="/learning" size="lg" className="max-[374px]:!px-4 max-[374px]:!text-[13px]">
              شروع یادگیری
            </Button>
            <Button href="/learning/assistant" variant="secondary" size="lg" className="max-[374px]:!px-4 max-[374px]:!text-[13px]">
              از AI بپرس
            </Button>
          </div>
        </div>

        <HeroOrb />
      </PublicContainer>
    </PublicSection>
  );
}

function HeroOrb() {
  return (
    <div
      className="home-hero-orb relative mx-auto aspect-square w-full max-w-[min(100%,18.5rem)] sm:max-w-[420px] lg:max-w-[480px]"
      role="img"
      aria-label="هسته هوش مهندسی با گره‌های Articles، Tools، Learning و Roadmaps"
    >
      <div
        className="home-hero-glow absolute inset-[18%] rounded-full blur-2xl"
        style={{
          background:
            "radial-gradient(circle, var(--home-purple-soft), var(--home-cyan-soft) 55%, transparent 70%)",
          boxShadow: "var(--home-glow-purple)",
        }}
        aria-hidden
      />
      <div
        className="absolute inset-[28%] rounded-full border"
        style={{
          borderColor: "var(--home-border-accent)",
          background:
            "radial-gradient(circle at 35% 30%, color-mix(in srgb, var(--home-purple) 45%, transparent), color-mix(in srgb, var(--home-blue) 28%, #080a12) 45%, #060816 78%)",
          boxShadow: "var(--home-glow-blue), inset 0 0 40px color-mix(in srgb, var(--home-cyan) 18%, transparent)",
        }}
        aria-hidden
      />
      <div
        className="absolute inset-[38%] rounded-full"
        style={{
          background:
            "radial-gradient(circle at 40% 35%, color-mix(in srgb, var(--home-text) 18%, transparent), transparent 55%)",
        }}
        aria-hidden
      />

      <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" aria-hidden>
        <defs>
          <linearGradient id="home-hero-edge" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="var(--home-purple)" />
            <stop offset="100%" stopColor="var(--home-cyan)" />
          </linearGradient>
        </defs>
        {HERO_NODES.map((node) => (
          <line
            key={node.id}
            x1="50"
            y1="50"
            x2={node.x}
            y2={node.y}
            stroke="url(#home-hero-edge)"
            strokeWidth="0.45"
            opacity="0.7"
          />
        ))}
      </svg>

      {HERO_NODES.map((node) => (
        <Link
          key={node.id}
          href={node.href}
          className="home-hero-node focus-ring absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center gap-0.5 sm:gap-1"
          style={{ left: `${node.x}%`, top: `${node.y}%` }}
        >
          <span
            className="home-hero-node-chip flex h-9 w-9 items-center justify-center rounded-2xl border text-[9px] font-extrabold backdrop-blur-md sm:h-12 sm:w-12 sm:text-[11px]"
            style={{
              borderColor: "var(--home-border-strong)",
              background: "var(--home-surface-elevated)",
              color: "var(--home-text)",
              boxShadow: "var(--home-shadow-sm)",
            }}
          >
            {node.label.slice(0, 2)}
          </span>
          <span
            className="rounded-md px-1.5 py-0.5 text-[10px] font-semibold backdrop-blur"
            style={{
              background: "color-mix(in srgb, var(--home-bg) 70%, transparent)",
              color: "var(--home-text-muted)",
            }}
          >
            {node.label}
          </span>
        </Link>
      ))}
    </div>
  );
}
