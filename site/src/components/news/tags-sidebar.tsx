import type { NewsCloudTag } from "@/data/news-articles";
import { NEWS_CLOUD_TAGS } from "@/data/news-articles";

type TagsSidebarProps = {
  activeTag: NewsCloudTag;
  onTagSelect: (tag: NewsCloudTag) => void;
};

function TagIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M3.5 12.5V5.8A2.3 2.3 0 0 1 5.8 3.5h6.7c.6 0 1.2.2 1.6.7l6.2 6.2a2.3 2.3 0 0 1 0 3.2l-6.1 6.1a2.3 2.3 0 0 1-3.2 0l-6.2-6.2a2.3 2.3 0 0 1-.7-1.6Z"
        stroke="currentColor"
        strokeWidth="1.7"
        strokeLinejoin="round"
      />
      <circle cx="8.2" cy="8.2" r="1.2" fill="currentColor" />
    </svg>
  );
}

/** Tag cloud sidebar matching reference styling. */
export function TagsSidebar({ activeTag, onTagSelect }: TagsSidebarProps) {
  return (
    <section
      className="rounded-[18px] border border-white/[0.08] bg-[#0F172A]/95 p-4 shadow-[0_8px_28px_rgba(2,6,23,0.32)] backdrop-blur-xl sm:p-5"
      aria-labelledby="news-tags-heading"
      dir="rtl"
    >
      <h2
        id="news-tags-heading"
        className="flex items-center gap-2 text-[16px] font-extrabold text-white sm:text-[17px]"
      >
        <TagIcon className="h-[18px] w-[18px] text-[#A78BFA]" />
        تگ‌ها
      </h2>
      <div className="mt-4 grid grid-cols-2 gap-2">
        {NEWS_CLOUD_TAGS.filter((tag) => tag !== "همه").map((tag) => {
          const isActive = activeTag === tag;
          return (
            <button
              key={tag}
              type="button"
              onClick={() => onTagSelect(isActive ? "همه" : tag)}
              aria-pressed={isActive}
              className={[
                "rounded-xl border px-2.5 py-2 text-[12px] font-semibold transition duration-300",
                isActive
                  ? "border-[rgba(168,85,247,0.5)] bg-[rgba(124,58,237,0.22)] text-[#E9D5FF] shadow-[0_0_18px_rgba(124,58,237,0.18)]"
                  : "border-white/[0.08] bg-[#111827]/70 text-[#94A3B8] hover:border-[rgba(168,85,247,0.35)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white",
              ].join(" ")}
            >
              #{tag}
            </button>
          );
        })}
      </div>
    </section>
  );
}
