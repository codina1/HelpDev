"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import type { ToolDetailModel } from "@/data/tool-detail";

type ToolHeroProps = {
  model: ToolDetailModel;
};

export function ToolHero({ model }: ToolHeroProps) {
  const [bookmarked, setBookmarked] = useState(false);

  return (
    <section className="relative overflow-hidden rounded-[22px] border border-[rgba(139,92,246,0.25)] bg-[#0B1224] shadow-[0_0_56px_rgba(139,92,246,0.2)]">
      <div
        className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_80%_25%,rgba(139,92,246,0.28),transparent_55%)]"
        aria-hidden
      />
      <div
        dir="ltr"
        className="relative grid grid-cols-1 items-center gap-5 px-5 py-6 sm:px-7 md:grid-cols-2 md:gap-6 lg:px-8 lg:py-7"
      >
        <div className="order-2 min-w-0 md:order-1" dir="rtl">
          <span className="inline-flex items-center rounded-lg border border-[#8B5CF6]/4 bg-[#8B5CF6]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#E9D5FF]">
            {model.categoryLabel}
          </span>

          <h1 className="mt-3 flex flex-wrap items-center gap-2 text-[28px] font-extrabold leading-[1.25] tracking-tight text-white sm:text-[36px]">
            {model.name}
            {model.verified ? (
              <span
                className="inline-flex h-6 w-6 items-center justify-center rounded-full bg-[#2563EB] text-[12px] text-white"
                title="تأییدشده"
                aria-label="تأییدشده"
              >
                ✓
              </span>
            ) : null}
          </h1>

          <p className="mt-2 max-w-xl text-[14px] leading-7 text-[#94A3B8] sm:text-[15.5px] sm:leading-8">
            {model.description}
          </p>

          <div className="mt-3 flex flex-wrap gap-2">
            {model.badges.map((badge) => (
              <span
                key={badge}
                className="rounded-lg border border-white/[0.1] bg-white/[0.04] px-2.5 py-1 text-[11.5px] font-bold text-[#CBD5E1]"
              >
                {badge}
              </span>
            ))}
          </div>

          <div className="mt-5 flex flex-wrap items-center gap-2.5">
            <motion.a
              href={model.downloadUrl}
              target="_blank"
              rel="noopener noreferrer"
              whileHover={{ scale: 1.02 }}
              whileTap={{ scale: 0.98 }}
              className="inline-flex h-11 items-center justify-center gap-2 rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] px-5 text-[14px] font-bold text-white no-underline shadow-[0_0_22px_rgba(139,92,246,0.4)]"
            >
              دانلود ابزار
            </motion.a>
            <a
              href={model.websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex h-11 items-center justify-center rounded-xl border border-white/[0.12] bg-[#070B18] px-4 text-[13px] font-bold text-[#E5E7EB] no-underline transition hover:border-[#8B5CF6]/4 hover:text-white"
            >
              بازدید از وب‌سایت
            </a>
            <button
              type="button"
              aria-pressed={bookmarked}
              aria-label="نشان‌گذاری"
              onClick={() => setBookmarked((v) => !v)}
              className={[
                "inline-flex h-11 w-11 items-center justify-center rounded-xl border transition",
                bookmarked
                  ? "border-[#8B5CF6]/5 bg-[#8B5CF6]/20 text-[#E9D5FF]"
                  : "border-white/[0.1] bg-[#070B18] text-[#94A3B8] hover:text-white",
              ].join(" ")}
            >
              {bookmarked ? "♥" : "♡"}
            </button>
          </div>

          <div className="mt-4 flex flex-wrap gap-x-4 gap-y-1.5 text-[12.5px] font-semibold text-[#94A3B8]">
            <span className="text-[#FBBF24]">★ {model.rating.toLocaleString("fa-IR")}</span>
            <span>{model.viewsLabel} بازدید</span>
            <span>{model.downloadsLabel} دانلود</span>
          </div>
        </div>

        <div className="order-1 flex items-center justify-center md:order-2">
          <ToolHeroArt name={model.name} />
        </div>
      </div>
    </section>
  );
}

function ToolHeroArt({ name }: { name: string }) {
  return (
    <div className="relative flex h-[180px] w-full max-w-[420px] items-center justify-center overflow-hidden rounded-2xl border border-white/[0.08] bg-[radial-gradient(circle_at_55%_40%,rgba(37,99,235,0.35),transparent_60%)] sm:h-[220px]">
      <div className="relative z-[1] flex h-28 w-40 items-center justify-center rounded-xl border border-[#8B5CF6]/35 bg-[#070B18] shadow-[0_0_28px_rgba(139,92,246,0.35)] sm:h-32 sm:w-48">
        <span className="text-[13px] font-bold text-white/90">{name}</span>
      </div>
      {["Extensions", "Debugging", "Git", "IntelliSense"].map((label, index) => (
        <span
          key={label}
          className="absolute z-[1] rounded-lg border border-white/[0.1] bg-[#0B1224]/9 px-2 py-1 text-[10px] font-bold text-[#CBD5E1] shadow-[0_0_12px_rgba(139,92,246,0.25)]"
          style={{
            top: `${18 + (index % 2) * 48}%`,
            [index < 2 ? "left" : "right"]: "8%",
          }}
        >
          {label}
        </span>
      ))}
      <span className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_70%_30%,rgba(139,92,246,0.22),transparent_50%)]" />
    </div>
  );
}
