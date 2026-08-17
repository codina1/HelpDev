"use client";

import Link from "next/link";
import {
  ADMIN_PROMPT_REVIEW_TAB_LABELS,
  ADMIN_PROMPT_REVIEW_TABS,
  type AdminPromptReviewQuery,
  type AdminPromptReviewTab,
} from "@/lib/admin/prompt-lab/admin-prompt-review-types";
import { buildAdminPromptReviewHref } from "@/lib/admin/prompt-lab/admin-prompt-review-url-state";

type AdminPromptReviewTabsProps = {
  query: AdminPromptReviewQuery;
};

/** Status tabs for admin prompt review. */
export function AdminPromptReviewTabs({ query }: AdminPromptReviewTabsProps) {
  return (
    <nav
      aria-label="وضعیت بازبینی پرامپت"
      className="flex flex-wrap items-center gap-1 border-b border-[var(--adm-border)]"
    >
      {ADMIN_PROMPT_REVIEW_TABS.map((tab) => {
        const isActive = query.tab === tab;
        return (
          <Link
            key={tab}
            href={buildAdminPromptReviewHref({ ...query, tab, page: 1 })}
            aria-current={isActive ? "page" : undefined}
            className={`adm-focus -mb-px border-b-2 px-3 py-2 text-[13px] font-semibold ${
              isActive
                ? "border-[var(--adm-accent)] text-[var(--adm-text)]"
                : "border-transparent text-[var(--adm-text-muted)] hover:text-[var(--adm-text)]"
            }`}
          >
            {ADMIN_PROMPT_REVIEW_TAB_LABELS[tab as AdminPromptReviewTab]}
          </Link>
        );
      })}
    </nav>
  );
}
