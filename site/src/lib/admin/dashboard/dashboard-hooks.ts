"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { useAuth } from "@/components/auth";
import {
  fetchDashboardOverview,
  fetchOperationsStatus,
  fetchRecentAudit,
  fetchRecentContent,
  fetchSystemHealth,
} from "@/lib/admin/dashboard/dashboard-api";
import {
  mapActivity,
  mapContentPipeline,
  mapDashboardOverview,
  mapOperationsSummary,
  mapRecentContent,
  mapSystemHealth,
} from "@/lib/admin/dashboard/dashboard-mappers";
import type {
  ActivityItem,
  ContentPipeline,
  DashboardOverview,
  OperationsSummary,
  RecentContentItem,
  SystemHealth,
} from "@/lib/admin/dashboard/dashboard-types";

export type AsyncSection<T> = {
  data: T | null;
  loading: boolean;
  error: unknown | null;
};

function initialSection<T>(): AsyncSection<T> {
  return { data: null, loading: true, error: null };
}

export type AdminDashboardData = {
  overview: AsyncSection<DashboardOverview>;
  pipeline: AsyncSection<ContentPipeline>;
  health: AsyncSection<SystemHealth>;
  operations: AsyncSection<OperationsSummary>;
  activity: AsyncSection<ActivityItem[]>;
  recentContent: AsyncSection<RecentContentItem[]>;
  reload: () => void;
};

/**
 * Loads all dashboard sections in parallel from existing APIs. Each section
 * tracks its own loading/error so one failing endpoint never blanks the page.
 * In-flight requests are aborted on unmount or reload to avoid races and
 * duplicate work.
 */
export function useAdminDashboard(): AdminDashboardData {
  const { token } = useAuth();
  const controllerRef = useRef<AbortController | null>(null);

  const [overview, setOverview] = useState<AsyncSection<DashboardOverview>>(initialSection);
  const [pipeline, setPipeline] = useState<AsyncSection<ContentPipeline>>(initialSection);
  const [health, setHealth] = useState<AsyncSection<SystemHealth>>(initialSection);
  const [operations, setOperations] = useState<AsyncSection<OperationsSummary>>(initialSection);
  const [activity, setActivity] = useState<AsyncSection<ActivityItem[]>>(initialSection);
  const [recentContent, setRecentContent] =
    useState<AsyncSection<RecentContentItem[]>>(initialSection);

  const load = useCallback(() => {
    if (!token) return;

    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    const { signal } = controller;

    const settle = <T,>(
      setter: (state: AsyncSection<T>) => void,
      promise: Promise<T>,
    ) => {
      setter({ data: null, loading: true, error: null });
      promise
        .then((data) => {
          if (signal.aborted) return;
          setter({ data, loading: false, error: null });
        })
        .catch((error) => {
          if (signal.aborted) return;
          setter({ data: null, loading: false, error });
        });
    };

    // Overview and content pipeline come from a single dashboard fetch.
    setOverview({ data: null, loading: true, error: null });
    setPipeline({ data: null, loading: true, error: null });
    fetchDashboardOverview(token, signal)
      .then((dto) => {
        if (signal.aborted) return;
        setOverview({ data: mapDashboardOverview(dto), loading: false, error: null });
        setPipeline({ data: mapContentPipeline(dto), loading: false, error: null });
      })
      .catch((error) => {
        if (signal.aborted) return;
        setOverview({ data: null, loading: false, error });
        setPipeline({ data: null, loading: false, error });
      });

    settle(setHealth, fetchSystemHealth(token, signal).then(mapSystemHealth));
    settle(setOperations, fetchOperationsStatus(token, signal).then(mapOperationsSummary));
    settle(setActivity, fetchRecentAudit(token, 8, signal).then(mapActivity));
    settle(setRecentContent, fetchRecentContent(signal).then((items) => mapRecentContent(items)));
  }, [token]);

  useEffect(() => {
    load();
    return () => controllerRef.current?.abort();
  }, [load]);

  return {
    overview,
    pipeline,
    health,
    operations,
    activity,
    recentContent,
    reload: load,
  };
}
