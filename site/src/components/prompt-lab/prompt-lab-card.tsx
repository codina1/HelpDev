"use client";

import { useState } from "react";
import Link from "next/link";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

type PromptLabCardProps = {
  item: PromptLabCardItem;
};

function BookmarkIcon({ className, filled }: { className?: string; filled?: boolean }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill={filled ? "currentColor" : "none"} aria-hidden>
      <path
        d="M7 4.5h10a1 1 0 0 1 1 1V20l-6-3.2L6 20V5.5a1 1 0 0 1 1-1Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
    </svg>
  );
}

function ViewIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" stroke="currentColor" strokeWidth="1.7" />
      <circle cx="12" cy="12" r="2.5" stroke="currentColor" strokeWidth="1.7" />
    </svg>
  );
}

function formatViews(value: number): string {
  if (value >= 1000) {
    const k = value / 1000;
    const rounded = k >= 10 ? Math.round(k) : Math.round(k * 10) / 10;
    return `${NUMBER_FA.format(rounded)}K`;
  }
  return NUMBER_FA.format(value);
}

/** Catalog prompt card — badge · bookmark · neon cover · title · meta. */
export function PromptLabCard({ item }: PromptLabCardProps) {
  const [saved, setSaved] = useState(false);

  return (
    <article
      className="group flex h-full min-h-[260px] min-w-0 flex-col overflow-hidden rounded-[14px] border border-white/[0.08] bg-[#111827] shadow-[0_4px_16px_rgba(2,6,23,0.25)] transition duration-300 hover:-translate-y-1 hover:border-[rgba(168,85,247,0.45)] hover:shadow-[0_0_30px_rgba(124,58,237,0.25)]"
      dir="rtl"
    >
      <div className="flex items-center justify-between gap-2 px-3.5 pt-3.5">
        <span className="inline-flex items-center rounded-md bg-[#7C3AED] px-2 py-[3px] text-[11px] font-bold text-white">
          {item.category}
        </span>
        <button
          type="button"
          onClick={(event) => {
            event.preventDefault();
            event.stopPropagation();
            setSaved((current) => !current);
          }}
          aria-pressed={saved}
          aria-label={saved ? "حذف از ذخیره‌ها" : "افزودن به ذخیره‌ها"}
          className={[
            "inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/[0.08] bg-white/[0.03] transition",
            saved ? "text-[#A855F7]" : "text-[#94A3B8] hover:text-white",
          ].join(" ")}
        >
          <BookmarkIcon className="h-4 w-4" filled={saved} />
        </button>
      </div>

      <Link
        href={publicPromptLabDetailPath(item.slug)}
        className="flex min-w-0 flex-1 flex-col text-inherit no-underline"
      >
        <div className="relative mx-auto flex h-[100px] w-full max-w-[160px] items-center justify-center">
          <span
            className="pointer-events-none absolute inset-2 rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.28),transparent_70%)] blur-md"
            aria-hidden
          />
          <img
            src={item.coverImage}
            alt=""
            width={160}
            height={100}
            loading="lazy"
            decoding="async"
            className="relative h-full w-full object-contain drop-shadow-[0_10px_24px_rgba(99,102,241,0.35)]"
          />
        </div>

        <div className="flex min-w-0 flex-1 flex-col gap-1.5 px-3.5 pb-3.5 pt-1">
          <h3 className="line-clamp-2 text-[15px] font-bold leading-6 text-white">{item.title}</h3>
          <p className="line-clamp-2 text-[12px] leading-[1.8] text-[#94A3B8]">{item.description}</p>

          <div className="mt-auto flex items-center justify-between gap-2 border-t border-white/[0.06] pt-3 text-[11.5px] font-semibold">
            <span className="truncate text-[#A5B4FC]">{item.aiModel}</span>
            <span className="inline-flex shrink-0 items-center gap-1 text-[#64748B]">
              <ViewIcon className="h-3.5 w-3.5 text-[#7C3AED]" />
              <bdi>{formatViews(item.viewCount)}</bdi>
            </span>
          </div>
        </div>
      </Link>
    </article>
  );
}

export function PromptLabCardSkeleton() {
  return (
    <article
      className="flex min-h-[260px] min-w-0 flex-col overflow-hidden rounded-[14px] border border-white/[0.08] bg-[#111827] p-3.5"
      aria-hidden
    >
      <div className="mb-3 h-5 w-16 animate-pulse rounded-md bg-white/[0.06]" />
      <div className="mx-auto mb-3 h-[100px] w-[100px] animate-pulse rounded-full bg-white/[0.05]" />
      <div className="mb-2 h-4 w-[80%] animate-pulse rounded bg-white/[0.06]" />
      <div className="mb-2 h-3 w-full animate-pulse rounded bg-white/[0.05]" />
      <div className="h-3 w-[60%] animate-pulse rounded bg-white/[0.05]" />
    </article>
  );
}
