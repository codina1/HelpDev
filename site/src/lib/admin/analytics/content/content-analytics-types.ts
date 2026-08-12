export type ContentMetricType = "View" | "SearchClick" | "Favorite" | "Save" | "Share" | "Completion";

export type ContentHealthStatus = "Healthy" | "NeedsAttention" | "Critical" | "Unknown";

export type ContentMetricDto = {
  metricType: ContentMetricType;
  value: number;
  periodStartUtc: string;
  periodEndUtc: string;
};

export type ContentAnalyticsOverviewDto = {
  range: { fromUtc: string; toUtc: string };
  totalViews: number;
  contentCreated: number;
  contentPublished: number;
  contentsWithViews: number;
  supportedMetrics: ContentMetricDto[];
};

export type ContentPerformanceDto = {
  contentId: string;
  title: string;
  slug: string | null;
  views: number;
  metrics: ContentMetricDto[];
  generatedAtUtc: string;
};

export type ContentHealthIndicatorDto = {
  contentId: string;
  title: string;
  status: string | null;
  healthStatus: ContentHealthStatus;
  reasons: string[];
  viewsInPeriod: number | null;
  revisionCount: number;
  updatedAtUtc: string;
};

export type ContentItemAnalyticsDto = {
  performance: ContentPerformanceDto;
  health: ContentHealthIndicatorDto | null;
};
