"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import {
  approveAdminPrompt,
  listAdminReviewPrompts,
  rejectAdminPrompt,
} from "@/lib/api/promptlab-admin-review";
import { mapAdminPromptReviewPage } from "@/lib/admin/prompt-lab/admin-prompt-review-mappers";
import { adminPromptReviewQueryKey } from "@/lib/admin/prompt-lab/admin-prompt-review-url-state";
import {
  ADMIN_PROMPT_REVIEW_TAB_STATUS,
  type AdminPromptReviewPage,
  type AdminPromptReviewQuery,
} from "@/lib/admin/prompt-lab/admin-prompt-review-types";

export type AdminPromptReviewListState = {
  data: AdminPromptReviewPage | null;
  loading: boolean;
  refreshing: boolean;
  error: unknown | null;
  reload: () => void;
};

export function useAdminPromptReviewList(query: AdminPromptReviewQuery): AdminPromptReviewListState {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);
  const requestIdRef = useRef(0);
  const hasLoadedRef = useRef(false);
  const queryRef = useRef(query);
  queryRef.current = query;

  const [data, setData] = useState<AdminPromptReviewPage | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const queryKey = adminPromptReviewQueryKey(query);

  const fetchList = useCallback(
    (mode: "initial" | "refresh") => {
      controllerRef.current?.abort();
      const controller = new AbortController();
      controllerRef.current = controller;
      const { signal } = controller;
      const requestId = ++requestIdRef.current;
      const currentQuery = queryRef.current;

      if (!token) {
        setError(new Error("برای مشاهده بازبینی پرامپت‌ها باید وارد شوید."));
        setData(null);
        setLoading(false);
        setRefreshing(false);
        hasLoadedRef.current = false;
        return;
      }

      if (mode === "initial") setLoading(true);
      else setRefreshing(true);
      setError(null);

      listAdminReviewPrompts(
        token,
        {
          status: ADMIN_PROMPT_REVIEW_TAB_STATUS[currentQuery.tab],
          page: currentQuery.page,
          pageSize: currentQuery.pageSize,
        },
        signal,
      )
        .then((raw) => {
          if (signal.aborted || requestId !== requestIdRef.current) return;
          setData(mapAdminPromptReviewPage(raw));
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

export function useAdminPromptReviewActions() {
  const { token } = useAuth();
  const [submittingId, setSubmittingId] = useState<string | null>(null);
  const [error, setError] = useState<unknown | null>(null);

  const run = useCallback(
    async (id: string, action: "approve" | "reject", reason?: string) => {
      if (!token) {
        setError(new Error("برای بازبینی پرامپت باید وارد شوید."));
        throw new Error("unauthenticated");
      }
      setSubmittingId(id);
      setError(null);
      try {
        if (action === "approve") {
          await approveAdminPrompt(token, id);
        } else {
          await rejectAdminPrompt(token, id, reason ?? "");
        }
      } catch (err) {
        setError(err);
        throw err;
      } finally {
        setSubmittingId(null);
      }
    },
    [token],
  );

  return { submittingId, error, approve: (id: string) => run(id, "approve"), reject: (id: string, reason: string) => run(id, "reject", reason) };
}
