"use client";

import { motion } from "framer-motion";
import type { ArticleDetailViewModel } from "@/data/article-detail";

type ArticleHeroProps = {
  model: ArticleDetailViewModel;
};

export function ArticleHero({ model }: ArticleHeroProps) {
  return (
    <header className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <span className="inline-flex items-center rounded-lg border border-[#8B5CF6]/4 bg-[#8B5CF6]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#E9D5FF]">
          {model.category}
        </span>
        {model.isFeatured ? (
          <span className="inline-flex items-center gap-1 rounded-lg border border-rose-400/35 bg-rose-500/15 px-2.5 py-1 text-[11.5px] font-bold text-rose-200">
            <span aria-hidden>🔥</span>
            ویژه
          </span>
        ) : (
          <span className="inline-flex items-center rounded-lg border border-[#2563EB]/35 bg-[#2563EB]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#93C5FD]">
            خبر جدید
          </span>
        )}
      </div>

      <h1 className="text-[28px] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[36px] lg:text-[40px] xl:text-[44px] xl:leading-[1.28]">
        {model.title}
      </h1>

      <p className="max-w-3xl text-[14.5px] leading-7 text-[#94A3B8] sm:text-[16px] sm:leading-8">
        {model.description}
      </p>

      <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-[12.5px] font-semibold text-[#94A3B8]">
        <span className="inline-flex items-center gap-2">
          <span className="inline-flex h-8 w-8 items-center justify-center overflow-hidden rounded-full border border-white/[0.1] bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[10px] font-bold text-white">
            {model.author.avatarUrl ? (
              <img src={model.author.avatarUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              model.author.initials
            )}
          </span>
          {model.displayAuthor}
        </span>
        <span className="inline-flex items-center gap-1.5">
          <MetaIcon kind="calendar" />
          {model.publishedAtLabel}
        </span>
        <span className="inline-flex items-center gap-1.5">
          <MetaIcon kind="clock" />
          {model.readingTime}
        </span>
        <span className="inline-flex items-center gap-1.5">
          <MetaIcon kind="eye" />
          {model.viewsLabel} بازدید
        </span>
      </div>

      <motion.div
        initial={{ opacity: 0, y: 10 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35 }}
        className="relative h-[220px] overflow-hidden rounded-[20px] border border-[rgba(139,92,246,0.2)] bg-[#0B1224] shadow-[0_0_48px_rgba(139,92,246,0.18)] sm:h-[280px] lg:h-[320px]"
      >
        {model.coverImage ? (
          <img
            src={model.coverImage}
            alt=""
            className="h-full w-full object-cover"
          />
        ) : (
          <HeroFallbackArt />
        )}
        <span
          className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_70%_40%,rgba(139,92,246,0.28),transparent_55%)]"
          aria-hidden
        />
      </motion.div>
    </header>
  );
}

function MetaIcon({ kind }: { kind: "calendar" | "clock" | "eye" }) {
  if (kind === "calendar") {
    return (
      <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" aria-hidden>
        <rect x="3.5" y="5" width="17" height="15" rx="2" stroke="currentColor" strokeWidth="1.6" />
        <path d="M8 3.5v3M16 3.5v3M3.5 9.5h17" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
      </svg>
    );
  }
  if (kind === "clock") {
    return (
      <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" aria-hidden>
        <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.6" />
        <path d="M12 8v4.5l3 1.8" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
      </svg>
    );
  }
  return (
    <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" stroke="currentColor" strokeWidth="1.6" />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function HeroFallbackArt() {
  return (
    <div className="relative flex h-full w-full items-center justify-center bg-[radial-gradient(circle_at_60%_40%,rgba(37,99,235,0.45),transparent_55%),linear-gradient(145deg,#0B1224,#050816)]">
      <svg viewBox="0 0 420 240" className="h-[78%] w-auto opacity-95" aria-hidden>
        <defs>
          <linearGradient id="artGlow" x1="0" y1="0" x2="1" y2="1">
            <stop offset="0%" stopColor="#8B5CF6" />
            <stop offset="100%" stopColor="#2563EB" />
          </linearGradient>
        </defs>
        <circle cx="210" cy="110" r="42" fill="none" stroke="#61DAFB" strokeWidth="4" opacity="0.9" />
        <ellipse cx="210" cy="110" rx="70" ry="24" fill="none" stroke="#61DAFB" strokeWidth="2.5" opacity="0.55" transform="rotate(60 210 110)" />
        <ellipse cx="210" cy="110" rx="70" ry="24" fill="none" stroke="#61DAFB" strokeWidth="2.5" opacity="0.55" transform="rotate(-60 210 110)" />
        <circle cx="210" cy="110" r="7" fill="#61DAFB" />
        <rect x="48" y="48" width="110" height="72" rx="10" fill="#070B18" stroke="url(#artGlow)" />
        <rect x="60" y="62" width="70" height="5" rx="2.5" fill="#22D3EE" opacity="0.7" />
        <rect x="60" y="74" width="86" height="4" rx="2" fill="#94A3B8" opacity="0.4" />
        <rect x="60" y="86" width="58" height="4" rx="2" fill="#8B5CF6" opacity="0.55" />
        <text x="210" y="200" textAnchor="middle" fill="white" fontSize="16" fontWeight="700" opacity="0.85">
          Better Performance · Server Components
        </text>
      </svg>
    </div>
  );
}
