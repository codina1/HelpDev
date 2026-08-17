"use client";

import { useEffect, useState } from "react";
import { PromptLabCategories } from "@/components/public/prompt-lab/prompt-lab-categories";
import { PromptLabHero } from "@/components/public/prompt-lab/prompt-lab-hero";
import { PromptLabPromptsSection } from "@/components/public/prompt-lab/prompt-lab-prompts-section";
import { ApiClientError } from "@/lib/api/errors";
import {
  EMPTY_PROMPT_LAB_CATALOG_PAGE,
  fetchPromptLabCatalog,
  PROMPT_LAB_PAGE_SIZE,
  PROMPT_LAB_TEASER_SIZE,
  type PromptLabCatalogPage,
} from "@/lib/public/prompt-lab-catalog";
import styles from "./public-prompt-lab-page.module.css";

function scrollToId(id: string) {
  document.getElementById(id)?.scrollIntoView({ behavior: "smooth", block: "start" });
}

function isCatalogAbort(error: unknown): boolean {
  if (error instanceof ApiClientError && error.code === "request_aborted") return true;
  if (error instanceof DOMException) return error.name === "AbortError";
  return error instanceof Error && error.name === "AbortError";
}

/**
 * Public Prompt Lab homepage — GET /api/v1/prompts.
 */
export function PublicPromptLabPage() {
  const [query, setQuery] = useState("");
  const [search, setSearch] = useState("");
  const [categorySlug, setCategorySlug] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [featured, setFeatured] = useState<PromptLabCatalogPage>(EMPTY_PROMPT_LAB_CATALOG_PAGE);
  const [popular, setPopular] = useState<PromptLabCatalogPage>(EMPTY_PROMPT_LAB_CATALOG_PAGE);
  const [latest, setLatest] = useState<PromptLabCatalogPage>(EMPTY_PROMPT_LAB_CATALOG_PAGE);
  const [status, setStatus] = useState<"loading" | "error" | "ready">("loading");
  const [reloadKey, setReloadKey] = useState(0);

  useEffect(() => {
    const next = query.trim();
    const timer = window.setTimeout(() => {
      setSearch((current) => {
        if (current !== next) {
          setPage(1);
        }
        return next;
      });
    }, 300);
    return () => window.clearTimeout(timer);
  }, [query]);

  useEffect(() => {
    const controller = new AbortController();
    setStatus("loading");

    Promise.all([
      fetchPromptLabCatalog({
        search,
        category: categorySlug,
        page: 1,
        pageSize: PROMPT_LAB_TEASER_SIZE,
        signal: controller.signal,
      }),
      fetchPromptLabCatalog({
        search,
        category: categorySlug,
        page: 1,
        pageSize: PROMPT_LAB_TEASER_SIZE,
        popular: true,
        signal: controller.signal,
      }),
      fetchPromptLabCatalog({
        search,
        category: categorySlug,
        page,
        pageSize: PROMPT_LAB_PAGE_SIZE,
        signal: controller.signal,
      }),
    ])
      .then(([featuredPage, popularPage, latestPage]) => {
        setFeatured(featuredPage);
        setPopular(popularPage);
        setLatest(latestPage);
        setStatus("ready");
      })
      .catch((error: unknown) => {
        if (isCatalogAbort(error)) return;
        setStatus("error");
      });

    return () => controller.abort();
  }, [search, categorySlug, page, reloadKey]);

  function retry() {
    setReloadKey((value) => value + 1);
  }

  return (
    <div className={styles.page}>
      <PromptLabHero
        query={query}
        onQueryChange={setQuery}
        onSearch={() => scrollToId("prompt-lab-featured")}
        onExplore={() => scrollToId("prompt-lab-featured")}
      />
      <PromptLabCategories
        selectedSlug={categorySlug}
        onSelect={(slug) => {
          setCategorySlug(slug);
          setPage(1);
        }}
      />
      <PromptLabPromptsSection
        id="prompt-lab-featured"
        headingId="prompt-lab-featured-heading"
        title="پرامپت‌های منتخب"
        lede="گزیده‌ای از پرامپت‌های تست‌شده برای ساخت و توسعه با هوش مصنوعی."
        items={featured.items}
        status={status}
        onRetry={retry}
      />
      <PromptLabPromptsSection
        id="prompt-lab-popular"
        headingId="prompt-lab-popular-heading"
        title="پرامپت‌های محبوب"
        lede="پربازدیدترین پرامپت‌ها بر اساس مشاهده و کپی."
        items={popular.items}
        status={status}
        onRetry={retry}
      />
      <PromptLabPromptsSection
        id="prompt-lab-latest"
        headingId="prompt-lab-latest-heading"
        title="تازه‌ترین پرامپت‌ها"
        lede="آخرین پرامپت‌های اضافه‌شده به کتابخانه."
        items={latest.items}
        status={status}
        onRetry={retry}
        page={latest.page}
        pageSize={latest.pageSize}
        total={latest.total}
        onPageChange={setPage}
        skeletonCount={PROMPT_LAB_PAGE_SIZE}
      />
    </div>
  );
}
