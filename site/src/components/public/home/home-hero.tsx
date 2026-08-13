"use client";

import Link from "next/link";
import { Button } from "@/components/ui/ds/button";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HOME_COVERS } from "@/lib/public/home-covers";

const HERO_NODES = [
  { id: "articles", label: "Articles", href: "/articles", x: 26, y: 22 },
  { id: "tools", label: "Tools", href: "/toolbox", x: 78, y: 24 },
  { id: "learning", label: "Learning", href: "/learning", x: 22, y: 76 },
  { id: "roadmaps", label: "Roadmaps", href: "/roadmap", x: 80, y: 74 },
] as const;

const HERO_PARTICLES = [
  { x: 12, y: 38, d: "0s" },
  { x: 88, y: 42, d: "0.6s" },
  { x: 36, y: 8, d: "1.1s" },
  { x: 64, y: 10, d: "1.7s" },
  { x: 8, y: 62, d: "2.2s" },
  { x: 92, y: 66, d: "0.3s" },
  { x: 48, y: 90, d: "1.4s" },
  { x: 70, y: 86, d: "2.8s" },
  { x: 18, y: 48, d: "0.9s" },
  { x: 84, y: 54, d: "2s" },
] as const;

const HERO_TRUST = ["دانش منتشرشده", "مسیر یادگیری", "دستیار AI"] as const;

/**
 * Homepage hero only — RTL copy + advanced AI orbital visualization.
 * Does not include stats or sections below.
 */
export function HomeHero() {
  return (
    <PublicSection
      className="home-hero overflow-hidden pb-6 pt-6 sm:pb-8 sm:pt-8 lg:pb-10 lg:pt-10"
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
        <div className="home-hero-copy space-y-6 text-center lg:text-start">
          <p className="home-hero-eyebrow">AI Engineering Platform</p>
          <h1 id="home-hero-title" className="home-hero-title">
            <span className="home-hero-title-accent">از پرسش تا ساخت</span>
            <span className="home-hero-title-plain">با هوش HelpDev</span>
          </h1>
          <p className="home-hero-lead">
            مقالات، ابزارها، مسیر یادگیری و نقشه راه را یک‌جا ببینید تا سریع‌تر
            تصمیم بگیرید و دقیق‌تر بسازید.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-2 sm:gap-3 lg:justify-start">
            <Button href="/learning" size="lg" className="max-[374px]:!px-4 max-[374px]:!text-[13px]">
              شروع یادگیری
            </Button>
            <Button href="/learning/assistant" variant="secondary" size="lg" className="max-[374px]:!px-4 max-[374px]:!text-[13px]">
              از AI بپرس
            </Button>
          </div>
          <ul className="home-hero-trust" aria-label="نشانه‌های پلتفرم">
            {HERO_TRUST.map((item) => (
              <li key={item} className="home-hero-trust-item">
                <TrustCheck />
                {item}
              </li>
            ))}
          </ul>
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
      aria-label="هسته HelpDev AI با گره‌های Articles، Tools، Learning و Roadmaps"
    >
      <img src={HOME_COVERS.hero} alt="" className="home-hero-scene" />
      <div className="home-hero-field" aria-hidden />
      <div className="home-hero-glow" aria-hidden />
      <span className="home-hero-ring home-hero-ring-a" aria-hidden />
      <span className="home-hero-ring home-hero-ring-b" aria-hidden />
      <span className="home-hero-ring home-hero-ring-c" aria-hidden />
      <span className="home-hero-scanner" aria-hidden />

      {HERO_PARTICLES.map((particle) => (
        <span
          key={`${particle.x}-${particle.y}`}
          className="home-hero-particle"
          style={{ left: `${particle.x}%`, top: `${particle.y}%`, animationDelay: particle.d }}
          aria-hidden
        />
      ))}

      <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" aria-hidden>
        <defs>
          <linearGradient id="home-hero-edge" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="var(--home-purple)" />
            <stop offset="55%" stopColor="var(--home-blue)" />
            <stop offset="100%" stopColor="var(--home-cyan)" />
          </linearGradient>
          <filter id="home-hero-edge-glow" x="-40%" y="-40%" width="180%" height="180%">
            <feGaussianBlur stdDeviation="1.1" result="blur" />
            <feMerge>
              <feMergeNode in="blur" />
              <feMergeNode in="SourceGraphic" />
            </feMerge>
          </filter>
        </defs>
        {HERO_NODES.map((node) => (
          <line
            key={node.id}
            className="home-hero-edge"
            x1="50"
            y1="50"
            x2={node.x}
            y2={node.y}
            stroke="url(#home-hero-edge)"
            strokeWidth="0.55"
            strokeLinecap="round"
            filter="url(#home-hero-edge-glow)"
          />
        ))}
      </svg>

      <div className="home-hero-core">
        <span className="home-hero-core-pulse" aria-hidden />
        <span className="home-hero-core-orb">AI</span>
        <span className="home-hero-core-label">HelpDev AI</span>
      </div>

      {HERO_NODES.map((node) => (
        <Link
          key={node.id}
          href={node.href}
          className="home-hero-node focus-ring absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center gap-0.5 sm:gap-1"
          style={{ left: `${node.x}%`, top: `${node.y}%` }}
        >
          <span className="home-hero-node-chip">
            <NodeGlyph name={node.id} />
          </span>
          <span className="home-hero-node-label">{node.label}</span>
        </Link>
      ))}
    </div>
  );
}

function TrustCheck() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.2" aria-hidden>
      <path d="M20 6 9 17l-5-5" />
    </svg>
  );
}

function NodeGlyph({ name }: { name: (typeof HERO_NODES)[number]["id"] }) {
  const common = {
    width: 16,
    height: 16,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.7,
  } as const;

  if (name === "articles") {
    return (
      <svg {...common} aria-hidden>
        <path d="M7 3h8l5 5v13H7z" />
        <path d="M15 3v5h5M10 13h7M10 17h5" />
      </svg>
    );
  }
  if (name === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  if (name === "learning") {
    return (
      <svg {...common} aria-hidden>
        <path d="M3 9 12 5l9 4-9 4-9-4Z" />
        <path d="M7 11.5v5.2c0 .6 2.2 2.3 5 2.3s5-1.7 5-2.3v-5.2" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <circle cx="6" cy="6" r="2.1" />
      <circle cx="18" cy="12" r="2.1" />
      <circle cx="8" cy="18" r="2.1" />
      <path d="M8 7.4 16.2 11M16.4 14 9.6 17" />
    </svg>
  );
}
