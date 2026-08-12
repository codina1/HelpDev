import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import type { AdminMediaListItem } from "@/lib/admin/media/media-types";
import { MediaCard } from "@/components/admin/media/media-card";
import { MediaGrid } from "@/components/admin/media/media-grid";
import { MediaEmptyState } from "@/components/admin/media/media-empty-state";
import { MediaSkeleton } from "@/components/admin/media/media-skeleton";
import { MediaPagination } from "@/components/admin/media/media-pagination";
import { MediaDropzone } from "@/components/admin/media/media-dropzone";
import { MediaToolbar } from "@/components/admin/media/media-toolbar";

const noop = () => {};

const item: AdminMediaListItem = {
  id: "m1",
  originalFileName: "cover-photo.jpg",
  contentType: "image/jpeg",
  sizeBytes: 204800,
  width: 1200,
  height: 800,
  publicUrl: "/media/2026/07/m1.jpg",
  absoluteUrl: "http://localhost:5221/media/2026/07/m1.jpg",
  altText: "پوشش مقاله",
  uploadedByUserId: "u1",
  createdAtUtc: "2026-07-01T00:00:00Z",
  status: "Active",
};

describe("MediaCard", () => {
  it("renders the filename, type, size and dimensions but never a storage key", () => {
    const html = renderToStaticMarkup(<MediaCard item={item} onClick={noop} />);
    expect(html).toContain("cover-photo.jpg");
    expect(html).toContain("JPEG");
    expect(html).toContain("1200×800");
    expect(html).not.toMatch(/storageKey|filePath/i);
  });

  it("supports a custom action label (used by the picker)", () => {
    const html = renderToStaticMarkup(<MediaCard item={item} onClick={noop} actionLabel="انتخاب" />);
    expect(html).toContain("انتخاب");
  });

  it("never renders a delete action", () => {
    const html = renderToStaticMarkup(<MediaCard item={item} onClick={noop} />);
    expect(html).not.toContain("حذف");
  });
});

describe("MediaGrid", () => {
  it("renders one card per item", () => {
    const html = renderToStaticMarkup(
      <MediaGrid items={[item, { ...item, id: "m2", originalFileName: "b.png" }]} onItemClick={noop} />,
    );
    expect(html).toContain("cover-photo.jpg");
    expect(html).toContain("b.png");
  });
});

describe("MediaEmptyState", () => {
  it("distinguishes the global-empty and filtered-empty states", () => {
    expect(renderToStaticMarkup(<MediaEmptyState filtered={false} />)).toContain(
      "هنوز رسانه‌ای بارگذاری نشده است",
    );
    expect(renderToStaticMarkup(<MediaEmptyState filtered />)).toContain(
      "رسانه‌ای با عبارت جستجوی فعلی پیدا نشد",
    );
  });
});

describe("MediaSkeleton", () => {
  it("renders the requested number of placeholder cards", () => {
    const html = renderToStaticMarkup(<MediaSkeleton count={5} />);
    const matches = html.match(/adm-skeleton aspect-square/g) ?? [];
    expect(matches).toHaveLength(5);
  });
});

describe("MediaPagination", () => {
  it("reuses AdminPagination with the Media page-size set", () => {
    const html = renderToStaticMarkup(
      <MediaPagination
        page={1}
        pageSize={24}
        totalCount={48}
        totalPages={2}
        onPageChange={noop}
        onPageSizeChange={noop}
      />,
    );
    expect(html).toContain("صفحه قبل");
    expect(html).toContain("۲۴ در صفحه");
  });
});

describe("MediaDropzone", () => {
  it("only accepts JPEG/PNG/WebP — never SVG", () => {
    const html = renderToStaticMarkup(<MediaDropzone file={null} onFileSelected={noop} />);
    const acceptMatch = html.match(/accept="([^"]*)"/);
    expect(acceptMatch).not.toBeNull();
    const accept = acceptMatch?.[1] ?? "";
    expect(accept).toContain("image/jpeg");
    expect(accept).toContain("image/png");
    expect(accept).toContain("image/webp");
    expect(accept).not.toContain("svg");
  });

  it("shows the selected file name instead of a storage path", () => {
    const file = new File([new Uint8Array(4)], "logo.png", { type: "image/png" });
    const html = renderToStaticMarkup(<MediaDropzone file={file} onFileSelected={noop} />);
    expect(html).toContain("logo.png");
  });
});

describe("MediaToolbar", () => {
  it("renders a search box and an explicit upload trigger (never uploads on keystroke)", () => {
    const html = renderToStaticMarkup(
      <MediaToolbar search="" onSearchCommit={noop} onUploadClick={noop} />,
    );
    expect(html).toContain("بارگذاری رسانه");
    expect(html).toContain('type="search"');
  });
});
