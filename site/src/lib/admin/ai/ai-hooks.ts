"use client";

import { useCallback } from "react";
import { useAuth } from "@/components/auth";
import { fetchAiDashboard, type AiDashboardDto } from "@/lib/admin/ai/ai-api";
import { useEffect, useState } from "react";

type LoadState<T> = {
  data: T | null;
  loading: boolean;
  error: unknown;
  reload: () => void;
};

function useTokenLoader<T>(loader: (token: string, signal: AbortSignal) => Promise<T>): LoadState<T> {
  const { token } = useAuth();
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [tick, setTick] = useState(0);
  const reload = useCallback(() => setTick((n) => n + 1), []);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      setData(null);
      return;
    }

    const controller = new AbortController();
    setLoading(true);
    setError(null);

    loader(token, controller.signal)
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });

    return () => controller.abort();
  }, [token, tick, loader]);

  return { data, loading, error, reload };
}

export function useAiDashboard(): LoadState<AiDashboardDto> {
  const loader = useCallback(
    (token: string, signal: AbortSignal) => fetchAiDashboard(token, signal),
    [],
  );
  return useTokenLoader(loader);
}
