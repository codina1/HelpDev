"use client";

import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PROMPT_LAB_CATEGORIES, type PromptLabCategory } from "@/lib/public/prompt-lab-mock";
import styles from "./prompt-lab-categories.module.css";

type PromptLabCategoriesProps = {
  categories?: readonly PromptLabCategory[];
  selectedSlug: string | null;
  onSelect: (slug: string | null) => void;
};

/**
 * Horizontal Prompt Lab category navigation — local filter only.
 */
export function PromptLabCategories({
  categories = PROMPT_LAB_CATEGORIES,
  selectedSlug,
  onSelect,
}: PromptLabCategoriesProps) {
  return (
    <nav className={styles.section} aria-labelledby="prompt-lab-categories-heading">
      <PublicContainer size="wide">
        <h2 id="prompt-lab-categories-heading" className={styles.heading}>
          دسته‌بندی پرامپت‌ها
        </h2>
        <ul className={styles.scroller}>
          {categories.map((category) => {
            const active = selectedSlug === category.slug;
            return (
              <li key={category.slug}>
                <button
                  type="button"
                  className={`${styles.chip} ${active ? styles.active : ""} focus-ring`}
                  aria-pressed={active}
                  onClick={() => onSelect(active ? null : category.slug)}
                >
                  <span className={styles.icon} aria-hidden>
                    <CategoryIcon slug={category.slug} />
                  </span>
                  {category.name}
                </button>
              </li>
            );
          })}
        </ul>
      </PublicContainer>
    </nav>
  );
}

function CategoryIcon({ slug }: { slug: string }) {
  const common = {
    width: 15,
    height: 15,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.8,
  } as const;

  if (slug === "image") {
    return (
      <svg {...common}>
        <rect x="3" y="5" width="18" height="14" rx="2" />
        <circle cx="8.5" cy="10" r="1.4" />
        <path d="M21 16l-5.5-5.5L7 19" />
      </svg>
    );
  }
  if (slug === "video") {
    return (
      <svg {...common}>
        <rect x="3" y="6" width="13" height="12" rx="2" />
        <path d="M16 10l5-3v10l-5-3z" />
      </svg>
    );
  }
  if (slug === "coding") {
    return (
      <svg {...common}>
        <path d="M8 8l-4 4 4 4M16 8l4 4-4 4M13 6l-2 12" />
      </svg>
    );
  }
  if (slug === "writing") {
    return (
      <svg {...common}>
        <path d="M4 20h16M7 16l9.5-9.5a2 2 0 0 1 2.8 2.8L9.8 18.8 6 20l1-3.8z" />
      </svg>
    );
  }
  if (slug === "design") {
    return (
      <svg {...common}>
        <circle cx="12" cy="12" r="8" />
        <path d="M12 4v16M4 12h16" />
      </svg>
    );
  }
  if (slug === "marketing") {
    return (
      <svg {...common}>
        <path d="M4 10v4h4l5 4V6L8 10H4zM16 9a3 3 0 0 1 0 6" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <path d="M4 19V6l8-3 8 3v13" />
      <path d="M8 19v-7h8v7" />
    </svg>
  );
}
