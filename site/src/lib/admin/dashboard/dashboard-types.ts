/**
 * Dashboard data contracts.
 *
 * `*Dto` types mirror the real backend JSON returned by existing `/api/v1`
 * endpoints (never invented). View-model types are what widgets consume after
 * mapping. No metric is hardcoded — every value originates from an API.
 */

export type OperationalStatus = "Healthy" | "Degraded" | "Unhealthy" | "Unknown";

/* ----------------------------- Raw backend DTOs ---------------------------- */

// GET /admin/dashboard
export type AdminDashboardDto = {
  users: { totalUsers: number; activeUsers: number; registrationsToday: number };
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
  };
};

// GET /admin/operations/health
export type HealthComponentDto = {
  name: string;
  status: string;
  code: string | null;
  summary: string;
  durationMilliseconds: number;
  checkedAtUtc: string;
};

export type HealthDto = {
  checkedAtUtc: string;
  overallStatus: string;
  applicationVersion: string;
  environmentName: string;
  uptimeSeconds: number;
  scope: string;
  components: HealthComponentDto[];
};

// GET /admin/operations/status
export type OperationsStatusDto = {
  overallStatus: string;
  checkedAtUtc: string;
  application: { version: string; environment: string; uptimeSeconds: number };
  database: { status: string; latencyMilliseconds: number };
  outbox: {
    status: string;
    pendingCount: number;
    failedCount: number;
    deadLetterCount: number;
    oldestPendingAgeSeconds: number | null;
  };
  search: { status: string; pendingCount: number; failedCount: number };
  analytics: {
    status: string;
    recentProcessedCount: number;
    recentFailedCount: number;
  };
  audit: { status: string; persistenceAvailable: boolean };
};

// GET /admin/audit
export type AuditRecordDto = {
  id: string;
  occurredAtUtc: string;
  category: string;
  action: string;
  outcome: string;
  actorUserId: string | null;
  actorType: string | null;
  subjectId: string | null;
  subjectType: string | null;
  subjectDisplay: string | null;
  correlationId: string | null;
};

export type AuditPageDto = {
  items: AuditRecordDto[];
  page: number;
  pageSize: number;
  totalCount: number;
};

/* ------------------------------- View models ------------------------------- */

export type DashboardOverview = {
  users: { total: number; active: number; registrationsToday: number };
  content: { total: number; published: number; draft: number };
  learning: { courses: number; enrollments: number };
};

export type SystemComponent = {
  key: string;
  label: string;
  status: OperationalStatus;
  summary: string;
};

export type SystemHealth = {
  overall: OperationalStatus;
  environment: string;
  version: string;
  healthyCount: number;
  totalCount: number;
  components: SystemComponent[];
};

export type OperationsSummary = {
  overall: OperationalStatus;
  outbox: { status: OperationalStatus; pending: number; failed: number };
  search: { status: OperationalStatus; pending: number };
  analytics: { status: OperationalStatus; recentProcessed: number };
  audit: { status: OperationalStatus; available: boolean };
};

export type ContentPipeline = {
  draft: number;
  published: number;
  total: number;
};

export type ActivityItem = {
  id: string;
  actorLabel: string;
  actionLabel: string;
  outcome: string;
  occurredAtUtc: string;
};

export type RecentContentItem = {
  id: string;
  title: string;
  typeLabel: string;
  status: string;
  createdAt: string;
};
