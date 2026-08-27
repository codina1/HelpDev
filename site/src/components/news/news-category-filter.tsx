"use client";

import {
  NEWS_CATEGORY_FILTERS,
  type NewsCategoryId,
} from "@/data/news-articles";

type NewsCategoryFilterProps = {
  active: NewsCategoryId;
  onSelect: (value: NewsCategoryId) => void;
};

const ICON_ACCENT: Record<string, string> = {
  all: "#C4B5FD",
  ai: "#A78BFA",
  code: "#60A5FA",
  dotnet: "#C084FC",
  frontend: "#22D3EE",
  backend: "#A78BFA",
  devops: "#38BDF8",
  tools: "#C084FC",
  security: "#67E8F9",
};

function CategoryIcon({ name, active }: { name: string; active: boolean }) {
  const stroke = active ? "#FFFFFF" : ICON_ACCENT[name] ?? "#A78BFA";
  const common = "h-[15px] w-[15px] shrink-0 sm:h-4 sm:w-4";

  switch (name) {
    case "ai":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 4.5c.9 0 1.7.5 2.1 1.3l.3.6h.8A3.3 3.3 0 0 1 18.5 9.7c0 .5-.1 1-.3 1.4.8.7 1.3 1.7 1.3 2.8A4 4 0 0 1 15.5 18h-1.1c-.5 1.3-1.7 2.2-3.1 2.2s-2.6-.9-3.1-2.2H7.1A4 4 0 0 1 3.2 13.9c0-1.1.5-2.1 1.3-2.8A3.3 3.3 0 0 1 4.2 9.7 3.3 3.3 0 0 1 7.5 6.4h.8l.3-.6A2.4 2.4 0 0 1 12 4.5Z"
            stroke={stroke}
            strokeWidth="1.6"
            strokeLinejoin="round"
          />
          <path d="M9.2 11.2h5.6M9.2 14.2h5.6" stroke={stroke} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "code":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M9 7.5 4.5 12 9 16.5M15 7.5 19.5 12 15 16.5"
            stroke={stroke}
            strokeWidth="1.8"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "dotnet":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 4.2 19.2 8v8L12 19.8 4.8 16V8L12 4.2Z"
            stroke={stroke}
            strokeWidth="1.6"
            strokeLinejoin="round"
          />
          <circle cx="12" cy="12" r="2.1" fill={stroke} />
        </svg>
      );
    case "frontend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="4.5" width="17" height="12.5" rx="2.2" stroke={stroke} strokeWidth="1.6" />
          <path d="M3.5 8.5h17M8.5 20h7M12 17v3" stroke={stroke} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="4" y="3.8" width="16" height="5" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <rect x="4" y="9.5" width="16" height="5" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <rect x="4" y="15.2" width="16" height="5" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <circle cx="7.4" cy="6.3" r="0.85" fill={stroke} />
          <circle cx="7.4" cy="12" r="0.85" fill={stroke} />
          <circle cx="7.4" cy="17.7" r="0.85" fill={stroke} />
        </svg>
      );
    case "devops":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M8.2 8.2c2-2.6 5.6-2.6 7.6 0 1.5 2 1.5 4.6 0 6.6-2 2.6-5.6 2.6-7.6 0-1.5-2-1.5-4.6 0-6.6Z"
            stroke={stroke}
            strokeWidth="1.7"
            strokeLinejoin="round"
          />
          <path
            d="M15.8 8.2c2-2.6 5.6-2.6 7.6 0 .2.3.4.6.5 1M8.2 15.8c-2 2.6-5.6 2.6-7.6 0A5.5 5.5 0 0 1 .7 14.8"
            stroke={stroke}
            strokeWidth="1.7"
            strokeLinecap="round"
          />
        </svg>
      );
    case "tools":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M14.2 5.2a3.6 3.6 0 0 0 4.2 4.1L14.8 13l-3.8-3.8 3.2-4ZM9.2 14.8 4.6 19.4"
            stroke={stroke}
            strokeWidth="1.7"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M8.2 9.5 4.8 6.1a2.3 2.3 0 0 1 3.3-3.2L11.5 6.3"
            stroke={stroke}
            strokeWidth="1.7"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
        </svg>
      );
    case "security":
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M12 3.2 19.2 6.2v5.4c0 4.4-3 7.7-7.2 9.2-4.2-1.5-7.2-4.8-7.2-9.2V6.2L12 3.2Z"
            stroke={stroke}
            strokeWidth="1.6"
            strokeLinejoin="round"
          />
          <path d="M9.8 12.1 11.4 13.7 14.6 10.2" stroke={stroke} strokeWidth="1.7" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    default:
      return (
        <svg className={common} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.6" stroke={stroke} strokeWidth="1.6" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.6" stroke={stroke} strokeWidth="1.6" />
        </svg>
      );
  }
}

/** Category pills — matches reference: rounded-full, colored icons, purple active. */
export function NewsCategoryFilter({ active, onSelect }: NewsCategoryFilterProps) {
  return (
    <div
      className="overflow-x-auto [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="rtl"
    >
      <div
        className="flex w-max min-w-full flex-nowrap items-center justify-start gap-2 sm:gap-2.5"
        role="toolbar"
        aria-label="فیلتر دسته‌بندی اخبار"
      >
        {NEWS_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-10 shrink-0 items-center gap-2 rounded-full border px-3.5 text-[12px] font-semibold transition duration-200 sm:h-11 sm:px-4 sm:text-[13px]",
                isActive
                  ? "border-transparent bg-[#7C3AED] text-white shadow-[0_0_0_1px_rgba(168,85,247,0.35),0_0_22px_rgba(124,58,237,0.55)]"
                  : "border-white/[0.12] bg-[#0B1224]/95 text-[#E2E8F0] hover:border-[rgba(168,85,247,0.45)] hover:bg-[rgba(124,58,237,0.1)] hover:text-white",
              ].join(" ")}
            >
              <CategoryIcon name={item.icon} active={isActive} />
              <span>{item.label}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
