"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import type { PromptDetailViewModel } from "@/data/prompt-detail";
import { formatCompactCount } from "@/data/prompt-detail";

type PromptHeroProps = {
  model: PromptDetailViewModel;
};

function PromptHeroArt() {
  return (
    <svg
      aria-hidden
      viewBox="0 0 320 180"
      className="pointer-events-none absolute inset-0 h-full w-full opacity-80"
    >
      <defs>
        <linearGradient id="promptHeroGlow" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#8B5CF6" stopOpacity="0.55" />
          <stop offset="100%" stopColor="#2563EB" stopOpacity="0.25" />
        </linearGradient>
      </defs>
      <rect x="28" y="34" width="118" height="78" rx="10" fill="#070B18" stroke="url(#promptHeroGlow)" />
      <rect x="38" y="48" width="72" height="6" rx="3" fill="#22D3EE" opacity="0.7" />
      <rect x="38" y="62" width="90" height="5" rx="2.5" fill="#94A3B8" opacity="0.45" />
      <rect x="38" y="74" width="64" height="5" rx="2.5" fill="#94A3B8" opacity="0.35" />
      <rect x="38" y="86" width="80" height="5" rx="2.5" fill="#8B5CF6" opacity="0.55" />
      <rect x="170" y="52" width="110" height="86" rx="12" fill="#0B1224" stroke="#8B5CF6" strokeOpacity="0.45" />
      <circle cx="226" cy="92" r="22" fill="none" stroke="#61DAFB" strokeWidth="3" opacity="0.85" />
      <ellipse cx="226" cy="92" rx="34" ry="12" fill="none" stroke="#61DAFB" strokeWidth="2" opacity="0.55" transform="rotate(60 226 92)" />
      <ellipse cx="226" cy="92" rx="34" ry="12" fill="none" stroke="#61DAFB" strokeWidth="2" opacity="0.55" transform="rotate(-60 226 92)" />
      <circle cx="226" cy="92" r="4" fill="#61DAFB" />
    </svg>
  );
}

export function PromptHero({ model }: PromptHeroProps) {
  const { detail, aiModels, rating, ratingCount } = model;
  const [copied, setCopied] = useState(false);
  const [bookmarked, setBookmarked] = useState(false);

  async function copyPrompt() {
    try {
      await navigator.clipboard.writeText(detail.content);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1800);
    } catch {
      setCopied(false);
    }
  }

  return (
    <section className="relative overflow-hidden rounded-[20px] border border-white/[0.08] bg-[#0B1224] shadow-[0_0_48px_rgba(139,92,246,0.18)]">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_80%_30%,rgba(139,92,246,0.28),transparent_55%)]"
        aria-hidden
      />
      <div
        dir="ltr"
        className="relative grid min-h-[220px] grid-cols-1 items-center gap-4 px-5 py-5 sm:px-7 md:grid-cols-2 md:gap-6 md:py-0 lg:px-8"
      >
        <div className="order-2 min-w-0 md:order-1" dir="rtl">
          <span className="inline-flex items-center rounded-lg border border-[#8B5CF6]/4 bg-[#8B5CF6]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#E9D5FF]">
            {detail.category}
          </span>
          <h1 className="mt-2.5 text-[26px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[32px] lg:text-[36px]">
            {detail.title}
          </h1>
          <p className="mt-2 max-w-xl text-[13.5px] leading-7 text-[#94A3B8] sm:text-[14.5px]">
            {detail.description}
          </p>

          <div className="mt-3.5 flex flex-wrap gap-2">
            {aiModels.map((modelChip) => (
              <span
                key={modelChip.id}
                className={[
                  "inline-flex items-center gap-1.5 rounded-xl border border-white/[0.08] bg-gradient-to-br px-2.5 py-1.5 text-[11.5px] font-bold text-white",
                  modelChip.tone,
                ].join(" ")}
              >
                <span className="h-1.5 w-1.5 rounded-full bg-[#22D3EE]" aria-hidden />
                {modelChip.name}
              </span>
            ))}
          </div>

          <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-[12.5px] font-semibold text-[#94A3B8]">
            <span>{formatCompactCount(detail.viewCount)} بازدید</span>
            <span className="text-[#FBBF24]">
              ★ {rating.toLocaleString("fa-IR")}
              <span className="ms-1 text-[#64748B]">
                ({ratingCount.toLocaleString("fa-IR")} نظر)
              </span>
            </span>
            <span>{formatCompactCount(detail.copyCount)} کپی</span>
          </div>

          <div className="mt-4 flex flex-wrap items-center gap-2.5">
            <motion.button
              type="button"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              onClick={() => void copyPrompt()}
              className="inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-5 text-[14px] font-bold text-white shadow-[0_0_22px_rgba(139,92,246,0.4)]"
            >
              <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" aria-hidden>
                <rect x="8" y="8" width="11" height="11" rx="2" stroke="currentColor" strokeWidth="1.7" />
                <path d="M6 15H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h8a2 2 0 0 1 2 2v1" stroke="currentColor" strokeWidth="1.7" />
              </svg>
              {copied ? "کپی شد" : "کپی پرامپت"}
            </motion.button>
            <motion.button
              type="button"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.96 }}
              aria-pressed={bookmarked}
              aria-label="نشان‌گذاری"
              onClick={() => setBookmarked((v) => !v)}
              className={[
                "inline-flex h-11 w-11 items-center justify-center rounded-xl border transition",
                bookmarked
                  ? "border-[#8B5CF6]/5 bg-[#8B5CF6]/20 text-[#E9D5FF]"
                  : "border-white/[0.1] bg-[#070B18] text-[#94A3B8] hover:border-[#8B5CF6]/35 hover:text-white",
              ].join(" ")}
            >
              <svg viewBox="0 0 24 24" className="h-5 w-5" fill={bookmarked ? "currentColor" : "none"} aria-hidden>
                <path
                  d="M7 4.5h10a1 1 0 0 1 1 1V20l-6-3.2L6 20V5.5a1 1 0 0 1 1-1Z"
                  stroke="currentColor"
                  strokeWidth="1.7"
                  strokeLinejoin="round"
                />
              </svg>
            </motion.button>
          </div>
        </div>

        <div className="order-1 flex items-center justify-center md:order-2 md:h-[220px]">
          <div className="relative flex h-[160px] w-full max-w-[420px] items-center justify-center overflow-hidden rounded-2xl border border-white/[0.08] bg-[radial-gradient(circle_at_60%_40%,rgba(37,99,235,0.35),transparent_60%)] md:h-[190px]">
            <PromptHeroArt />
            <img
              src={detail.coverImage || "/courses/course-react.png"}
              alt=""
              className="relative z-[1] h-[72%] w-auto object-contain mix-blend-screen drop-shadow-[0_12px_28px_rgba(139,92,246,0.45)]"
            />
            <span className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_40%_60%,rgba(34,211,238,0.2),transparent_50%)]" />
            <span className="absolute bottom-3 start-3 z-[1] text-[11px] font-bold text-white/85">
              Build Better Components
            </span>
          </div>
        </div>
      </div>

      {copied ? (
        <div
          role="status"
          className="absolute bottom-3 start-1/2 z-10 -translate-x-1/2 rounded-lg border border-[#8B5CF6]/35 bg-[#0B1224] px-3 py-1.5 text-[12px] font-bold text-[#E9D5FF] shadow-lg"
        >
          پرامپت در کلیپ‌بورد کپی شد
        </div>
      ) : null}
    </section>
  );
}
