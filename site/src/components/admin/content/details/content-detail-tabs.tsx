"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

export type ContentDetailTab =
  | "overview"
  | "editor"
  | "seo"
  | "history"
  | "workflow"
  | "analytics"
  | "ai";

/**
 * Tab navigation for a content item. Overview is the details page; Editor and
 * SEO both open the Content Studio (the SEO workspace lives inside the editor).
 */
export function ContentDetailTabs({
  id,
  active,
}: {
  id: string;
  active: ContentDetailTab;
}) {
  const base = `${ADMIN_ROUTES.content}/${encodeURIComponent(id)}`;
  const tabs: Array<{ id: ContentDetailTab; label: string; href: string }> = [
    { id: "overview", label: "نمای کلی", href: base },
    { id: "editor", label: "ویرایشگر", href: `${base}/edit` },
    { id: "seo", label: "سئو", href: `${base}/edit` },
    { id: "history", label: "تاریخچه", href: `${base}/history` },
    { id: "workflow", label: "گردش کار", href: `${base}/workflow` },
    { id: "analytics", label: "تحلیل", href: `${base}/analytics` },
    { id: "ai", label: "دستیار AI", href: `${base}/ai` },
  ];

  return (
    <nav
      aria-label="بخش‌های محتوا"
      className="flex flex-wrap items-center gap-1 border-b border-[var(--adm-border)]"
    >
      {tabs.map((tab) => {
        const isActive = tab.id === active;
        return (
          <Link
            key={tab.id}
            href={tab.href}
            aria-current={isActive ? "page" : undefined}
            className={`adm-focus -mb-px border-b-2 px-3 py-2 text-[13px] font-semibold ${
              isActive
                ? "border-[var(--adm-accent)] text-[var(--adm-text)]"
                : "border-transparent text-[var(--adm-text-muted)] hover:text-[var(--adm-text)]"
            }`}
          >
            {tab.label}
          </Link>
        );
      })}
    </nav>
  );
}
