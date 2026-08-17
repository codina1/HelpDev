"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import { listWriterPrompts } from "@/lib/api/promptlab-writer";
import { listPromptAiModels, listPromptCategories } from "@/lib/api/promptlab";
import { mapWriterPromptPagedResult } from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import { writerPromptListQueryKey } from "@/lib/admin/prompt-lab/writer-prompt-url-state";
import type {
  WriterPromptListQuery,
  WriterPromptPagedResult,
  WriterPromptStats,
} from "@/lib/admin/prompt-lab/writer-prompt-types";

export type WriterPromptListState = {
  data: WriterPromptPagedResult | null;
  loading: boolean;
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

export function useWriterPromptList(query: WriterPromptListQuery): WriterPromptListState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const hasLoadedRef = useRef(false);
  const queryRef = useRef(query);
  queryRef.current = query;

  const [data, setData] = useState<WriterPromptPagedResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const queryKey = writerPromptListQueryKey(query);

  const fetchList = useCallback(
    (mode: "initial" | "refresh") => {
      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;
      const currentQuery = queryRef.current;

      if (!token) {
        setError(new Error("برای مشاهده پرامپت‌ها باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      listWriterPrompts(
        token,
        {
          page: currentQuery.page,
          pageSize: currentQuery.pageSize,
          status: currentQuery.status === "all" ? undefined : currentQuery.status,
        },
        signal,
      )
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapWriterPromptPagedResult(raw));
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

export type WriterPromptStatsState = {
  stats: WriterPromptStats | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads writer prompt totals via lightweight list requests (pageSize=1). */
export function useWriterPromptStats(): WriterPromptStatsState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const [stats, setStats] = useState<WriterPromptStats | null>(null);
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

    Promise.all([
      listWriterPrompts(token, { page: 1, pageSize: 1 }, signal),
      listWriterPrompts(token, { status: "Draft", page: 1, pageSize: 1 }, signal),
      listWriterPrompts(token, { status: "Submitted", page: 1, pageSize: 1 }, signal),
      listWriterPrompts(token, { status: "Approved", page: 1, pageSize: 1 }, signal),
    ])
      .then(([all, drafts, pending, published]) => {
        if (signal.aborted) return;
        setStats({
          total: all.total,
          drafts: drafts.total,
          pendingReview: pending.total,
          published: published.total,
        });
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

  return { stats, loading, error, reload };
}

export type WriterPromptCatalogState = {
  categories: { id: string; name: string; slug: string }[];
  aiModels: { id: string; name: string; slug: string; provider: string }[];
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads active categories and AI models for the writer prompt form. */
export function useWriterPromptCatalog(): WriterPromptCatalogState {
  const controllerRef = useRef<AbortController | null>(null);
  const [categories, setCategories] = useState<WriterPromptCatalogState["categories"]>([]);
  const [aiModels, setAiModels] = useState<WriterPromptCatalogState["aiModels"]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    setLoading(true);
    setError(null);

    Promise.all([listPromptCategories(signal), listPromptAiModels(signal)])
      .then(([nextCategories, nextModels]) => {
        if (signal.aborted) return;
        setCategories(
          nextCategories.map((item) => ({ id: item.id, name: item.name, slug: item.slug })),
        );
        setAiModels(
          nextModels.map((item) => ({
            id: item.id,
            name: item.name,
            slug: item.slug,
            provider: item.provider,
          })),
        );
      })
      .catch((err) => {
        if (signal.aborted) return;
        setError(err);
        setCategories([]);
        setAiModels([]);
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });
  }, []);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return { categories, aiModels, loading, error, reload: load };
}
