"use client";

import { useState } from "react";
import type { ArticleDetailViewModel } from "@/data/article-detail";

type ArticleHeroProps = {
  model: ArticleDetailViewModel;
};

/** News/article hero: blurred cover backdrop + metadata, then 16:9 cover. */
export function ArticleHero({ model }: ArticleHeroProps) {
  const [coverState, setCoverState] = useState<"loading" | "ready" | "error">(
    model.coverImage ? "loading" : "error",
  );
  const isNews = model.hub === "news";

  return (
    <header className="space-y-4">
      <div
        className="relative overflow-hidden rounded-[18px] border border-[rgba(139,92,246,0.22)] bg-[#0B1224] shadow-[0_0_40px_rgba(139,92,246,0.16)] md:min-h-[200px]"
      >
        {model.coverImage ? (
          <img
            src={model.coverImage}
            alt=""
            aria-hidden
            className="pointer-events-none absolute inset-0 h-full w-full scale-110 object-cover opacity-40 blur-2xl"
          />
        ) : null}
        <div
          className="pointer-events-none absolute inset-0 bg-[linear-gradient(100deg,rgba(5,8,22,0.92)_18%,rgba(5,8,22,0.72)_55%,rgba(88,28,135,0.35))]"
          aria-hidden
        />

        <div className="relative flex flex-col justify-center gap-3 px-5 py-5 sm:px-7 sm:py-6 md:min-h-[200px] md:py-7">
          <div className="flex flex-wrap items-center gap-2">
            <span className="inline-flex items-center rounded-lg border border-[#8B5CF6]/4 bg-[#8B5CF6]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#E9D5FF]">
              {model.category}
            </span>
            {model.isHot ? (
              <span className="inline-flex items-center gap-1 rounded-lg border border-rose-400/35 bg-rose-500/15 px-2.5 py-1 text-[11.5px] font-bold text-rose-200">
                <span aria-hidden>🔥</span>
                داغ
              </span>
            ) : isNews ? (
              <span className="inline-flex items-center rounded-lg border border-[#2563EB]/35 bg-[#2563EB]/15 px-2.5 py-1 text-[11.5px] font-bold text-[#93C5FD]">
                خبر جدید
              </span>
            ) : model.isFeatured ? (
              <span className="inline-flex items-center gap-1 rounded-lg border border-rose-400/35 bg-rose-500/15 px-2.5 py-1 text-[11.5px] font-bold text-rose-200">
                <span aria-hidden>🔥</span>
                ویژه
              </span>
            ) : null}
          </div>

          <h1 className="line-clamp-2 max-w-4xl text-[24px] font-extrabold leading-[1.35] tracking-tight text-white sm:text-[30px] lg:text-[34px]">
            {model.title}
          </h1>

          <p className="line-clamp-2 max-w-3xl text-[13.5px] leading-7 text-[#CBD5E1] sm:text-[14.5px] sm:leading-7">
            {model.description}
          </p>

          <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-2 text-[12.5px] font-semibold text-[#94A3B8]">
            <span className="inline-flex items-center gap-2">
              <span className="inline-flex h-8 w-8 items-center justify-center overflow-hidden rounded-full border border-white/[0.1] bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[10px] font-bold text-white">
                {model.author.avatarUrl ? (
                  <img src={model.author.avatarUrl} alt="" className="h-full w-full object-cover" />
                ) : (
                  model.author.initials
                )}
              </span>
              <span className="text-[#E5E7EB]">{model.displayAuthor}</span>
            </span>
            <span className="inline-flex items-center gap-1.5">
              <MetaIcon kind="calendar" />
              <time dateTime={model.publishedAtLabel}>{model.publishedAtLabel}</time>
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
        </div>
      </div>

      <div className="relative aspect-video overflow-hidden rounded-[18px] border border-[rgba(139,92,246,0.2)] bg-[#0B1224] shadow-[0_0_36px_rgba(139,92,246,0.14)]">
        {coverState === "loading" ? (
          <div className="absolute inset-0 animate-pulse bg-gradient-to-bl from-[#1E1B4B]/60 via-[#0B1224] to-[#050816]" />
        ) : null}
        {model.coverImage && coverState !== "error" ? (
          <img
            src={model.coverImage}
            alt={model.title}
            className={[
              "h-full w-full object-cover transition-opacity duration-300",
              coverState === "ready" ? "opacity-100" : "opacity-0",
            ].join(" ")}
            onLoad={() => setCoverState("ready")}
            onError={() => setCoverState("error")}
          />
        ) : (
          <CoverFallback title={model.title} />
        )}
      </div>
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
      <path
        d="M2.5 12s3.5-6.5 9.5-6.5S21.5 12 21.5 12s-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z"
        stroke="currentColor"
        strokeWidth="1.6"
      />
      <circle cx="12" cy="12" r="2.6" stroke="currentColor" strokeWidth="1.6" />
    </svg>
  );
}

function CoverFallback({ title }: { title: string }) {
  return (
    <div className="relative flex h-full w-full items-center justify-center bg-[radial-gradient(circle_at_60%_40%,rgba(139,92,246,0.35),transparent_55%),linear-gradient(145deg,#0B1224,#050816)]">
      <p className="max-w-[80%] px-4 text-center text-[14px] font-bold text-[#CBD5E1]">{title}</p>
    </div>
  );
}
