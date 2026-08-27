"use client";

import {
  NEWS_CATEGORY_FILTERS,
  type NewsCategoryId,
} from "@/data/news-articles";

type NewsCategoryFilterProps = {
  active: NewsCategoryId;
  onSelect: (value: NewsCategoryId) => void;
};

function CategoryIcon({ name }: { name: string }) {
  const common = "h-[18px] w-[18px] shrink-0";
  switch (name) {
    case "ai":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="3" stroke="currentColor" strokeWidth="1.7" />
          <path
            d="M12 3v2.5M12 18.5V21M3 12h2.5M18.5 12H21M5.6 5.6l1.8 1.8M16.6 16.6l1.8 1.8M18.4 5.6l-1.8 1.8M7.4 16.6l-1.8 1.8"
            stroke="currentColor"
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
    case "code":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.5 7.5 4 12l4.5 4.5M15.5 7.5 20 12l-4.5 4.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 4.5 19 8v8l-7 3.5L5 16V8l7-3.5Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
        </svg>
      );
    case "frontend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="5" width="17" height="12" rx="2" stroke="currentColor" strokeWidth="1.7" />
          <path d="M8 20h8M12 17v3" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="4" y="4" width="16" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="4" y="10" width="16" height="5" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="4" y="16" width="16" height="4" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
    case "devops":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.5 8.5c1.8-2.4 5.2-2.4 7 0 1.4 1.8 1.4 4.2 0 6-1.8 2.4-5.2 2.4-7 0-1.4-1.8-1.4-4.2 0-6Z" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
    case "tools":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M14.5 5.5a3.5 3.5 0 0 0 4 4L15 13l-4-4 3.5-3.5ZM9 15l-4.5 4.5" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "security":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5 19 6.5v5.2c0 4.2-2.8 7.4-7 8.8-4.2-1.4-7-4.6-7-8.8V6.5l7-3Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
        </svg>
      );
    default:
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke="currentColor" strokeWidth="1.7" />
        </svg>
      );
  }
}

export function NewsCategoryFilter({ active, onSelect }: NewsCategoryFilterProps) {
  return (
    <div className="overflow-x-auto [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden" dir="rtl">
      <div className="flex w-max min-w-full flex-nowrap items-center gap-2.5" role="toolbar" aria-label="فیلتر دسته‌بندی اخبار">
        {NEWS_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-11 shrink-0 items-center gap-2 rounded-xl border px-3.5 text-[13px] font-bold transition sm:h-12 sm:px-4",
                isActive
                  ? "border-transparent bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_20px_rgba(124,58,237,0.45)]"
                  : "border-white/[0.1] bg-[#0F172A] text-[#E2E8F0] hover:border-[rgba(168,85,247,0.4)] hover:text-white",
              ].join(" ")}
            >
              <span className={isActive ? "text-white" : "text-[#A78BFA]"}>
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
