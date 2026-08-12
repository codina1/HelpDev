"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import {
  MEDIA_CAPABILITIES,
  MediaOperationUnsupportedError,
  fetchAdminMediaById,
  fetchAdminMediaList,
  uploadMediaAssetItem,
} from "@/lib/admin/media/media-api";
import { mapAdminMediaDetail, mapAdminMediaPagedResult } from "@/lib/admin/media/media-mappers";
import { adminMediaListQueryKey } from "@/lib/admin/media/media-url-state";
import type {
  AdminMediaDetail,
  AdminMediaListQuery,
  AdminMediaPagedResult,
  UploadMediaPayload,
} from "@/lib/admin/media/media-types";

export type AdminMediaListState = {
  data: AdminMediaPagedResult | null;
  loading: boolean;
  /** True when a subsequent fetch is in flight while previous rows remain visible. */
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

/**
 * Server-paginated Media list (`GET /admin/media`).
 * Aborts stale requests and preserves prior rows during filter/page refreshes.
 */
export function useAdminMediaList(query: AdminMediaListQuery): AdminMediaListState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const queryRef = useRef(query);
  queryRef.current = query;
  const hasLoadedRef = useRef(false);

  const [data, setData] = useState<AdminMediaPagedResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const queryKey = adminMediaListQueryKey(query);

  const fetchList = useCallback(
    (mode: "initial" | "refresh") => {
      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;
      const currentQuery = queryRef.current;

      if (!token) {
        setError(new MediaOperationUnsupportedError("برای مشاهده رسانه‌ها باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      fetchAdminMediaList(token, currentQuery, signal)
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapAdminMediaPagedResult(raw));
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

export type AdminMediaDetailState = {
  data: AdminMediaDetail | null;
  loading: boolean;
  error: unknown | null;
  reload: () => void;
};

/** Loads a single media asset by id (`GET /admin/media/{id}`). */
export function useAdminMediaDetail(id: string | null): AdminMediaDetailState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const [data, setData] = useState<AdminMediaDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown | null>(null);

  const load = useCallback(() => {
    if (!id) {
      setLoading(false);
      return;
    }

    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    setLoading(true);
    setError(null);

    if (!token) {
      setError(new MediaOperationUnsupportedError("برای مشاهده رسانه باید وارد شوید."));
      setData(null);
      setLoading(false);
      return;
    }

    fetchAdminMediaById(token, id, signal)
      .then((raw) => {
        if (signal.aborted) return;
        setData(mapAdminMediaDetail(raw));
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

export type UploadMediaAssetState = {
  upload: (payload: UploadMediaPayload) => Promise<AdminMediaDetail>;
  submitting: boolean;
  error: unknown | null;
  reset: () => void;
};

/**
 * Uploads a single image via `POST /admin/media` (multipart/form-data).
 * There is no real byte-level progress from `fetch`, so this never fabricates
 * an upload percentage — only a boolean `submitting` state.
 */
export function useUploadMediaAsset(): UploadMediaAssetState {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const upload = useCallback(
    async (payload: UploadMediaPayload): Promise<AdminMediaDetail> => {
      if (!token) {
        throw new MediaOperationUnsupportedError("برای بارگذاری رسانه باید وارد شوید.");
      }
      setSubmitting(true);
      setError(null);
      try {
        const raw = await uploadMediaAssetItem(token, payload);
        return mapAdminMediaDetail(raw);
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

  return { upload, submitting, error, reset };
}

export { MEDIA_CAPABILITIES };
