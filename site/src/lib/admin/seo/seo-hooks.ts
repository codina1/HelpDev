"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import { fetchSeoDashboard } from "@/lib/admin/seo/seo-api";
import type { SeoDashboard } from "@/lib/admin/seo/seo-types";

export type SeoDashboardState = {
  data: SeoDashboard | null;
  loading: boolean;
  error: unknown;
  reload: () => void;
};

export function useSeoDashboard(): SeoDashboardState {
  const { token } = useAuth();
  const [data, setData] = useState<SeoDashboard | null>(null);
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

    fetchSeoDashboard(token, controller.signal)
      .then((dto) => {
        setData(dto);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });

    return () => controller.abort();
  }, [token, tick]);

  return { data, loading, error, reload };
}
