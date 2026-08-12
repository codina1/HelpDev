import { apiRequest, type ApiRequestOptions } from "./client";

/**
 * Typed foundation for existing Admin APIs. Every method requires a Bearer
 * token; the backend authorization remains authoritative (client checks only
 * improve UX). Requests are sent with `no-store` so Admin views never render
 * stale cached data.
 */

function adminGet<T>(path: string, token: string, signal?: AbortSignal): Promise<T> {
  return apiRequest<T>({ path, token, signal, cache: "no-store" });
}

function adminSend<T>(
  method: NonNullable<ApiRequestOptions["method"]>,
  path: string,
  token: string,
  body?: unknown,
  signal?: AbortSignal,
): Promise<T> {
  return apiRequest<T>({ method, path, token, body, signal, cache: "no-store" });
}

export type AdminDashboardDto = {
  users: {
    totalUsers: number;
    activeUsers: number;
    registrationsToday: number;
  };
  content: {
    totalContent: number;
    publishedContent: number;
    draftContent: number;
    publicationsToday: number | null;
  };
  learning: {
    totalCourses: number;
    publishedCourses: number;
    totalEnrollments: number;
    enrollmentsToday: number;
  };
  search: {
    totalSearchDocuments: number;
    publishedSearchDocuments: number;
    lastIndexedAtUtc: string | null;
  };
  outbox: {
    pending: number;
    processing: number;
    failed: number;
    processed: number;
    oldestPendingAtUtc: string | null;
    lastProcessedAtUtc: string | null;
  };
  recentItems: Array<{
    category: string;
    id: string;
    title: string;
    occurredAtUtc: string;
  }>;
};

export type OperationalVersionDto = {
  version: string;
  informationalVersion?: string | null;
  commit?: string | null;
  buildTimestampUtc?: string | null;
  channel: string;
  environment: string;
  uptimeSeconds: number;
};

export const adminApi = {
  getDashboard: (token: string, signal?: AbortSignal) =>
    adminGet<AdminDashboardDto>("/admin/dashboard", token, signal),

  getUsers: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/users", token, signal),

  getUser: <T = unknown>(token: string, userId: string, signal?: AbortSignal) =>
    adminGet<T>(`/admin/users/${encodeURIComponent(userId)}`, token, signal),

  getFeatureFlags: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/features", token, signal),

  getSettings: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/settings", token, signal),

  getAnnouncements: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/announcements", token, signal),

  getAnalytics: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/analytics", token, signal),

  getAudit: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/audit", token, signal),

  getOutbox: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/outbox", token, signal),

  getOperations: <T = unknown>(token: string, signal?: AbortSignal) =>
    adminGet<T>("/admin/operations", token, signal),

  getVersion: (token: string, signal?: AbortSignal) =>
    adminGet<OperationalVersionDto>("/admin/operations/version", token, signal),
} as const;

export { adminGet, adminSend };
