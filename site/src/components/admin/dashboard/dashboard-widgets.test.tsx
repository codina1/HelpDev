import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ApiClientError } from "@/lib/api/errors";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import type {
  ContentPipeline,
  SystemHealth,
} from "@/lib/admin/dashboard/dashboard-types";
import { StatusBadge } from "@/components/admin/dashboard/widgets/status-badge";
import { KpiCard } from "@/components/admin/dashboard/widgets/kpi-card";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import { ContentPipelineCard } from "@/components/admin/dashboard/widgets/content-pipeline-card";
import { SystemHealthCard } from "@/components/admin/dashboard/widgets/system-health-card";

const noop = () => {};

function section<T>(partial: Partial<AsyncSection<T>>): AsyncSection<T> {
  return { data: null, loading: false, error: null, ...partial };
}

describe("StatusBadge", () => {
  it("renders the Persian label for each status", () => {
    expect(renderToStaticMarkup(<StatusBadge status="Healthy" />)).toContain("سالم");
    expect(renderToStaticMarkup(<StatusBadge status="Degraded" />)).toContain("نیاز به بررسی");
    expect(renderToStaticMarkup(<StatusBadge status="Unhealthy" />)).toContain("خطا");
    expect(renderToStaticMarkup(<StatusBadge status="Unknown" />)).toContain("نامشخص");
  });
});

describe("KpiCard", () => {
  it("renders a skeleton while loading", () => {
    const html = renderToStaticMarkup(<KpiCard label="کاربران" value="—" loading />);
    expect(html).toContain("adm-skeleton");
  });

  it("renders a safe error message with retry", () => {
    const error = new ApiClientError({ message: "boom", status: 500 });
    const html = renderToStaticMarkup(
      <KpiCard label="کاربران" value="—" error={error} onRetry={noop} />,
    );
    expect(html).toContain("خطای سرور");
    expect(html).toContain("تلاش مجدد");
    expect(html).not.toContain("boom");
  });

  it("renders the value and subtitle on success", () => {
    const html = renderToStaticMarkup(
      <KpiCard label="کاربران" value="۱۸٬۵۲۰" subtitle="۱۲٬۰۰۰ کاربر فعال" />,
    );
    expect(html).toContain("۱۸٬۵۲۰");
    expect(html).toContain("کاربر فعال");
  });
});

describe("WidgetCard state machine", () => {
  it("shows the loading skeleton", () => {
    const html = renderToStaticMarkup(
      <WidgetCard title="نمونه" loading>
        <div>content</div>
      </WidgetCard>,
    );
    expect(html).toContain("adm-skeleton");
    expect(html).not.toContain("content");
  });

  it("shows a safe error state with retry and no raw payload", () => {
    const error = new ApiClientError({ message: "secret detail", status: 500 });
    const html = renderToStaticMarkup(
      <WidgetCard title="نمونه" error={error} onRetry={noop}>
        <div>content</div>
      </WidgetCard>,
    );
    expect(html).toContain("تلاش مجدد");
    expect(html).not.toContain("secret detail");
    expect(html).not.toContain("content");
  });

  it("shows the empty state", () => {
    const html = renderToStaticMarkup(
      <WidgetCard title="نمونه" isEmpty emptyTitle="چیزی نیست">
        <div>content</div>
      </WidgetCard>,
    );
    expect(html).toContain("چیزی نیست");
    expect(html).not.toContain("content");
  });

  it("renders children on success", () => {
    const html = renderToStaticMarkup(
      <WidgetCard title="نمونه">
        <div>content-visible</div>
      </WidgetCard>,
    );
    expect(html).toContain("content-visible");
  });
});

describe("ContentPipelineCard", () => {
  it("renders the empty state when there is no content", () => {
    const html = renderToStaticMarkup(
      <ContentPipelineCard
        pipeline={section<ContentPipeline>({ data: { draft: 0, published: 0, total: 0 } })}
        onRetry={noop}
      />,
    );
    expect(html).toContain("محتوایی وجود ندارد");
  });

  it("renders draft and published rows on success", () => {
    const html = renderToStaticMarkup(
      <ContentPipelineCard
        pipeline={section<ContentPipeline>({ data: { draft: 3, published: 7, total: 10 } })}
        onRetry={noop}
      />,
    );
    expect(html).toContain("منتشرشده");
    expect(html).toContain("پیش‌نویس");
  });
});

describe("SystemHealthCard", () => {
  it("renders every mapped component label and the overall badge", () => {
    const health: SystemHealth = {
      overall: "Degraded",
      environment: "Development",
      version: "1.0.0.0",
      healthyCount: 5,
      totalCount: 6,
      components: [
        { key: "api", label: "API", status: "Healthy", summary: "" },
        { key: "database", label: "پایگاه داده", status: "Healthy", summary: "" },
        { key: "search", label: "جستجو", status: "Healthy", summary: "" },
        { key: "outbox", label: "Outbox", status: "Degraded", summary: "" },
        { key: "analytics", label: "تحلیل‌ها", status: "Healthy", summary: "" },
        { key: "audit", label: "Audit", status: "Healthy", summary: "" },
      ],
    };
    const html = renderToStaticMarkup(
      <SystemHealthCard health={section<SystemHealth>({ data: health })} onRetry={noop} />,
    );
    expect(html).toContain("پایگاه داده");
    expect(html).toContain("جستجو");
    expect(html).toContain("نیاز به بررسی");
  });
});
