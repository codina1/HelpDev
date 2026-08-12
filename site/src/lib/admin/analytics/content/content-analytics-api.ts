import { apiRequest } from "@/lib/api/client";
import type {
  ContentAnalyticsOverviewDto,
  ContentHealthIndicatorDto,
  ContentItemAnalyticsDto,
  ContentPerformanceDto,
} from "@/lib/admin/analytics/content/content-analytics-types";

export async function fetchContentAnalyticsOverview(
  token: string,
  signal?: AbortSignal,
): Promise<ContentAnalyticsOverviewDto> {
  return apiRequest<ContentAnalyticsOverviewDto>({
    token,
    method: "GET",
    path: "/admin/analytics/content",
    signal,
  });
}

export async function fetchTopContentAnalytics(
  token: string,
  limit = 10,
  signal?: AbortSignal,
): Promise<ContentPerformanceDto[]> {
  return apiRequest<ContentPerformanceDto[]>({
    token,
    method: "GET",
    path: `/admin/analytics/top-content?limit=${encodeURIComponent(String(limit))}`,
    signal,
  });
}

export async function fetchContentHealth(
  token: string,
  limit = 20,
  signal?: AbortSignal,
): Promise<ContentHealthIndicatorDto[]> {
  return apiRequest<ContentHealthIndicatorDto[]>({
    token,
    method: "GET",
    path: `/admin/analytics/content-health?limit=${encodeURIComponent(String(limit))}`,
    signal,
  });
}

export async function fetchContentItemAnalytics(
  token: string,
  contentId: string,
  signal?: AbortSignal,
): Promise<ContentItemAnalyticsDto> {
  return apiRequest<ContentItemAnalyticsDto>({
    token,
    method: "GET",
    path: `/admin/analytics/content/${encodeURIComponent(contentId)}`,
    signal,
  });
}
