"use client";

import type { NewsTag } from "@/types";
import { NEWS_CATEGORY_FILTERS } from "@/data/news-articles";

type NewsCategoryFilterProps = {
  active: "همه" | NewsTag;
  onSelect: (value: "همه" | NewsTag) => void;
};

function CategoryIcon({ name }: { name: string }) {
  const common = "h-[18px] w-[18px] shrink-0";
  switch (name) {
    case "ai":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 3v3M12 18v3M3 12h3M18 12h3M5.6 5.6l2.1 2.1M16.3 16.3l2.1 2.1M18.4 5.6l-2.1 2.1M7.7 16.3l-2.1 2.1"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
          />
          <circle cx="12" cy="12" r="3.2" stroke="currentColor" strokeWidth="1.8" />
        </svg>
      );
    case "code":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M8.5 7.5 4 12l4.5 4.5M15.5 7.5 20 12l-4.5 4.5M13.2 5.5l-2.4 13"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="4" y="4" width="16" height="16" rx="3.5" stroke="currentColor" strokeWidth="1.8" />
          <path d="M8 12h8M12 8v8" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
        </svg>
      );
    case "devops":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M7.5 8.5a4.5 4.5 0 0 1 7.6-2.4 3.8 3.8 0 1 1 .9 7.4H8.2a3.7 3.7 0 0 1-.7-7.3Z"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinejoin="round"
          />
        </svg>
      );
    default:
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M4 7.5h16M4 12h16M4 16.5h10"
            stroke="currentColor"
            strokeWidth="1.8"
            strokeLinecap="round"
          />
        </svg>
      );
  }
}

/** Horizontal category chips under the news hero. */
export function NewsCategoryFilter({ active, onSelect }: NewsCategoryFilterProps) {
  return (
    <div className="overflow-x-auto pb-1 [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
      <div className="flex min-w-max flex-wrap justify-start gap-2 sm:gap-2.5" role="list">
        {NEWS_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              role="listitem"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex items-center gap-2 rounded-xl border px-3.5 py-2 text-[12px] font-bold transition duration-300 sm:px-4 sm:text-[13px]",
                isActive
                  ? "border-[rgba(168,85,247,0.55)] bg-[rgba(124,58,237,0.28)] text-white shadow-[0_0_20px_rgba(124,58,237,0.28)]"
                  : "border-white/[0.1] bg-[#111827]/80 text-[#94A3B8] hover:border-[rgba(168,85,247,0.35)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white",
              ].join(" ")}
            >
              <span className={isActive ? "text-[#E9D5FF]" : "text-[#64748B]"}>
                <CategoryIcon name={item.icon} />
              </span>
              {item.label}
            </button>
          );
        })}
      </div>
    </div>
  );
}
