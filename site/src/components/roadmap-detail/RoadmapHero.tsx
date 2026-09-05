"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import type { RoadmapDetailModel } from "@/data/roadmap-detail";

type RoadmapHeroProps = {
  model: RoadmapDetailModel;
  onStart: () => void;
};

export function RoadmapHero({ model, onStart }: RoadmapHeroProps) {
  const [favorite, setFavorite] = useState(false);

  return (
    <section className="relative overflow-hidden rounded-[22px] border border-[rgba(139,92,246,0.25)] bg-[#0B1224] shadow-[0_0_56px_rgba(139,92,246,0.22)]">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_75%_30%,rgba(139,92,246,0.32),transparent_55%)]"
        aria-hidden
      />
      <div
        dir="ltr"
        className="relative grid grid-cols-1 items-center gap-5 px-5 py-6 sm:px-7 md:grid-cols-2 md:gap-6 lg:px-8 lg:py-7"
      >
        <div className="order-2 min-w-0 md:order-1" dir="rtl">
          <div className="flex flex-wrap gap-2">
            {model.badges.map((badge, index) => (
              <span
                key={badge}
                className={[
                  "inline-flex items-center rounded-lg border px-2.5 py-1 text-[11.5px] font-bold",
                  index === model.badges.length - 1
                    ? "border-[#8B5CF6]/45 bg-[#8B5CF6]/18 text-[#E9D5FF]"
                    : "border-white/[0.1] bg-white/[0.04] text-[#CBD5E1]",
                ].join(" ")}
              >
                {badge}
              </span>
            ))}
          </div>

          <h1 className="mt-3 text-[26px] font-extrabold leading-[1.3] tracking-tight text-white sm:text-[34px] lg:text-[38px]">
            {model.title}
          </h1>
          <p className="mt-2 max-w-xl text-[14px] leading-7 text-[#94A3B8] sm:text-[15.5px] sm:leading-8">
            {model.description}
          </p>

          <div className="mt-3.5 flex flex-wrap gap-x-4 gap-y-1.5 text-[12.5px] font-semibold text-[#94A3B8]">
            <span>سطح: {model.levelLabel}</span>
            <span>{model.durationLabel}</span>
            <span>{model.stepsCount.toLocaleString("fa-IR")} مرحله</span>
            <span>{model.projectsCount.toLocaleString("fa-IR")} پروژه</span>
            <span>{model.resourcesCount.toLocaleString("fa-IR")}+ منبع</span>
          </div>

          <div className="mt-5 flex flex-wrap items-center gap-2.5">
            <motion.button
              type="button"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              onClick={onStart}
              className="inline-flex h-11 items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-5 text-[14px] font-bold text-white shadow-[0_0_22px_rgba(139,92,246,0.45)]"
            >
              شروع یادگیری مسیر
            </motion.button>
            <motion.button
              type="button"
              whileTap={{ scale: 0.97 }}
              aria-pressed={favorite}
              onClick={() => setFavorite((v) => !v)}
              className={[
                "inline-flex h-11 items-center gap-2 rounded-xl border px-4 text-[13px] font-bold transition",
                favorite
                  ? "border-[#8B5CF6]/5 bg-[#8B5CF6]/20 text-[#E9D5FF]"
                  : "border-white/[0.12] bg-[#070B18] text-[#CBD5E1] hover:border-[#8B5CF6]/35 hover:text-white",
              ].join(" ")}
            >
              <span aria-hidden>{favorite ? "♥" : "♡"}</span>
              {favorite ? "در علاقه‌مندی‌ها" : "افزودن به علاقه‌مندی‌ها"}
            </motion.button>
          </div>
        </div>

        <div className="order-1 flex items-center justify-center md:order-2">
          <RoadmapHeroArt />
        </div>
      </div>
    </section>
  );
}

function RoadmapHeroArt() {
  return (
    <div className="relative flex h-[180px] w-full max-w-[420px] items-center justify-center overflow-hidden rounded-2xl border border-white/[0.08] bg-[radial-gradient(circle_at_55%_40%,rgba(139,92,246,0.35),transparent_60%)] sm:h-[220px]">
      <svg viewBox="0 0 420 240" className="h-[90%] w-auto" aria-hidden>
        <defs>
          <linearGradient id="rmPath" x1="0" y1="1" x2="1" y2="0">
            <stop offset="0%" stopColor="#8B5CF6" />
            <stop offset="100%" stopColor="#22D3EE" />
          </linearGradient>
        </defs>
        <path
          d="M40 190 C 100 190, 110 120, 170 120 S 240 190, 300 140 S 360 70, 390 55"
          fill="none"
          stroke="url(#rmPath)"
          strokeWidth="4"
          strokeLinecap="round"
          opacity="0.9"
        />
        {[
          [70, 185, "Build"],
          [170, 120, "Learn"],
          [300, 140, "Practice"],
          [385, 55, "Grow"],
        ].map(([x, y, label], i) => (
          <g key={String(label)}>
            <circle cx={Number(x)} cy={Number(y)} r="8" fill="#8B5CF6" opacity={0.9 - i * 0.1} />
            <circle cx={Number(x)} cy={Number(y)} r="14" fill="none" stroke="#A78BFA" strokeWidth="1.5" opacity="0.45" />
            <text x={Number(x)} y={Number(y) + 28} textAnchor="middle" fill="#CBD5E1" fontSize="11" fontWeight="700">
              {String(label)}
            </text>
          </g>
        ))}
        <circle cx="330" cy="70" r="28" fill="none" stroke="#61DAFB" strokeWidth="3.5" opacity="0.9" />
        <ellipse cx="330" cy="70" rx="44" ry="16" fill="none" stroke="#61DAFB" strokeWidth="2" opacity="0.5" transform="rotate(60 330 70)" />
        <ellipse cx="330" cy="70" rx="44" ry="16" fill="none" stroke="#61DAFB" strokeWidth="2" opacity="0.5" transform="rotate(-60 330 70)" />
        <circle cx="330" cy="70" r="5" fill="#61DAFB" />
        <circle cx="250" cy="40" r="26" fill="#1E1B4B" opacity="0.55" />
        <circle cx="250" cy="40" r="18" fill="#E2E8F0" opacity="0.2" />
      </svg>
      <span className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_70%_30%,rgba(34,211,238,0.18),transparent_50%)]" />
    </div>
  );
}
