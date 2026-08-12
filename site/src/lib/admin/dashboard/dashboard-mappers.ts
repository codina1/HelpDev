import type { ContentSummaryDto } from "@/lib/api/content";
import type {
  AdminDashboardDto,
  ActivityItem,
  AuditPageDto,
  ContentPipeline,
  DashboardOverview,
  HealthDto,
  OperationalStatus,
  OperationsStatusDto,
  OperationsSummary,
  RecentContentItem,
  SystemComponent,
  SystemHealth,
} from "@/lib/admin/dashboard/dashboard-types";

/** Normalizes any backend status string into a known operational status. */
export function normalizeStatus(raw: string | null | undefined): OperationalStatus {
  switch ((raw ?? "").toLowerCase()) {
    case "healthy":
      return "Healthy";
    case "degraded":
      return "Degraded";
    case "unhealthy":
      return "Unhealthy";
    default:
      return "Unknown";
  }
}

export function mapDashboardOverview(dto: AdminDashboardDto): DashboardOverview {
  return {
    users: {
      total: dto.users.totalUsers,
      active: dto.users.activeUsers,
      registrationsToday: dto.users.registrationsToday,
    },
    content: {
      total: dto.content.totalContent,
      published: dto.content.publishedContent,
      draft: dto.content.draftContent,
    },
    learning: {
      courses: dto.learning.totalCourses,
      enrollments: dto.learning.totalEnrollments,
    },
  };
}

export function mapContentPipeline(dto: AdminDashboardDto): ContentPipeline {
  return {
    draft: dto.content.draftContent,
    published: dto.content.publishedContent,
    total: dto.content.totalContent,
  };
}

const HEALTH_COMPONENT_LABELS: Array<{ names: string[]; key: string; label: string }> = [
  { names: ["self"], key: "api", label: "API" },
  { names: ["postgresql", "database"], key: "database", label: "پایگاه داده" },
  { names: ["search_projection", "search"], key: "search", label: "جستجو" },
  { names: ["outbox"], key: "outbox", label: "Outbox" },
  { names: ["analytics"], key: "analytics", label: "تحلیل‌ها" },
  { names: ["audit"], key: "audit", label: "Audit" },
];

export function mapSystemHealth(dto: HealthDto): SystemHealth {
  const components: SystemComponent[] = HEALTH_COMPONENT_LABELS.map((entry) => {
    const found = dto.components.find((component) =>
      entry.names.includes(component.name.toLowerCase()),
    );
    return {
      key: entry.key,
      label: entry.label,
      status: normalizeStatus(found?.status),
      summary: found?.summary ?? "اطلاعات وضعیت در دسترس نیست",
    };
  });

  const healthyCount = components.filter((c) => c.status === "Healthy").length;

  return {
    overall: normalizeStatus(dto.overallStatus),
    environment: dto.environmentName,
    version: dto.applicationVersion,
    healthyCount,
    totalCount: components.length,
    components,
  };
}

export function mapOperationsSummary(dto: OperationsStatusDto): OperationsSummary {
  return {
    overall: normalizeStatus(dto.overallStatus),
    outbox: {
      status: normalizeStatus(dto.outbox.status),
      pending: dto.outbox.pendingCount,
      failed: dto.outbox.failedCount,
    },
    search: {
      status: normalizeStatus(dto.search.status),
      pending: dto.search.pendingCount,
    },
    analytics: {
      status: normalizeStatus(dto.analytics.status),
      recentProcessed: dto.analytics.recentProcessedCount,
    },
    audit: {
      status: normalizeStatus(dto.audit.status),
      available: dto.audit.persistenceAvailable,
    },
  };
}

const AUDIT_ACTION_LABELS: Record<string, string> = {
  "authentication.otp_requested": "درخواست کد ورود",
  "authentication.otp_verified": "ورود موفق",
  "authentication.otp_verification_failed": "تلاش ناموفق برای ورود",
  "authentication.rate_limited": "محدودیت نرخ درخواست ورود",
  "authentication.login_succeeded": "ورود موفق",
  "authorization.access_denied": "دسترسی رد شد",
  "administration.feature_flag.created": "ایجاد Feature Flag",
  "administration.feature_flag.updated": "به‌روزرسانی Feature Flag",
  "administration.feature_flag.enabled": "فعال‌سازی Feature Flag",
  "administration.feature_flag.disabled": "غیرفعال‌سازی Feature Flag",
  "administration.setting.created": "ایجاد تنظیم",
  "administration.setting.updated": "به‌روزرسانی تنظیم",
  "security.rate_limit_exceeded": "عبور از محدودیت نرخ",
};

export function labelForAuditAction(action: string): string {
  const known = AUDIT_ACTION_LABELS[action];
  if (known) return known;
  // Safe humanization fallback (no raw payloads/metadata are ever surfaced).
  return action.replace(/[._]/g, " ").trim() || "رویداد";
}

const ACTOR_TYPE_LABELS: Record<string, string> = {
  user: "کاربر",
  anonymous: "مهمان",
  system: "سیستم",
  service: "سرویس",
};

export function labelForActor(record: {
  subjectDisplay: string | null;
  actorType: string | null;
}): string {
  if (record.subjectDisplay) return record.subjectDisplay;
  const type = (record.actorType ?? "").toLowerCase();
  return ACTOR_TYPE_LABELS[type] ?? "ناشناس";
}

export function mapActivity(dto: AuditPageDto): ActivityItem[] {
  return dto.items.map((item) => ({
    id: item.id,
    actorLabel: labelForActor(item),
    actionLabel: labelForAuditAction(item.action),
    outcome: item.outcome,
    occurredAtUtc: item.occurredAtUtc,
  }));
}

const CONTENT_TYPE_LABELS: Record<string, string> = {
  article: "مقاله",
  news: "خبر",
  tool: "ابزار",
  roadmapstep: "نقشه راه",
  course: "دوره",
  cheatsheet: "چیت‌شیت",
  prompt: "پرامپت",
};

export function labelForContentType(type: string): string {
  return CONTENT_TYPE_LABELS[type.toLowerCase()] ?? type;
}

export function mapRecentContent(
  items: ContentSummaryDto[],
  limit = 6,
): RecentContentItem[] {
  return [...items]
    .sort(
      (a, b) =>
        new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
    )
    .slice(0, limit)
    .map((item) => ({
      id: item.id,
      title: item.title,
      typeLabel: labelForContentType(item.type),
      status: item.status ? item.status : "منتشرشده",
      createdAt: item.createdAt,
    }));
}

const NUMBER_FORMAT = new Intl.NumberFormat("fa-IR");

/** Formats a number using Persian digits and grouping. */
export function formatNumberFa(value: number): string {
  if (!Number.isFinite(value)) return "۰";
  return NUMBER_FORMAT.format(value);
}

const RELATIVE_TIME = new Intl.RelativeTimeFormat("fa", { numeric: "auto" });

/** Formats an ISO timestamp as a Persian relative time (e.g. «۵ دقیقه پیش»). */
export function formatRelativeTimeFa(
  iso: string,
  now: number = Date.now(),
): string {
  const then = new Date(iso).getTime();
  if (Number.isNaN(then)) return "";

  const diffSeconds = Math.round((then - now) / 1000);
  const abs = Math.abs(diffSeconds);

  if (abs < 60) return RELATIVE_TIME.format(Math.round(diffSeconds), "second");
  const diffMinutes = Math.round(diffSeconds / 60);
  if (Math.abs(diffMinutes) < 60) return RELATIVE_TIME.format(diffMinutes, "minute");
  const diffHours = Math.round(diffMinutes / 60);
  if (Math.abs(diffHours) < 24) return RELATIVE_TIME.format(diffHours, "hour");
  const diffDays = Math.round(diffHours / 24);
  return RELATIVE_TIME.format(diffDays, "day");
}
