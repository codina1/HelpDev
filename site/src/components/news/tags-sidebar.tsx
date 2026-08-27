import type { NewsCloudTag } from "@/data/news-articles";
import { NEWS_CLOUD_TAGS } from "@/data/news-articles";

type TagsSidebarProps = {
  activeTag: NewsCloudTag;
  onTagSelect: (tag: NewsCloudTag) => void;
};

/** Rich tag cloud sidebar for secondary filtering. */
export function TagsSidebar({ activeTag, onTagSelect }: TagsSidebarProps) {
  return (
    <section
      className="rounded-[20px] border border-white/[0.08] bg-[#111827]/90 p-5 shadow-[0_8px_28px_rgba(2,6,23,0.28)] backdrop-blur-xl"
      aria-labelledby="news-tags-heading"
    >
      <h2 id="news-tags-heading" className="text-[17px] font-extrabold text-white">
        تگ‌ها
      </h2>
      <div className="mt-5 flex flex-wrap gap-2">
        {NEWS_CLOUD_TAGS.map((tag) => {
          const isActive = activeTag === tag;
          return (
            <button
              key={tag}
              type="button"
              onClick={() => onTagSelect(tag)}
              aria-pressed={isActive}
              className={[
                "rounded-lg border px-3 py-1.5 text-[12px] font-semibold transition duration-300",
                isActive
                  ? "border-[rgba(168,85,247,0.5)] bg-[rgba(124,58,237,0.22)] text-[#E9D5FF] shadow-[0_0_18px_rgba(124,58,237,0.18)]"
                  : "border-white/[0.08] bg-white/[0.03] text-[#94A3B8] hover:border-[rgba(168,85,247,0.35)] hover:bg-[rgba(124,58,237,0.12)] hover:text-white",
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
