import { NEWS_CLOUD_TAGS, type NewsCloudTag } from "@/data/news-articles";

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

export function TagsSidebar({ activeTag, onTagSelect }: TagsSidebarProps) {
  return (
    <section
      className="rounded-[16px] border border-white/[0.08] bg-[#0F172A] p-4 shadow-[0_8px_28px_rgba(2,6,23,0.3)]"
      aria-labelledby="news-tags-heading"
      dir="rtl"
    >
      <h2 id="news-tags-heading" className="flex items-center gap-2 text-[15px] font-extrabold text-white">
        <TagIcon className="h-4 w-4 text-[#A78BFA]" />
        تگ‌ها
      </h2>
      <div className="mt-4 flex flex-wrap gap-2">
        {NEWS_CLOUD_TAGS.map((tag) => {
          const isActive = activeTag === tag;
          return (
            <button
              key={tag}
              type="button"
              onClick={() => onTagSelect(isActive ? "همه" : tag)}
              aria-pressed={isActive}
              className={[
                "rounded-lg border px-2.5 py-1.5 text-[11px] font-semibold transition",
                isActive
                  ? "border-[rgba(168,85,247,0.5)] bg-[rgba(124,58,237,0.22)] text-[#E9D5FF]"
                  : "border-white/[0.08] bg-[#111827] text-[#94A3B8] hover:border-[rgba(168,85,247,0.35)] hover:text-white",
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
