"use client";

import { useState } from "react";
import type { NewsItem } from "@/types";

type NewsCardProps = {
  item: NewsItem;
};

export function NewsCard({ item }: NewsCardProps) {
  const [saved, setSaved] = useState(false);

  return (
    <article className="ui-card p-5 sm:p-6">
      <div className="flex items-start justify-between gap-5">
        <div className="min-w-0 flex-1">
          <div className="mb-3 flex flex-wrap items-center gap-2.5">
            <span className="ui-badge">{item.tag}</span>
            <time className="ui-meta">{item.time}</time>
          </div>

          <h2 className="ui-heading text-base">{item.title}</h2>
          <p className="ui-body mt-2">{item.description}</p>
        </div>

        <button
          type="button"
          onClick={() => setSaved((value) => !value)}
          aria-label={saved ? "حذف از ذخیره‌ها" : "ذخیره مطلب"}
          aria-pressed={saved}
          className={[
            "ui-btn shrink-0 px-3 py-2",
            saved ? "ui-btn-active" : "ui-btn-secondary",
          ].join(" ")}
        >
          <span className="inline-flex items-center gap-1.5">
            <BookmarkIcon filled={saved} />
            {saved ? "ذخیره شد" : "ذخیره"}
          </span>
        </button>
      </div>
    </article>
  );
}

function BookmarkIcon({ filled }: { filled: boolean }) {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 24 24"
      fill={filled ? "currentColor" : "none"}
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <path d="M19 21l-7-5-7 5V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2z" />
    </svg>
  );
}
