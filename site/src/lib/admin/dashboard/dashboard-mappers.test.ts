import { describe, expect, it } from "vitest";
import {
  formatNumberFa,
  formatRelativeTimeFa,
  labelForActor,
  labelForAuditAction,
  labelForContentType,
  mapActivity,
  mapContentPipeline,
  mapDashboardOverview,
  mapOperationsSummary,
  mapRecentContent,
  mapSystemHealth,
  normalizeStatus,
} from "./dashboard-mappers";
import type {
  AdminDashboardDto,
  AuditPageDto,
  HealthDto,
  OperationsStatusDto,
} from "./dashboard-types";
import type { ContentSummaryDto } from "@/lib/api/content";

const dashboardDto: AdminDashboardDto = {
  users: { totalUsers: 18520, activeUsers: 12000, registrationsToday: 42 },
  content: {
    totalContent: 1240,
    publishedContent: 980,
    draftContent: 260,
    publicationsToday: 5,
  },
  learning: {
    totalCourses: 32,
    publishedCourses: 20,
    totalEnrollments: 8420,
    enrollmentsToday: 11,
  },
  search: {
    totalSearchDocuments: 1000,
    publishedSearchDocuments: 980,
    lastIndexedAtUtc: null,
  },
  outbox: { pending: 0, processing: 0, failed: 8, processed: 100 },
};

describe("normalizeStatus", () => {
  it("normalizes known statuses case-insensitively", () => {
    expect(normalizeStatus("Healthy")).toBe("Healthy");
    expect(normalizeStatus("degraded")).toBe("Degraded");
    expect(normalizeStatus("UNHEALTHY")).toBe("Unhealthy");
  });

  it("falls back to Unknown for unrecognized values", () => {
    expect(normalizeStatus(null)).toBe("Unknown");
    expect(normalizeStatus(undefined)).toBe("Unknown");
    expect(normalizeStatus("weird")).toBe("Unknown");
  });
});

describe("mapDashboardOverview / mapContentPipeline", () => {
  it("maps real dashboard values without inventing numbers", () => {
    const overview = mapDashboardOverview(dashboardDto);
    expect(overview.users.total).toBe(18520);
    expect(overview.users.active).toBe(12000);
    expect(overview.content.total).toBe(1240);
    expect(overview.content.published).toBe(980);
    expect(overview.content.draft).toBe(260);
    expect(overview.learning.courses).toBe(32);
    expect(overview.learning.enrollments).toBe(8420);
  });

  it("derives the content pipeline from dashboard content only", () => {
    const pipeline = mapContentPipeline(dashboardDto);
    expect(pipeline).toEqual({ draft: 260, published: 980, total: 1240 });
  });
});

describe("mapSystemHealth", () => {
  const healthDto: HealthDto = {
    checkedAtUtc: "2026-07-21T08:00:00Z",
    overallStatus: "Degraded",
    applicationVersion: "1.0.0.0",
    environmentName: "Development",
    uptimeSeconds: 0,
    scope: "Instance",
    components: [
      { name: "self", status: "Healthy", code: null, summary: "ok", durationMilliseconds: 0, checkedAtUtc: "" },
      { name: "postgresql", status: "Healthy", code: null, summary: "db", durationMilliseconds: 0, checkedAtUtc: "" },
      { name: "search_projection", status: "Healthy", code: null, summary: "s", durationMilliseconds: 0, checkedAtUtc: "" },
      { name: "outbox", status: "Degraded", code: "warn", summary: "backlog", durationMilliseconds: 0, checkedAtUtc: "" },
      { name: "analytics", status: "Healthy", code: null, summary: "a", durationMilliseconds: 0, checkedAtUtc: "" },
      { name: "audit", status: "Healthy", code: null, summary: "au", durationMilliseconds: 0, checkedAtUtc: "" },
    ],
  };

  it("maps six components in a fixed order with friendly labels", () => {
    const health = mapSystemHealth(healthDto);
    expect(health.components.map((c) => c.key)).toEqual([
      "api",
      "database",
      "search",
      "outbox",
      "analytics",
      "audit",
    ]);
    expect(health.components[0].label).toBe("API");
    expect(health.overall).toBe("Degraded");
    expect(health.totalCount).toBe(6);
    expect(health.healthyCount).toBe(5);
    expect(health.environment).toBe("Development");
  });

  it("marks missing components as Unknown", () => {
    const health = mapSystemHealth({ ...healthDto, components: [] });
    expect(health.healthyCount).toBe(0);
    expect(health.components.every((c) => c.status === "Unknown")).toBe(true);
  });
});

describe("mapOperationsSummary", () => {
  const statusDto: OperationsStatusDto = {
    overallStatus: "Degraded",
    checkedAtUtc: "",
    application: { version: "1", environment: "Development", uptimeSeconds: 0 },
    database: { status: "Healthy", latencyMilliseconds: 0 },
    outbox: { status: "Degraded", pendingCount: 3, failedCount: 8, deadLetterCount: 8, oldestPendingAgeSeconds: null },
    search: { status: "Healthy", pendingCount: 0, failedCount: 0 },
    analytics: { status: "Healthy", recentProcessedCount: 2, recentFailedCount: 0 },
    audit: { status: "Healthy", persistenceAvailable: true },
  };

  it("maps operational metrics from status", () => {
    const summary = mapOperationsSummary(statusDto);
    expect(summary.overall).toBe("Degraded");
    expect(summary.outbox).toEqual({ status: "Degraded", pending: 3, failed: 8 });
    expect(summary.search).toEqual({ status: "Healthy", pending: 0 });
    expect(summary.analytics).toEqual({ status: "Healthy", recentProcessed: 2 });
    expect(summary.audit).toEqual({ status: "Healthy", available: true });
  });
});

describe("audit / activity mapping", () => {
  it("maps known audit actions to Persian labels", () => {
    expect(labelForAuditAction("authentication.otp_verified")).toBe("ورود موفق");
    expect(labelForAuditAction("authorization.access_denied")).toBe("دسترسی رد شد");
  });

  it("humanizes unknown actions without leaking raw codes", () => {
    expect(labelForAuditAction("custom.some_event")).toBe("custom some event");
  });

  it("labels actors by subject display then actor type", () => {
    expect(labelForActor({ subjectDisplay: "مختار", actorType: "User" })).toBe("مختار");
    expect(labelForActor({ subjectDisplay: null, actorType: "Anonymous" })).toBe("مهمان");
    expect(labelForActor({ subjectDisplay: null, actorType: null })).toBe("ناشناس");
  });

  it("maps an audit page to safe activity items (no metadata leaked)", () => {
    const dto: AuditPageDto = {
      page: 1,
      pageSize: 2,
      totalCount: 2,
      items: [
        {
          id: "a",
          occurredAtUtc: "2026-07-21T08:00:00Z",
          category: "Authentication",
          action: "authentication.otp_verified",
          outcome: "Success",
          actorUserId: "u1",
          actorType: "User",
          subjectId: null,
          subjectType: null,
          subjectDisplay: null,
          correlationId: "c1",
        },
      ],
    };
    const items = mapActivity(dto);
    expect(items).toHaveLength(1);
    expect(items[0]).toEqual({
      id: "a",
      actorLabel: "کاربر",
      actionLabel: "ورود موفق",
      outcome: "Success",
      occurredAtUtc: "2026-07-21T08:00:00Z",
    });
    expect(Object.keys(items[0])).not.toContain("correlationId");
    expect(Object.keys(items[0])).not.toContain("metadata");
  });
});

describe("mapRecentContent", () => {
  const items: ContentSummaryDto[] = [
    { id: "1", title: "قدیمی", slug: "a", type: "Article", status: "", views: 0, saves: 0, createdAt: "2026-06-01T00:00:00Z" },
    { id: "2", title: "جدید", slug: "b", type: "News", status: "", views: 0, saves: 0, createdAt: "2026-07-01T00:00:00Z" },
    { id: "3", title: "میانه", slug: "c", type: "Tool", status: "", views: 0, saves: 0, createdAt: "2026-06-15T00:00:00Z" },
  ];

  it("sorts by newest first and applies the limit", () => {
    const mapped = mapRecentContent(items, 2);
    expect(mapped.map((m) => m.id)).toEqual(["2", "3"]);
  });

  it("maps type labels and defaults status to published", () => {
    const mapped = mapRecentContent(items, 1);
    expect(mapped[0].typeLabel).toBe("خبر");
    expect(mapped[0].status).toBe("منتشرشده");
  });

  it("does not mutate the input array", () => {
    const copy = [...items];
    mapRecentContent(items);
    expect(items).toEqual(copy);
  });
});

describe("labelForContentType", () => {
  it("maps known types and falls back to the raw type", () => {
    expect(labelForContentType("RoadmapStep")).toBe("نقشه راه");
    expect(labelForContentType("Unknown")).toBe("Unknown");
  });
});

describe("formatNumberFa", () => {
  it("formats finite numbers and guards against non-finite input", () => {
    expect(formatNumberFa(1000)).toMatch(/[۰-۹]/);
    expect(formatNumberFa(Number.NaN)).toBe("۰");
  });
});

describe("formatRelativeTimeFa", () => {
  const now = Date.parse("2026-07-21T12:00:00Z");

  it("formats minutes and hours relative to a fixed now", () => {
    expect(formatRelativeTimeFa("2026-07-21T11:55:00Z", now)).toContain("دقیقه");
    expect(formatRelativeTimeFa("2026-07-21T09:00:00Z", now)).toContain("ساعت");
  });

  it("returns an empty string for invalid timestamps", () => {
    expect(formatRelativeTimeFa("not-a-date", now)).toBe("");
  });
});
