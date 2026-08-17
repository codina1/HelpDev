"use client";

import { useMemo, useState } from "react";
import { PromptLabCategories } from "@/components/public/prompt-lab/prompt-lab-categories";
import { PromptLabHero } from "@/components/public/prompt-lab/prompt-lab-hero";
import { PromptLabPromptsSection } from "@/components/public/prompt-lab/prompt-lab-prompts-section";
import {
  PROMPT_LAB_PROMPTS,
  featuredPromptLabPrompts,
  latestPromptLabPrompts,
  popularPromptLabPrompts,
} from "@/lib/public/prompt-lab-mock";
import styles from "./public-prompt-lab-page.module.css";

function scrollToId(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
}

/**
 * Public Prompt Lab homepage — local mock catalog, no API.
 */
export function PublicPromptLabPage() {
  const [query, setQuery] = useState("");
  const [categorySlug, setCategorySlug] = useState<string | null>(null);

  const featured = useMemo(
    () => featuredPromptLabPrompts(PROMPT_LAB_PROMPTS, query, categorySlug),
    [query, categorySlug],
  );
  const popular = useMemo(
    () => popularPromptLabPrompts(PROMPT_LAB_PROMPTS, query, categorySlug),
    [query, categorySlug],
  );
  const latest = useMemo(
    () => latestPromptLabPrompts(PROMPT_LAB_PROMPTS, query, categorySlug),
    [query, categorySlug],
  );

  return (
    <div className={styles.page}>
      <PromptLabHero
        query={query}
        onQueryChange={setQuery}
        onSearch={() => scrollToId("prompt-lab-featured")}
        onExplore={() => scrollToId("prompt-lab-featured")}
      />
      <PromptLabCategories selectedSlug={categorySlug} onSelect={setCategorySlug} />
      <PromptLabPromptsSection
        id="prompt-lab-featured"
        headingId="prompt-lab-featured-heading"
        title="پرامپت‌های منتخب"
        lede="گزیده‌ای از پرامپت‌های تست‌شده برای ساخت و توسعه با هوش مصنوعی."
        items={featured}
      />
      <PromptLabPromptsSection
        id="prompt-lab-popular"
        headingId="prompt-lab-popular-heading"
        title="پرامپت‌های محبوب"
        lede="پربازدیدترین پرامپت‌ها بر اساس مشاهده و کپی."
        items={popular}
      />
      <PromptLabPromptsSection
        id="prompt-lab-latest"
        headingId="prompt-lab-latest-heading"
        title="تازه‌ترین پرامپت‌ها"
        lede="آخرین پرامپت‌های اضافه‌شده به کتابخانه."
        items={latest}
      />
    </div>
  );
}
