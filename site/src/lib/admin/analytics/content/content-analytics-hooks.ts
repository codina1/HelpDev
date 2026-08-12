"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import {
  fetchContentAnalyticsOverview,
  fetchContentHealth,
  fetchContentItemAnalytics,
  fetchTopContentAnalytics,
} from "@/lib/admin/analytics/content/content-analytics-api";
import type {
  ContentAnalyticsOverviewDto,
  ContentHealthIndicatorDto,
  ContentItemAnalyticsDto,
  ContentPerformanceDto,
} from "@/lib/admin/analytics/content/content-analytics-types";

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

export function useContentAnalyticsDashboard(): {
  overview: LoadState<ContentAnalyticsOverviewDto>;
  top: LoadState<ContentPerformanceDto[]>;
  health: LoadState<ContentHealthIndicatorDto[]>;
} {
  const overviewLoader = useCallback(
    (token: string, signal: AbortSignal) => fetchContentAnalyticsOverview(token, signal),
    [],
  );
  const topLoader = useCallback(
    (token: string, signal: AbortSignal) => fetchTopContentAnalytics(token, 10, signal),
    [],
  );
  const healthLoader = useCallback(
    (token: string, signal: AbortSignal) => fetchContentHealth(token, 20, signal),
    [],
  );

  return {
    overview: useTokenLoader(overviewLoader),
    top: useTokenLoader(topLoader),
    health: useTokenLoader(healthLoader),
  };
}

export function useContentItemAnalytics(contentId: string): LoadState<ContentItemAnalyticsDto> {
  const loader = useCallback(
    (token: string, signal: AbortSignal) => fetchContentItemAnalytics(token, contentId, signal),
    [contentId],
  );
  return useTokenLoader(loader);
}
