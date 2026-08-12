import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import { PageErrorState } from "@/components/ui/page-error-state";
import { PageLoadingState } from "@/components/ui/page-loading-state";
import { ProgressCard } from "@/components/learning/progress-card";
import { RecommendationCard } from "@/components/learning/recommendation-card";
import { RoadmapCard } from "@/components/learning/roadmap-card";
import { CourseCard } from "@/components/learning/course-card";
import { NotificationCenter } from "@/components/notifications/notification-center";
import { ContentBulkToolbar } from "@/components/admin/content/list/content-bulk-toolbar";
import { ApiClientError } from "@/lib/api/errors";
import { emptyNotificationFeed } from "@/lib/notifications";

describe("Sprint 45 — shared page states", () => {
  it("renders empty / loading / error with safe messaging", () => {
    const empty = renderToStaticMarkup(
      <PageEmptyState title="خالی" description="توضیح" />,
    );
    expect(empty).toContain("خالی");

    const loading = renderToStaticMarkup(<PageLoadingState label="بارگذاری" />);
    expect(loading).toContain("بارگذاری");

    const error = renderToStaticMarkup(
      <PageErrorState
        error={new ApiClientError({ message: "شکست", status: 500, correlationId: "corr-1" })}
      />,
    );
    expect(error).toContain("corr-1");
    expect(error).not.toContain("stack");
  });
});

describe("Sprint 45 — learning cards", () => {
  it("renders course / progress / recommendation / roadmap cards", () => {
    expect(
      renderToStaticMarkup(
        <CourseCard
          course={{ id: "1", title: "ASP.NET", slug: "aspnet", status: "Published" }}
          progressPercentage={40}
        />,
      ),
    ).toContain("ASP.NET");

    expect(
      renderToStaticMarkup(
        <ProgressCard title="دوره" progressPercentage={55} status="Active" />,
      ),
    ).toContain("55");

    expect(
      renderToStaticMarkup(
        <RecommendationCard
          item={{
            kind: "Course",
            courseId: "c1",
            title: "پیشنهاد",
            slug: "rec",
            rationale: "دلیل",
          }}
        />,
      ),
    ).toContain("پیشنهاد");

    expect(
      renderToStaticMarkup(
        <RoadmapCard
          roadmap={{
            id: "r1",
            goal: "هدف",
            status: "Suggested",
            steps: [{ stepOrder: 1, title: "گام", description: "د", relatedCourseId: null }],
            createdAtUtc: new Date().toISOString(),
            updatedAtUtc: new Date().toISOString(),
            approvedAtUtc: null,
          }}
        />,
      ),
    ).toContain("هدف");
  });
});

describe("Sprint 45 — notifications and CMS bulk foundation", () => {
  it("shows notification empty state without fake items", () => {
    const feed = emptyNotificationFeed();
    expect(feed.items).toHaveLength(0);
    const html = renderToStaticMarkup(<NotificationCenter feed={feed} />);
    expect(html).toContain("اعلان‌ها");
    expect(html).not.toContain("fake");
  });

  it("disables unsupported bulk actions with explanation", () => {
    const html = renderToStaticMarkup(
      <ContentBulkToolbar selectedCount={2} onClear={() => undefined} />,
    );
    expect(html).toContain("انتشار گروهی");
    expect(html).toContain("API انتشار گروهی");
  });
});
