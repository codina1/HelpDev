"use client";

import {
  COURSE_CATEGORY_FILTERS,
  type CourseCategoryFilter,
} from "@/data/courses";

type CoursesCategoryBarProps = {
  active: CourseCategoryFilter;
  onSelect: (value: CourseCategoryFilter) => void;
};

const ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  code: "#C4B5FD",
  frontend: "#22D3EE",
  backend: "#818CF8",
  devops: "#38BDF8",
  ai: "#A78BFA",
  tools: "#C084FC",
  database: "#67E8F9",
  mobile: "#A5B4FC",
};

function CategoryIcon({ name, color }: { name: string; color: string }) {
  const s = color;
  const cls = "h-[17px] w-[17px] shrink-0";

  switch (name) {
    case "code":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M9 7.2 4.2 12 9 16.8M15 7.2 19.8 12 15 16.8" stroke={s} strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "frontend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.2" y="4.2" width="17.6" height="12.8" rx="2" stroke={s} strokeWidth="1.55" />
          <path d="M3.2 8.4h17.6M8.5 20h7M12 17v3" stroke={s} strokeWidth="1.55" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.8" y="3.5" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <rect x="3.8" y="9.45" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <rect x="3.8" y="15.4" width="16.4" height="5.1" rx="1.5" stroke={s} strokeWidth="1.55" />
          <circle cx="7.2" cy="6.05" r="0.85" fill={s} />
          <circle cx="7.2" cy="12" r="0.85" fill={s} />
          <circle cx="7.2" cy="17.95" r="0.85" fill={s} />
        </svg>
      );
    case "devops":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.4 8.6c1.8-2.3 5-2.3 6.8 0 1.4 1.8 1.4 4.2 0 6-1.8 2.3-5 2.3-6.8 0-1.4-1.8-1.4-4.2 0-6Z" stroke={s} strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M15.2 8.6c1.5-1.9 4-1.9 5.2.2M8.8 15.4c-1.5 1.9-4 1.9-5.2-.2" stroke={s} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "ai":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M9.5 8.2h5M9.5 15.8h5M8.2 12h7.6" stroke={s} strokeWidth="1.6" strokeLinecap="round" />
          <path d="M12 3.8c1.1 0 2 .6 2.5 1.5A3.3 3.3 0 0 1 18.2 8.5c0 .5-.1 1-.3 1.4.8.7 1.3 1.7 1.3 2.8a4 4 0 0 1-4 4h-.7c-.5 1.2-1.6 2-2.9 2s-2.4-.8-2.9-2h-.7a4 4 0 0 1-4-4c0-1.1.5-2.1 1.3-2.8A3.2 3.2 0 0 1 5.5 8.5 3.3 3.3 0 0 1 9.5 5.3C10 4.4 10.9 3.8 12 3.8Z" stroke={s} strokeWidth="1.55" strokeLinejoin="round" />
        </svg>
      );
    case "tools":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M14.4 4.8a3.7 3.7 0 0 0 4.4 4.3L15 13.1l-4-4 3.4-4.3ZM9.2 14.8 4.5 19.5" stroke={s} strokeWidth="1.65" strokeLinecap="round" strokeLinejoin="round" />
          <path d="M8.3 9.6 4.7 6a2.4 2.4 0 0 1 3.4-3.4l3.6 3.6" stroke={s} strokeWidth="1.65" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "database":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <ellipse cx="12" cy="6.2" rx="7.2" ry="2.9" stroke={s} strokeWidth="1.6" />
          <path d="M4.8 6.2v11.6c0 1.6 3.2 2.9 7.2 2.9s7.2-1.3 7.2-2.9V6.2" stroke={s} strokeWidth="1.6" />
          <path d="M4.8 12c0 1.6 3.2 2.9 7.2 2.9s7.2-1.3 7.2-2.9" stroke={s} strokeWidth="1.6" />
        </svg>
      );
    case "mobile":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="7" y="2.8" width="10" height="18.4" rx="2.4" stroke={s} strokeWidth="1.6" />
          <path d="M10.6 18.4h2.8" stroke={s} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    default:
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke={s} strokeWidth="1.6" />
        </svg>
      );
  }
}

/** Horizontal category strip — label first, icon on the far side (reference). */
export function CoursesCategoryBar({ active, onSelect }: CoursesCategoryBarProps) {
  return (
    <div
      className="overflow-x-auto [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="rtl"
    >
      <div
        className="flex w-max min-w-full flex-nowrap items-center justify-start gap-3"
        role="toolbar"
        aria-label="فیلتر دسته‌بندی دوره‌ها"
      >
        {COURSE_CATEGORY_FILTERS.map((item) => {
          const isActive = active === item.id;
          return (
            <button
              key={item.id}
              type="button"
              aria-pressed={isActive}
              onClick={() => onSelect(item.id)}
              className={[
                "inline-flex h-[46px] shrink-0 items-center gap-2.5 rounded-[14px] border px-5 text-[14px] font-semibold transition duration-200",
                isActive
                  ? "border-transparent bg-gradient-to-r from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_16px_rgba(124,58,237,0.35)]"
                  : "border-white/[0.1] bg-[#0F1626]/90 text-[#E5E7EB] hover:border-[rgba(168,85,247,0.4)] hover:bg-[#151E33] hover:text-white",
              ].join(" ")}
            >
              <span>{item.label}</span>
              <CategoryIcon
                name={item.icon}
                color={isActive ? "#FFFFFF" : ICON_COLOR[item.icon] ?? "#A78BFA"}
              />
            </button>
          );
        })}
      </div>
    </div>
  );
}
