"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import { mapAdminContentDetail } from "@/lib/admin/content/content-mappers";
import type { AdminContentDetail } from "@/lib/admin/content/content-types";
import {
  ContentRevisionOperationError,
  contentRevisionListQueryKey,
  fetchContentRevisionDetail,
  fetchContentRevisions,
  restoreContentRevisionItem,
} from "@/lib/admin/content/history/history-api";
import { mapContentRevisionDetail, mapContentRevisionPagedResult } from "@/lib/admin/content/history/history-mappers";
import type {
  ContentRevisionDetail,
  ContentRevisionListQuery,
  ContentRevisionPagedResult,
  RestoreContentRevisionPayload,
} from "@/lib/admin/content/history/history-types";

export type ContentRevisionsState = {
  data: ContentRevisionPagedResult | null;
  loading: boolean;
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Paginated revision list for a content item (`GET /admin/content/{id}/revisions`). */
export function useContentRevisions(
  contentId: string | null,
  query: ContentRevisionListQuery,
): ContentRevisionsState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const queryRef = useRef(query);
  queryRef.current = query;
  const hasLoadedRef = useRef(false);

  const [data, setData] = useState<ContentRevisionPagedResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const queryKey = contentId ? contentRevisionListQueryKey(contentId, query) : "";

  const fetchList = useCallback(
    (mode: "initial" | "refresh") => {
      if (!contentId) return;

      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;
      const currentQuery = queryRef.current;

      if (!token) {
        setError(new ContentRevisionOperationError("برای مشاهده تاریخچه باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      fetchContentRevisions(token, contentId, currentQuery, signal)
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapContentRevisionPagedResult(raw));
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
    [contentId, token],
  );

  useEffect(() => {
    if (!contentId) {
      setData(null);
      setLoading(false);
      return;
    }
    fetchList(hasLoadedRef.current ? "refresh" : "initial");
    return () => controllerRef.current?.abort();
  }, [contentId, queryKey, token, fetchList]);

  const reload = useCallback(() => {
    fetchList(hasLoadedRef.current ? "refresh" : "initial");
  }, [fetchList]);

  return { data, loading, refreshing, error, reload };
}

export type ContentRevisionDetailState = {
  data: ContentRevisionDetail | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads a single revision snapshot (`GET /admin/content/{id}/revisions/{version}`). */
export function useContentRevisionDetail(
  contentId: string | null,
  version: number | null,
): ContentRevisionDetailState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const [data, setData] = useState<ContentRevisionDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    if (!contentId || version == null) {
      setData(null);
      setLoading(false);
      setError(null);
      return;
    }

    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    setLoading(true);
    setError(null);

    if (!token) {
      setError(new ContentRevisionOperationError("برای مشاهده نسخه باید وارد شوید."));
      setData(null);
      setLoading(false);
      return;
    }

    fetchContentRevisionDetail(token, contentId, version, signal)
      .then((raw) => {
        if (signal.aborted) return;
        setData(mapContentRevisionDetail(raw));
      })
      .catch((err) => {
        if (signal.aborted) return;
        setError(err);
        setData(null);
      })
      .finally(() => {
        if (!signal.aborted) setLoading(false);
      });
  }, [contentId, version, token]);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return { data, loading, error, reload: load };
}

export type RestoreContentRevisionState = {
  restore: (
    contentId: string,
    version: number,
    payload?: RestoreContentRevisionPayload,
  ) => Promise<AdminContentDetail>;
  submitting: boolean;
  error: unknown | null;
  reset: () => void;
};

/** POST /admin/content/{id}/revisions/{version}/restore */
export function useRestoreContentRevision(): RestoreContentRevisionState {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const restore = useCallback(
    async (
      contentId: string,
      version: number,
      payload: RestoreContentRevisionPayload = {},
    ): Promise<AdminContentDetail> => {
      if (!token) {
        throw new ContentRevisionOperationError("برای بازیابی نسخه باید وارد شوید.");
      }
      setSubmitting(true);
      setError(null);
      try {
        const raw = await restoreContentRevisionItem(token, contentId, version, payload);
        return mapAdminContentDetail(raw);
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

  return { restore, submitting, error, reset };
}
