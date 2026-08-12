"use client";

import { useState } from "react";
import { MarkdownPreview } from "@/components/admin/content/shared/markdown-preview";
import { ContentTypeBadge } from "@/components/admin/content/shared/content-type-badge";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import type { ContentFormValues } from "@/lib/admin/content/content-types";

type PreviewMode = "desktop" | "mobile";

/**
 * Live article preview with desktop/mobile framing. Renders the safe Markdown
 * tree (never `dangerouslySetInnerHTML`). Cover image and excerpt are shown when
 * present. `bare` omits the outer surface so it can nest inside a studio panel.
 */
export function ContentPreviewPanel({
  values,
  bare = false,
  defaultMode = "desktop",
}: {
  values: ContentFormValues;
  bare?: boolean;
  defaultMode?: PreviewMode;
}) {
  const [mode, setMode] = useState<PreviewMode>(defaultMode);

  const body = (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <h2 className="adm-text text-[14px] font-bold">پیش‌نمایش</h2>
        <div className="flex items-center gap-2">
          <ContentTypeBadge type={values.type} />
          <ContentStatusBadge status={values.status} />
        </div>
      </div>

      <div
        role="tablist"
        aria-label="حالت پیش‌نمایش"
        className="inline-flex rounded-lg border border-[var(--adm-border)] p-0.5"
      >
        {(["desktop", "mobile"] as const).map((option) => (
          <button
            key={option}
            type="button"
            role="tab"
            aria-selected={mode === option}
            onClick={() => setMode(option)}
            className={`adm-focus rounded-md px-3 py-1 text-[11px] font-semibold ${
              mode === option
                ? "bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]"
                : "adm-muted"
            }`}
          >
            {option === "desktop" ? "دسکتاپ" : "موبایل"}
          </button>
        ))}
      </div>

      <div className={mode === "mobile" ? "mx-auto w-full max-w-[380px]" : "w-full"}>
        <article className="overflow-hidden rounded-xl border border-[var(--adm-border)] bg-[var(--adm-surface)]">
          {values.coverImage.trim() ? (
            <div className="aspect-[16/7] w-full bg-[var(--adm-surface-3)]">
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src={values.coverImage.trim()}
                alt=""
                className="h-full w-full object-cover"
                referrerPolicy="no-referrer"
              />
            </div>
          ) : null}
          <div className="space-y-3 p-4">
            <div className="space-y-1 border-b border-[var(--adm-border)] pb-3">
              <h3 className="adm-text text-lg font-black">
                {values.title.trim() || "عنوان محتوا"}
              </h3>
              {values.slug.trim() ? (
                <p dir="ltr" className="adm-subtle text-start text-[11px]">
                  /{values.slug.trim()}
                </p>
              ) : null}
              {values.excerpt.trim() ? (
                <p className="adm-muted pt-1 text-[13px] leading-6">{values.excerpt.trim()}</p>
              ) : null}
            </div>
            <MarkdownPreview source={values.body} />
          </div>
        </article>
      </div>
    </div>
  );

  if (bare) {
    return body;
  }

  return (
    <section className="adm-surface rounded-xl p-4">{body}</section>
  );
}
