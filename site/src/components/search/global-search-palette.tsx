"use client";

import { useCallback, useEffect, useId, useMemo, useRef, useState } from "react";
import { useRouter } from "next/navigation";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import { PageErrorState } from "@/components/ui/page-error-state";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";
import { search, searchAsk, type SearchResultItemDto } from "@/lib/api/search";
import { hrefForSearchResult, labelForSearchSource } from "@/lib/public/search-navigation";

type GlobalSearchPaletteProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

type SectionKey = "Knowledge" | "Tools" | "Roadmaps" | "Courses" | "Other";

type FlatRow =
  | { key: string; kind: "ai"; index: number }
  | { key: string; kind: "item"; index: number; item: SearchResultItemDto; section: SectionKey };

function sectionForItem(item: SearchResultItemDto): SectionKey {
  const t = (item.type ?? item.sourceType ?? "").toLowerCase();
  if (t === "tool") return "Tools";
  if (t === "roadmap" || t === "roadmapstep") return "Roadmaps";
  if (t === "course" || t === "lesson") return "Courses";
  if (t === "content" || t === "article" || t === "news" || t === "prompt") return "Knowledge";
  return "Other";
}

const SECTION_ORDER: SectionKey[] = ["Knowledge", "Tools", "Roadmaps", "Courses", "Other"];

export function GlobalSearchPalette({ open, onOpenChange }: GlobalSearchPaletteProps) {
  const router = useRouter();
  const inputRef = useRef<HTMLInputElement>(null);
  const dialogId = useId();
  const [query, setQuery] = useState("");
  const [items, setItems] = useState<SearchResultItemDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [activeIndex, setActiveIndex] = useState(0);
  const [aiAnswer, setAiAnswer] = useState<string | null>(null);
  const [aiLoading, setAiLoading] = useState(false);

  useEffect(() => {
    if (!open) return;
    const handle = window.setTimeout(() => inputRef.current?.focus(), 0);
    return () => window.clearTimeout(handle);
  }, [open]);

  const runSearch = useCallback(async (q: string) => {
    const trimmed = q.trim();
    if (!trimmed) {
      setItems([]);
      setError(null);
      setLoading(false);
      setAiAnswer(null);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const result = await search({ q: trimmed, page: 1, pageSize: 16 });
      setItems(result.items ?? []);
      setActiveIndex(0);
    } catch (err) {
      setError(err);
      setItems([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    if (!open) return;
    const handle = window.setTimeout(() => void runSearch(query), 260);
    return () => window.clearTimeout(handle);
  }, [query, open, runSearch]);

  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") {
        event.preventDefault();
        onOpenChange(false);
      }
    }
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, onOpenChange]);

  const rows: FlatRow[] = useMemo(() => {
    if (!query.trim()) return [];
    const next: FlatRow[] = [{ key: "ai", kind: "ai", index: 0 }];
    let index = 1;
    for (const section of SECTION_ORDER) {
      for (const item of items.filter((i) => sectionForItem(i) === section)) {
        next.push({
          key: `${item.sourceType}-${item.sourceId}`,
          kind: "item",
          index,
          item,
          section,
        });
        index += 1;
      }
    }
    return next;
  }, [query, items]);

  const grouped = useMemo(() => {
    const map = new Map<SectionKey, FlatRow[]>();
    for (const row of rows) {
      if (row.kind !== "item") continue;
      const bucket = map.get(row.section) ?? [];
      bucket.push(row);
      map.set(row.section, bucket);
    }
    return map;
  }, [rows]);

  if (!open) return null;

  function navigateTo(item: SearchResultItemDto) {
    onOpenChange(false);
    router.push(hrefForSearchResult(item));
  }

  async function runAiAnswer() {
    const q = query.trim();
    if (!q) return;
    setAiLoading(true);
    setAiAnswer(null);
    try {
      const result = await searchAsk(q);
      setAiAnswer(result.answer);
    } catch (err) {
      setAiAnswer(err instanceof Error ? err.message : "پاسخ AI دریافت نشد.");
    } finally {
      setAiLoading(false);
    }
  }

  function activate(index: number) {
    const row = rows.find((r) => r.index === index);
    if (!row) return;
    if (row.kind === "ai") void runAiAnswer();
    else navigateTo(row.item);
  }

  const aiRow = rows.find((r) => r.kind === "ai");

  return (
    <div
      className="fixed inset-0 z-[90] flex flex-col bg-[color:color-mix(in_srgb,var(--pub-bg)_92%,transparent)] backdrop-blur-xl"
      role="presentation"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget) onOpenChange(false);
      }}
    >
      <div
        id={dialogId}
        role="dialog"
        aria-modal="true"
        aria-label="فرمان سراسری HelpDev"
        className="mx-auto flex h-full w-full max-w-3xl flex-col px-3 pb-4 pt-[max(1rem,env(safe-area-inset-top))] sm:px-4 sm:pt-10"
        dir="rtl"
        onMouseDown={(e) => e.stopPropagation()}
      >
        <div className="pub-glass-strong flex min-h-0 flex-1 flex-col overflow-hidden rounded-2xl border border-[color:var(--pub-glass-border)] shadow-[0_40px_100px_rgba(0,0,0,0.55)]">
          <div className="border-b border-[color:var(--pub-glass-border)] p-3 sm:p-4">
            <div className="mb-2 flex items-center justify-between gap-2">
              <div className="flex flex-wrap gap-1.5" aria-hidden>
                <PremiumBadge variant="ai">AI</PremiumBadge>
                <PremiumBadge variant="outline">Knowledge</PremiumBadge>
                <PremiumBadge variant="cyan">Tools</PremiumBadge>
                <PremiumBadge variant="success">Roadmaps</PremiumBadge>
              </div>
              <button
                type="button"
                className="focus-ring rounded-lg px-2 py-1 text-[12px] text-[color:var(--pub-muted)] hover:bg-white/5"
                onClick={() => onOpenChange(false)}
              >
                Esc
              </button>
            </div>
            <label className="sr-only" htmlFor={`${dialogId}-input`}>
              جستجو یا پرسش
            </label>
            <input
              id={`${dialogId}-input`}
              ref={inputRef}
              type="search"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="جستجو در دانش، ابزار، نقشه راه یا پرسش AI..."
              className="focus-ring w-full rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg)] px-4 py-3.5 text-[16px] text-[color:var(--pub-fg)] placeholder:text-[color:var(--pub-muted)]"
              aria-controls={`${dialogId}-results`}
              aria-autocomplete="list"
              onKeyDown={(event) => {
                if (event.key === "ArrowDown") {
                  event.preventDefault();
                  setActiveIndex((i) => Math.min(i + 1, Math.max(rows.length - 1, 0)));
                } else if (event.key === "ArrowUp") {
                  event.preventDefault();
                  setActiveIndex((i) => Math.max(i - 1, 0));
                } else if (event.key === "Enter") {
                  event.preventDefault();
                  activate(activeIndex);
                }
              }}
            />
          </div>

          <div id={`${dialogId}-results`} className="min-h-0 flex-1 overflow-y-auto p-2 sm:p-3">
            {loading ? (
              <p className="px-3 py-8 text-center text-sm text-[color:var(--pub-muted)]" role="status">
                در حال جستجو...
              </p>
            ) : error ? (
              <PageErrorState error={error} onRetry={() => void runSearch(query)} />
            ) : !query.trim() ? (
              <PageEmptyState
                title="فرمان را تایپ کنید"
                description="بخش‌ها: Knowledge · Tools · Roadmaps · AI"
                className="border-0 bg-transparent py-12"
              />
            ) : (
              <div className="space-y-4" role="listbox" aria-label="نتایج فرمان">
                {aiRow ? (
                  <section aria-label="AI">
                    <p className="mb-1.5 px-2 text-[11px] font-bold text-[color:var(--pub-ai-from)]">AI</p>
                    <button
                      type="button"
                      role="option"
                      aria-selected={activeIndex === aiRow.index}
                      className={[
                        "focus-ring flex w-full items-center justify-between rounded-xl px-3 py-3 text-start",
                        activeIndex === aiRow.index
                          ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_20%,transparent)]"
                          : "hover:bg-white/[0.04]",
                      ].join(" ")}
                      onMouseEnter={() => setActiveIndex(aiRow.index)}
                      onClick={() => void runAiAnswer()}
                    >
                      <span>
                        <span className="block text-[14px] font-bold text-[color:var(--pub-fg)]">
                          پاسخ AI برای «{query.trim()}»
                        </span>
                        <span className="text-[11px] text-[color:var(--pub-muted)]">
                          {aiLoading ? "در حال تولید..." : "Enter · searchAsk"}
                        </span>
                      </span>
                      <PremiumBadge variant="ai">AI</PremiumBadge>
                    </button>
                    {aiAnswer ? (
                      <div className="mt-2 rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg)]/70 p-3 text-[13px] leading-7 text-[color:var(--pub-fg)]">
                        {aiAnswer}
                      </div>
                    ) : null}
                  </section>
                ) : null}

                {SECTION_ORDER.map((section) => {
                  const sectionRows = grouped.get(section) ?? [];
                  if (sectionRows.length === 0) return null;
                  return (
                    <section key={section} aria-label={section}>
                      <p className="mb-1.5 px-2 text-[11px] font-bold text-[color:var(--pub-secondary)]">
                        {section}
                      </p>
                      <ul className="space-y-1">
                        {sectionRows.map((row) => {
                          if (row.kind !== "item") return null;
                          return (
                            <li key={row.key}>
                              <button
                                type="button"
                                role="option"
                                aria-selected={activeIndex === row.index}
                                className={[
                                  "focus-ring flex w-full flex-col gap-0.5 rounded-xl px-3 py-2.5 text-start",
                                  activeIndex === row.index
                                    ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_18%,transparent)]"
                                    : "hover:bg-white/[0.04]",
                                ].join(" ")}
                                onMouseEnter={() => setActiveIndex(row.index)}
                                onClick={() => navigateTo(row.item)}
                              >
                                <span className="flex items-center justify-between gap-2">
                                  <span className="text-[13px] font-semibold text-[color:var(--pub-fg)]">
                                    {row.item.title}
                                  </span>
                                  <PremiumBadge variant="outline">
                                    {labelForSearchSource(row.item)}
                                  </PremiumBadge>
                                </span>
                                {row.item.summary ? (
                                  <span className="line-clamp-2 text-[12px] text-[color:var(--pub-muted)]">
                                    {row.item.summary}
                                  </span>
                                ) : null}
                              </button>
                            </li>
                          );
                        })}
                      </ul>
                    </section>
                  );
                })}

                {!loading && items.length === 0 ? (
                  <PageEmptyState
                    title="نتیجه ساختاری نیست"
                    description="از بخش AI بالا برای پاسخ grounded استفاده کنید."
                    className="border-0 bg-transparent py-6"
                  />
                ) : null}
              </div>
            )}
          </div>

          <div className="flex items-center justify-between border-t border-[color:var(--pub-glass-border)] px-3 py-2 text-[11px] text-[color:var(--pub-muted)]">
            <span>↑↓ انتخاب · Enter اجرا · Esc بستن</span>
            <button
              type="button"
              className="focus-ring rounded-lg px-2 py-1 hover:bg-white/5"
              onClick={() => {
                onOpenChange(false);
                router.push(`/search?q=${encodeURIComponent(query.trim())}`);
              }}
            >
              صفحه جستجو
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
