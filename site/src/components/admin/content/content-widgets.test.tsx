import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ApiClientError } from "@/lib/api/errors";
import type { AdminContentListItem } from "@/lib/admin/content/content-types";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import { ContentTypeBadge } from "@/components/admin/content/shared/content-type-badge";
import { ContentTable } from "@/components/admin/content/list/content-table";
import { ContentEmptyState } from "@/components/admin/content/list/content-empty-state";
import { ContentActions } from "@/components/admin/content/shared/content-actions";
import { PublishPanel } from "@/components/admin/content/editor/publish-panel";
import { MarkdownPreview } from "@/components/admin/content/shared/markdown-preview";
import { AdminPagination } from "@/components/admin/shared/admin-pagination";

const noop = () => {};

const draftItem: AdminContentListItem = {
  id: "draft-1",
  title: "پیش‌نویس نمونه",
  slug: "draft-sample",
  type: "Article",
  typeLabel: "مقاله",
  authorId: "11111111-2222-3333-4444-555555555555",
  createdAtUtc: "2026-07-01T00:00:00Z",
  updatedAtUtc: "2026-07-02T00:00:00Z",
  publishedAtUtc: null,
  status: "Draft",
  statusLabel: "پیش‌نویس",
};

const publishedItem: AdminContentListItem = {
  ...draftItem,
  id: "pub-1",
  title: "عنوان نمونه",
  slug: "sample-slug",
  status: "Published",
  statusLabel: "منتشرشده",
  publishedAtUtc: "2026-07-03T00:00:00Z",
};

describe("content badges", () => {
  it("renders status labels", () => {
    expect(renderToStaticMarkup(<ContentStatusBadge status="Published" />)).toContain("منتشرشده");
    expect(renderToStaticMarkup(<ContentStatusBadge status="Draft" />)).toContain("پیش‌نویس");
  });

  it("renders type labels", () => {
    expect(renderToStaticMarkup(<ContentTypeBadge type="News" />)).toContain("خبر");
  });
});

describe("ContentTable", () => {
  it("renders Draft and Published rows with id-based actions", () => {
    const html = renderToStaticMarkup(
      <ContentTable items={[draftItem, publishedItem]} onPublish={noop} />,
    );
    expect(html).toContain("پیش‌نویس نمونه");
    expect(html).toContain("عنوان نمونه");
    expect(html).toContain("sample-slug");
    expect(html).toContain("مقاله");
    expect(html).toContain("مشاهده");
    expect(html).toContain("ویرایش");
    expect(html).toContain("/admin/content/draft-1");
    expect(html).toContain("/admin/content/pub-1/edit");
    expect(html).toContain("انتشار");
  });
});

describe("ContentActions", () => {
  it("shows publish only for Draft and uses id routes", () => {
    const draftHtml = renderToStaticMarkup(
      <ContentActions id="d1" title="T" status="Draft" onPublish={noop} />,
    );
    expect(draftHtml).toContain("انتشار");
    expect(draftHtml).toContain("/admin/content/d1");
    expect(draftHtml).not.toContain("encodeURIComponent");

    const pubHtml = renderToStaticMarkup(
      <ContentActions id="p1" title="T" status="Published" onPublish={noop} />,
    );
    expect(pubHtml).not.toContain("انتشار");
  });
});

describe("ContentEmptyState", () => {
  it("distinguishes empty, filtered-empty, and writer-scoped empty", () => {
    expect(renderToStaticMarkup(<ContentEmptyState filtered={false} />)).toContain(
      "هنوز محتوایی ایجاد نشده است",
    );
    expect(renderToStaticMarkup(<ContentEmptyState filtered />)).toContain(
      "محتوایی با فیلترهای انتخاب‌شده پیدا نشد",
    );
    expect(renderToStaticMarkup(<ContentEmptyState filtered={false} writerScoped />)).toContain(
      "هنوز محتوایی برای این حساب ثبت نشده است",
    );
  });
});

describe("AdminPagination", () => {
  it("disables previous on first page and next on last page", () => {
    const first = renderToStaticMarkup(
      <AdminPagination
        page={1}
        pageSize={20}
        totalCount={40}
        totalPages={2}
        onPageChange={noop}
        onPageSizeChange={noop}
      />,
    );
    expect(first).toContain("disabled");
    expect(first).toContain("صفحه قبل");

    const last = renderToStaticMarkup(
      <AdminPagination
        page={2}
        pageSize={20}
        totalCount={40}
        totalPages={2}
        onPageChange={noop}
        onPageSizeChange={noop}
      />,
    );
    expect(last).toContain("صفحه بعد");
  });
});

describe("PublishPanel", () => {
  it("shows the disabled reason and does not leak raw errors in edit mode", () => {
    const error = new ApiClientError({ message: "raw-secret", status: 500 });
    const html = renderToStaticMarkup(
      <PublishPanel
        status="Draft"
        submitting={false}
        canMutate={false}
        disabledReason="ذخیره ویرایش هنوز پشتیبانی نمی‌شود"
        error={error}
        onSaveDraft={noop}
        onPublish={noop}
      />,
    );
    expect(html).toContain("این محتوا هنوز منتشر نشده است");
    expect(html).toContain("ذخیره ویرایش هنوز پشتیبانی نمی‌شود");
    expect(html).not.toContain("raw-secret");
    expect(html).toContain("disabled");
  });

  it("shows the published status message", () => {
    const html = renderToStaticMarkup(
      <PublishPanel
        status="Published"
        submitting={false}
        canMutate
        onSaveDraft={noop}
        onPublish={noop}
      />,
    );
    expect(html).toContain("این محتوا منتشر شده است");
  });
});

describe("MarkdownPreview", () => {
  it("renders formatting and escapes raw HTML (no XSS)", () => {
    const html = renderToStaticMarkup(
      <MarkdownPreview source={"**bold** and <script>alert(1)</script>"} />,
    );
    expect(html).toContain("<strong");
    expect(html).not.toContain("<script>");
  });
});
