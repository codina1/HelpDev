"use client";

import { Button } from "@/components/ui/ds/button";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import { PromptLabCard, PromptLabCardSkeleton } from "./prompt-lab-card";
import { PromptLabPagination } from "./prompt-lab-pagination";
import styles from "./prompt-lab-prompts-section.module.css";

export type PromptLabSectionStatus = "loading" | "error" | "ready";

type PromptLabPromptsSectionProps = {
  id: string;
  headingId: string;
  title: string;
  lede: string;
  items: readonly PromptLabCardItem[];
  status?: PromptLabSectionStatus;
  errorMessage?: string;
  onRetry?: () => void;
  page?: number;
  pageSize?: number;
  total?: number;
  onPageChange?: (page: number) => void;
  skeletonCount?: number;
};

export function PromptLabPromptsSection({
  id,
  headingId,
  title,
  lede,
  items,
  status = "ready",
  errorMessage = "بارگذاری پرامپت‌ها ناموفق بود.",
  onRetry,
  page,
  pageSize,
  total,
  onPageChange,
  skeletonCount = 4,
}: PromptLabPromptsSectionProps) {
  return (
    <section id={id} className={styles.section} aria-labelledby={headingId}>
      <PublicContainer size="wide">
        <div className={styles.head}>
          <div>
            <h2 id={headingId} className={styles.heading}>
              {title}
            </h2>
            <p className={styles.lede}>{lede}</p>
          </div>
        </div>
        {status === "loading" ? (
          <ul className={styles.grid} aria-busy="true" aria-live="polite">
            {Array.from({ length: skeletonCount }, (_, index) => (
              <li key={`skeleton-${index}`}>
                <PromptLabCardSkeleton />
              </li>
            ))}
          </ul>
        ) : null}
        {status === "error" ? (
          <div className={styles.status} role="alert">
            <p className={styles.empty}>{errorMessage}</p>
            {onRetry ? (
              <Button type="button" size="sm" variant="secondary" onClick={onRetry}>
                تلاش مجدد
              </Button>
            ) : null}
          </div>
        ) : null}
        {status === "ready" && items.length === 0 ? (
          <p className={styles.empty} role="status">
            پرامپتی با این فیلتر پیدا نشد.
          </p>
        ) : null}
        {status === "ready" && items.length > 0 ? (
          <>
            <ul className={styles.grid}>
              {items.map((item) => (
                <li key={item.id}>
                  <PromptLabCard item={item} />
                </li>
              ))}
            </ul>
            {page && pageSize && total != null && onPageChange ? (
              <PromptLabPagination
                page={page}
                pageSize={pageSize}
                total={total}
                onPageChange={onPageChange}
              />
            ) : null}
          </>
        ) : null}
      </PublicContainer>
    </section>
  );
}
