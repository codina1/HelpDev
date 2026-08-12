"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import {
  fetchRelatedKnowledge,
  type SearchContextItemDto,
} from "@/lib/api/search";

type RelatedKnowledgePanelProps = {
  contentId: string;
};

/**
 * Content Studio — Related Knowledge.
 * Suggestions only; never auto-links or rewrites content.
 */
export function RelatedKnowledgePanel({ contentId }: RelatedKnowledgePanelProps) {
  const { token } = useAuth();
  const [items, setItems] = useState<SearchContextItemDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [loaded, setLoaded] = useState(false);

  const load = useCallback(() => {
    if (!token || !contentId) return;

    const controller = new AbortController();
    setLoading(true);
    setError(null);
    fetchRelatedKnowledge(token, "content", contentId, 6, controller.signal)
      .then((dto) => {
        setItems(dto.items ?? []);
        setLoaded(true);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });

    return () => controller.abort();
  }, [token, contentId]);

  useEffect(() => {
    // Explicit panel mount load — does not mutate content.
    return load();
  }, [load]);

  return (
    <section className="space-y-3" aria-labelledby="related-knowledge-heading">
      <div className="space-y-1">
        <h2 id="related-knowledge-heading" className="adm-text text-[15px] font-bold">
          دانش مرتبط
        </h2>
        <p className="adm-subtle text-[12px] leading-6">
          پیشنهادهای مشابه از مقالات، دوره‌ها، ابزارها و پرامپت‌های ایندکس‌شده. پیوند خودکار ایجاد نمی‌شود.
        </p>
      </div>

      {loading ? <p className="adm-subtle text-[12px]">در حال یافتن موارد مرتبط…</p> : null}

      {error ? (
        <AdminErrorState
          error={error}
          title="بارگذاری دانش مرتبط ناموفق بود"
          onRetry={load}
          showHome={false}
        />
      ) : null}

      {loaded && !loading && !error && items.length === 0 ? (
        <p className="adm-subtle rounded-lg border border-dashed border-[var(--adm-border)] p-3 text-center text-[12px]">
          مورد مرتبطی پیدا نشد. پس از انتشار و ایندکس معنایی دوباره بررسی کنید.
        </p>
      ) : null}

      {items.length > 0 ? (
        <ul className="space-y-2">
          {items.map((item) => (
            <li
              key={`${item.sourceType}-${item.sourceId}`}
              className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3"
            >
              <div className="flex items-center justify-between gap-2">
                <p className="adm-text text-[13px] font-semibold">{item.title}</p>
                <span className="adm-subtle shrink-0 text-[11px]" dir="ltr">
                  {item.sourceType}
                </span>
              </div>
              <p className="adm-subtle mt-1 text-[12px] leading-5">{item.snippet}</p>
              <p className="adm-subtle mt-2 text-[11px]" dir="ltr">
                {item.sourceUrl} · similarity {item.similarity.toFixed(2)}
              </p>
            </li>
          ))}
        </ul>
      ) : null}
    </section>
  );
}
