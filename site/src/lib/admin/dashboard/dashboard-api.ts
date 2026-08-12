import { apiRequest } from "@/lib/api/client";
import { listPublishedContent, type ContentSummaryDto } from "@/lib/api/content";
import type {
  AdminDashboardDto,
  AuditPageDto,
  HealthDto,
  OperationsStatusDto,
} from "@/lib/admin/dashboard/dashboard-types";

/**
 * Dashboard data access. All calls go through the shared typed API client and
 * target existing canonical `/api/v1` routes. No new endpoints, no duplicated
 * client logic. Content reuses the existing public content module.
 */

export function fetchDashboardOverview(
  token: string,
  signal?: AbortSignal,
): Promise<AdminDashboardDto> {
  return apiRequest<AdminDashboardDto>({
    path: "/admin/dashboard",
    token,
    signal,
    cache: "no-store",
  });
}

export function fetchSystemHealth(
  token: string,
  signal?: AbortSignal,
): Promise<HealthDto> {
  return apiRequest<HealthDto>({
    path: "/admin/operations/health",
    token,
    signal,
    cache: "no-store",
  });
}

export function fetchOperationsStatus(
  token: string,
  signal?: AbortSignal,
): Promise<OperationsStatusDto> {
  return apiRequest<OperationsStatusDto>({
    path: "/admin/operations/status",
    token,
    signal,
    cache: "no-store",
  });
}

export function fetchRecentAudit(
  token: string,
  pageSize = 8,
  signal?: AbortSignal,
): Promise<AuditPageDto> {
  return apiRequest<AuditPageDto>({
    path: `/admin/audit?page=1&pageSize=${pageSize}`,
    token,
    signal,
    cache: "no-store",
  });
}

export function fetchRecentContent(
  signal?: AbortSignal,
): Promise<ContentSummaryDto[]> {
  return listPublishedContent(signal);
}
