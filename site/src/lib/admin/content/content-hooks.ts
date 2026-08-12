"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import {
  analyzeContentSeo,
  CONTENT_CAPABILITIES,
  ContentOperationUnsupportedError,
  createContentItem,
  fetchAdminContentById,
  fetchAdminContentList,
  fetchArticleMetadata,
  fetchContentBySlug,
  fetchContentStats,
  fetchNewsMetadata,
  publishContentItem,
  runContentAi,
  updateArticleMetadata,
  updateContentItem,
  updateNewsMetadata,
  updateSeoMetadata,
} from "@/lib/admin/content/content-api";
import {
  mapAdminContentDetail,
  mapAdminContentPagedResult,
  mapArticleForm,
  mapContentDetail,
  mapNewsForm,
  mapSeoAnalysisReport,
} from "@/lib/admin/content/content-mappers";
import { adminContentListQueryKey } from "@/lib/admin/content/content-url-state";
import type {
  AdminContentDetail,
  AdminContentDetailRawDto,
  AdminContentListQuery,
  AdminContentPagedResult,
  ArticleFormValues,
  ArticleMetadataRawDto,
  ContentAiResult,
  ContentAiStatus,
  ContentDetail,
  ContentStats,
  CreateContentPayload,
  NewsFormValues,
  NewsMetadataRawDto,
  SeoAnalysisReport,
  SeoAnalysisStatus,
  UpdateArticleMetadataPayload,
  UpdateContentPayload,
  UpdateNewsMetadataPayload,
  UpdateSeoMetadataPayload,
} from "@/lib/admin/content/content-types";
import { EMPTY_ARTICLE_FORM, EMPTY_NEWS_FORM } from "@/lib/admin/content/content-types";

export type ContentStatsState = {
  stats: ContentStats | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads global content aggregates from the Admin Dashboard (honest totals). */
export function useContentStats(): ContentStatsState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const [stats, setStats] = useState<ContentStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    if (!token) {
      setStats(null);
      setLoading(false);
      setError(null);
      return;
    }

    setLoading(true);
    setError(null);

    fetchContentStats(token, signal)
      .then((value) => {
        if (signal.aborted) return;
        setStats(value);
      })
      .catch((err) => {
        if (signal.aborted) return;
        setError(err);
        setStats(null);
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });
  }, [token]);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return { stats, loading, error, reload: load };
}

export type AdminContentListState = {
  data: AdminContentPagedResult | null;
  loading: boolean;
  /** True when a subsequent fetch is in flight while previous rows remain visible. */
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

/**
 * Server-paginated Admin content list (`GET /admin/content`).
 * Aborts stale requests and preserves prior rows during filter/page refreshes.
 */
export function useAdminContentList(query: AdminContentListQuery): AdminContentListState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const queryRef = useRef(query);
  queryRef.current = query;
  const hasLoadedRef = useRef(false);

  const [data, setData] = useState<AdminContentPagedResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const queryKey = adminContentListQueryKey(query);

  const fetchList = useCallback(
    (mode: "initial" | "refresh") => {
      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;
      const currentQuery = queryRef.current;

      if (!token) {
        setError(new ContentOperationUnsupportedError("برای مشاهده فهرست محتوا باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      fetchAdminContentList(token, currentQuery, signal)
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapAdminContentPagedResult(raw));
          hasLoadedRef.current = true;
        })
        .catch((err) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setError(err);
          if (mode === "initial") {
            setData(null);
            hasLoadedRef.current = false;
          }
        })
        .finally(() => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setLoading(false);
          setRefreshing(false);
        });
    },
    [token],
  );

  useEffect(() => {
    fetchList(hasLoadedRef.current ? "refresh" : "initial");
    return () => controllerRef.current?.abort();
  }, [queryKey, token, fetchList]);

  const reload = useCallback(() => {
    fetchList(hasLoadedRef.current ? "refresh" : "initial");
  }, [fetchList]);

  return { data, loading, refreshing, error, reload };
}

export type ContentDetailState = {
  data: ContentDetail | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads a single published content item by slug (public API; not used by Admin workspace). */
export function useContent(slug: string | null): ContentDetailState {
  const controllerRef = useRef<AbortController | null>(null);
  const [data, setData] = useState<ContentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    if (!slug) return;
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    setLoading(true);
    setError(null);

    fetchContentBySlug(slug, signal)
      .then((raw) => {
        if (signal.aborted) return;
        setData(mapContentDetail(raw));
      })
      .catch((err) => {
        if (signal.aborted) return;
        setError(err);
        setData(null);
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });
  }, [slug]);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return { data, loading, error, reload: load };
}

export type AdminContentDetailState = {
  data: AdminContentDetail | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/**
 * Loads a single content item by id from the Admin Read Model
 * (`GET /admin/content/{id}`). Returns full detail — body, excerpt, cover, SEO,
 * timestamps — for both drafts and published items. Requires an auth token.
 */
export function useAdminContentDetail(id: string | null): AdminContentDetailState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const [data, setData] = useState<AdminContentDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    if (!id) return;
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    setLoading(true);
    setError(null);

    if (!token) {
      setError(new ContentOperationUnsupportedError("برای مشاهده محتوا باید وارد شوید."));
      setData(null);
      setLoading(false);
      return;
    }

    fetchAdminContentById(token, id, signal)
      .then((raw) => {
        if (signal.aborted) return;
        setData(mapAdminContentDetail(raw));
      })
      .catch((err) => {
        if (signal.aborted) return;
        setError(err);
        setData(null);
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });
  }, [id, token]);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return { data, loading, error, reload: load };
}

export type CreateContentState = {
  create: (payload: CreateContentPayload) => Promise<ContentDetail>;
  submitting: boolean;
  error: unknown | null;
  reset: () => void;
};

/** Creates content (real). Supports Draft or Published status at creation. */
export function useCreateContent(): CreateContentState {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const create = useCallback(
    async (payload: CreateContentPayload): Promise<ContentDetail> => {
      if (!token) {
        throw new ContentOperationUnsupportedError("برای ایجاد محتوا باید وارد شوید.");
      }
      setSubmitting(true);
      setError(null);
      try {
        const raw = await createContentItem(token, payload);
        return mapContentDetail(raw);
      } catch (err) {
        setError(err);
        throw err;
      } finally {
        setSubmitting(false);
      }
    },
    [token],
  );

  const reset = useCallback(() => setError(null), []);

  return { create, submitting, error, reset };
}

export type MutationState<TArgs extends unknown[], TResult = AdminContentDetailRawDto> = {
  run: (...args: TArgs) => Promise<TResult>;
  submitting: boolean;
  error: unknown | null;
  reset: () => void;
};

/** Shared mutation hook wiring token, submitting and error state (abort-agnostic). */
function useContentMutation<TArgs extends unknown[], TResult = AdminContentDetailRawDto>(
  action: (token: string, ...args: TArgs) => Promise<TResult>,
  missingTokenMessage: string,
): MutationState<TArgs, TResult> {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const run = useCallback(
    async (...args: TArgs): Promise<TResult> => {
      if (!token) {
        throw new ContentOperationUnsupportedError(missingTokenMessage);
      }
      setSubmitting(true);
      setError(null);
      try {
        return await action(token, ...args);
      } catch (err) {
        setError(err);
        throw err;
      } finally {
        setSubmitting(false);
      }
    },
    [token, action, missingTokenMessage],
  );

  const reset = useCallback(() => setError(null), []);

  return { run, submitting, error, reset };
}

/** Edits an existing content item via PUT /admin/content/{id}. */
export function useUpdateContent(): MutationState<[id: string, payload: UpdateContentPayload]> {
  return useContentMutation(
    (token, id, payload) => updateContentItem(token, id, payload),
    "برای ویرایش محتوا باید وارد شوید.",
  );
}

/** Publishes an existing draft via POST /admin/content/{id}/publish. */
export function usePublishContent(): MutationState<[id: string]> {
  return useContentMutation(
    (token, id) => publishContentItem(token, id),
    "برای انتشار محتوا باید وارد شوید.",
  );
}

/** Updates SEO metadata via PUT /admin/content/{id}/seo. */
export function useUpdateSeoMetadata(): MutationState<[id: string, payload: UpdateSeoMetadataPayload]> {
  return useContentMutation(
    (token, id, payload) => updateSeoMetadata(token, id, payload),
    "برای ذخیره سئو باید وارد شوید.",
  );
}

export type ArticleMetadataState = {
  values: ArticleFormValues;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
  setValues: (patch: Partial<ArticleFormValues>) => void;
  replaceValues: (next: ArticleFormValues) => void;
};

export function useArticleMetadata(id: string | null): ArticleMetadataState {
  const { token } = useAuth();
  const [values, setValuesState] = useState<ArticleFormValues>(EMPTY_ARTICLE_FORM);
  const [loading, setLoading] = useState(Boolean(id));
  const [error, setError] = useState<unknown | null>(null);
  const [tick, setTick] = useState(0);
  const controllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    if (!id) {
      setValuesState(EMPTY_ARTICLE_FORM);
      setLoading(false);
      setError(null);
      return;
    }
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    setLoading(true);
    setError(null);

    if (!token) {
      setError(new ContentOperationUnsupportedError("برای مشاهده تنظیمات مقاله باید وارد شوید."));
      setLoading(false);
      return;
    }

    fetchArticleMetadata(token, id, controller.signal)
      .then((dto) => {
        if (!controller.signal.aborted) {
          setValuesState(mapArticleForm(dto));
          setLoading(false);
        }
      })
      .catch((err) => {
        if (!controller.signal.aborted) {
          setError(err);
          setLoading(false);
        }
      });

    return () => controller.abort();
  }, [id, token, tick]);

  return {
    values,
    loading,
    error,
    reload: () => setTick((n) => n + 1),
    setValues: (patch) => setValuesState((prev) => ({ ...prev, ...patch })),
    replaceValues: setValuesState,
  };
}

export function useUpdateArticleMetadata(): MutationState<
  [id: string, payload: UpdateArticleMetadataPayload],
  ArticleMetadataRawDto
> {
  return useContentMutation(
    (token, id, payload) => updateArticleMetadata(token, id, payload),
    "برای ذخیره تنظیمات مقاله باید وارد شوید.",
  );
}

export type NewsMetadataState = {
  values: NewsFormValues;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
  setValues: (patch: Partial<NewsFormValues>) => void;
  replaceValues: (next: NewsFormValues) => void;
};

export function useNewsMetadata(id: string | null): NewsMetadataState {
  const { token } = useAuth();
  const [values, setValuesState] = useState<NewsFormValues>(() => ({
    ...EMPTY_NEWS_FORM,
    newsDateUtc: new Date().toISOString().slice(0, 16),
  }));
  const [loading, setLoading] = useState(Boolean(id));
  const [error, setError] = useState<unknown | null>(null);
  const [tick, setTick] = useState(0);
  const controllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    if (!id) {
      setValuesState({
        ...EMPTY_NEWS_FORM,
        newsDateUtc: new Date().toISOString().slice(0, 16),
      });
      setLoading(false);
      setError(null);
      return;
    }
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    setLoading(true);
    setError(null);

    if (!token) {
      setError(new ContentOperationUnsupportedError("برای مشاهده تنظیمات خبر باید وارد شوید."));
      setLoading(false);
      return;
    }

    fetchNewsMetadata(token, id, controller.signal)
      .then((dto) => {
        if (!controller.signal.aborted) {
          setValuesState(mapNewsForm(dto));
          setLoading(false);
        }
      })
      .catch((err) => {
        if (!controller.signal.aborted) {
          setError(err);
          setLoading(false);
        }
      });

    return () => controller.abort();
  }, [id, token, tick]);

  return {
    values,
    loading,
    error,
    reload: () => setTick((n) => n + 1),
    setValues: (patch) => setValuesState((prev) => ({ ...prev, ...patch })),
    replaceValues: setValuesState,
  };
}

export function useUpdateNewsMetadata(): MutationState<
  [id: string, payload: UpdateNewsMetadataPayload],
  NewsMetadataRawDto
> {
  return useContentMutation(
    (token, id, payload) => updateNewsMetadata(token, id, payload),
    "برای ذخیره تنظیمات خبر باید وارد شوید.",
  );
}

export type ContentSeoAnalysisState = {
  status: SeoAnalysisStatus;
  report: SeoAnalysisReport | null;
  error: unknown | null;
  /** Explicit user action — runs POST /admin/content/{id}/seo-analysis. Never call on keystroke. */
  analyze: () => void;
  /** Marks a previously-successful report as out of date (no network call). No-op before a first run. */
  markStale: () => void;
};

/**
 * SEO Analyzer Engine v1 — analyzes the SAVED server content only
 * (`POST /admin/content/{id}/seo-analysis`). This is an explicit, user-triggered
 * action: it never runs on mount or on keystroke. The caller (Content Studio)
 * is responsible for invoking {@link ContentSeoAnalysisState.markStale} when the
 * content/SEO form becomes dirty or right after a save, so a previously fetched
 * report is never presented as current without the "stale" label.
 */
export function useContentSeoAnalysis(id: string | null): ContentSeoAnalysisState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const [status, setStatus] = useState<SeoAnalysisStatus>("idle");
  const [report, setReport] = useState<SeoAnalysisReport | null>(null);
  const [error, setError] = useState<unknown | null>(null);

  // Reset all analysis state when switching to a different content item.
  useEffect(() => {
    controllerRef.current?.abort();
    requestIdRef.current += 1;
    setStatus("idle");
    setReport(null);
    setError(null);
  }, [id]);

  useEffect(() => {
    return () => controllerRef.current?.abort();
  }, []);

  const analyze = useCallback(() => {
    if (!id) return;

    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;
    const requestId = ++requestIdRef.current;

    if (!token) {
      setError(new ContentOperationUnsupportedError("برای تحلیل سئو باید وارد شوید."));
      setStatus("error");
      return;
    }

    setStatus("analyzing");
    setError(null);

    analyzeContentSeo(token, id, signal)
      .then((raw) => {
        if (signal.aborted || requestId !== requestIdRef.current) return;
        setReport(mapSeoAnalysisReport(raw));
        setStatus("success");
      })
      .catch((err) => {
        if (signal.aborted || requestId !== requestIdRef.current) return;
        setError(err);
        setStatus("error");
      });
  }, [id, token]);

  // No-op unless a report already exists — "stale" only makes sense relative
  // to a previously fetched analysis; it never triggers a network call.
  const markStale = useCallback(() => {
    setStatus((prev) => (prev === "success" || prev === "stale" ? "stale" : prev));
  }, []);

  return { status, report, error, analyze, markStale };
}

export type ContentAiActionKey =
  | "analyze"
  | "title-suggestions"
  | "meta-description"
  | "outline"
  | "faq";

export type ContentAiAssistantState = {
  status: ContentAiStatus;
  result: ContentAiResult | null;
  activeAction: ContentAiActionKey | null;
  error: unknown | null;
  /** Explicit user action — never auto-applies to the content form. */
  run: (action: ContentAiActionKey) => void;
  clear: () => void;
};

/**
 * Content AI Assistant v1 — on-demand suggestions only.
 * Results are shown for human copy/apply; never written back automatically.
 */
export function useContentAiAssistant(id: string | null): ContentAiAssistantState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const [status, setStatus] = useState<ContentAiStatus>("idle");
  const [result, setResult] = useState<ContentAiResult | null>(null);
  const [activeAction, setActiveAction] = useState<ContentAiActionKey | null>(null);
  const [error, setError] = useState<unknown | null>(null);

  useEffect(() => {
    controllerRef.current?.abort();
    requestIdRef.current += 1;
    setStatus("idle");
    setResult(null);
    setActiveAction(null);
    setError(null);
  }, [id]);

  useEffect(() => {
    return () => controllerRef.current?.abort();
  }, []);

  const clear = useCallback(() => {
    setStatus("idle");
    setResult(null);
    setActiveAction(null);
    setError(null);
  }, []);

  const run = useCallback(
    (action: ContentAiActionKey) => {
      if (!id) return;

      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;

      if (!token) {
        setError(new ContentOperationUnsupportedError("برای استفاده از دستیار هوش مصنوعی باید وارد شوید."));
        setStatus("error");
        setActiveAction(action);
        return;
      }

      setStatus("loading");
      setActiveAction(action);
      setError(null);

      runContentAi(token, id, action, signal)
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setResult({
            taskType: String(raw.taskType ?? ""),
            generatedText: String(raw.generatedText ?? ""),
            createdAtUtc: String(raw.createdAtUtc ?? ""),
            model: String(raw.model ?? ""),
            provider: String(raw.provider ?? ""),
          });
          setStatus("success");
        })
        .catch((err) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setError(err);
          setStatus("error");
        });
    },
    [id, token],
  );

  return { status, result, activeAction, error, run, clear };
}

export { CONTENT_CAPABILITIES };
