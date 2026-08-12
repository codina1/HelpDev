"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import { mapAdminContentDetail } from "@/lib/admin/content/content-mappers";
import type { AdminContentDetail } from "@/lib/admin/content/content-types";
import {
  approveContentItem,
  archiveContentItem,
  ContentWorkflowOperationError,
  fetchContentWorkflowHistory,
  publishContentWorkflowItem,
  rejectContentItem,
  submitContentForReviewItem,
} from "@/lib/admin/content/workflow/workflow-api";
import { mapWorkflowHistory } from "@/lib/admin/content/workflow/workflow-mappers";
import type {
  RejectContentPayload,
  WorkflowHistory,
} from "@/lib/admin/content/workflow/workflow-types";

export type WorkflowHistoryState = {
  data: WorkflowHistory | null;
  loading: boolean;
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

/** GET /admin/content/{id}/workflow-history */
export function useContentWorkflowHistory(contentId: string | null): WorkflowHistoryState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const hasLoadedRef = useRef(false);

  const [data, setData] = useState<WorkflowHistory | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const fetchHistory = useCallback(
    (mode: "initial" | "refresh") => {
      if (!contentId) return;

      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;

      if (!token) {
        setError(new ContentWorkflowOperationError("برای مشاهده گردش کار باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      fetchContentWorkflowHistory(token, contentId, signal)
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapWorkflowHistory(raw));
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
    fetchHistory(hasLoadedRef.current ? "refresh" : "initial");
    return () => controllerRef.current?.abort();
  }, [contentId, token, fetchHistory]);

  const reload = useCallback(() => {
    fetchHistory(hasLoadedRef.current ? "refresh" : "initial");
  }, [fetchHistory]);

  return { data, loading, refreshing, error, reload };
}

export type WorkflowMutationState = {
  submitting: boolean;
  error: unknown | null;
  reset: () => void;
};

function useWorkflowDetailMutation(
  mutate: (token: string, contentId: string) => Promise<AdminContentDetail>,
): WorkflowMutationState & {
  run: (contentId: string) => Promise<AdminContentDetail>;
} {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const run = useCallback(
    async (contentId: string): Promise<AdminContentDetail> => {
      if (!token) {
        throw new ContentWorkflowOperationError("برای این عملیات باید وارد شوید.");
      }
      setSubmitting(true);
      setError(null);
      try {
        return await mutate(token, contentId);
      } catch (err) {
        setError(err);
        throw err;
      } finally {
        setSubmitting(false);
      }
    },
    [mutate, token],
  );

  const reset = useCallback(() => setError(null), []);

  return { run, submitting, error, reset };
}

export function useSubmitContentForReview() {
  const mutate = useCallback(async (token: string, contentId: string) => {
    const raw = await submitContentForReviewItem(token, contentId);
    return mapAdminContentDetail(raw);
  }, []);
  return useWorkflowDetailMutation(mutate);
}

export function useApproveContent() {
  const mutate = useCallback(async (token: string, contentId: string) => {
    const raw = await approveContentItem(token, contentId);
    return mapAdminContentDetail(raw);
  }, []);
  return useWorkflowDetailMutation(mutate);
}

export function useRejectContent() {
  const { token } = useAuth();
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const run = useCallback(
    async (contentId: string, payload: RejectContentPayload): Promise<AdminContentDetail> => {
      if (!token) {
        throw new ContentWorkflowOperationError("برای این عملیات باید وارد شوید.");
      }
      const comment = payload.comment.trim();
      if (!comment) {
        throw new ContentWorkflowOperationError("برای رد محتوا باید توضیح وارد کنید.");
      }
      setSubmitting(true);
      setError(null);
      try {
        const raw = await rejectContentItem(token, contentId, { comment });
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

  return { run, submitting, error, reset };
}

export function usePublishContentWorkflow() {
  const mutate = useCallback(async (token: string, contentId: string) => {
    const raw = await publishContentWorkflowItem(token, contentId);
    return mapAdminContentDetail(raw);
  }, []);
  return useWorkflowDetailMutation(mutate);
}

export function useArchiveContent() {
  const mutate = useCallback(async (token: string, contentId: string) => {
    const raw = await archiveContentItem(token, contentId);
    return mapAdminContentDetail(raw);
  }, []);
  return useWorkflowDetailMutation(mutate);
}
