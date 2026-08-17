"use client";

import { Button } from "@/components/ui/ds/button";
import styles from "./prompt-lab-pagination.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

type PromptLabPaginationProps = {
  page: number;
  pageSize: number;
  total: number;
  onPageChange: (page: number) => void;
};

export function promptLabPageCount(total: number, pageSize: number): number {
  if (pageSize < 1) return 1;
  return Math.max(1, Math.ceil(total / pageSize));
}

export function PromptLabPagination({
  page,
  pageSize,
  total,
  onPageChange,
}: PromptLabPaginationProps) {
  const totalPages = promptLabPageCount(total, pageSize);
  if (total <= pageSize) return null;

  return (
    <nav className={styles.nav} aria-label="صفحه‌بندی پرامپت‌ها">
      <Button
        type="button"
        size="sm"
        variant="secondary"
        disabled={page <= 1}
        onClick={() => onPageChange(Math.max(1, page - 1))}
        aria-label="صفحه قبل"
      >
        قبلی
      </Button>
      <span className={styles.meta} aria-live="polite">
        {NUMBER_FA.format(page)} / {NUMBER_FA.format(totalPages)}
      </span>
      <Button
        type="button"
        size="sm"
        variant="secondary"
        disabled={page >= totalPages}
        onClick={() => onPageChange(Math.min(totalPages, page + 1))}
        aria-label="صفحه بعد"
      >
        بعدی
      </Button>
    </nav>
  );
}
