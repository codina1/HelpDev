import { describe, expect, it } from "vitest";
import {
  formatFileSize,
  labelForMediaContentType,
  mapAdminMediaDetail,
  mapAdminMediaListItem,
  mapAdminMediaPagedResult,
  resolveMediaUrl,
  toMediaPickerSelection,
} from "./media-mappers";
import type { MediaAssetRawDto, MediaAssetListItemRawDto } from "./media-types";

describe("resolveMediaUrl", () => {
  it("resolves a relative publicUrl against the API origin (not the /api/v1 base)", () => {
    const resolved = resolveMediaUrl("/media/2026/07/abc.jpg");
    expect(resolved).not.toContain("/api/v1");
    expect(resolved).toMatch(/\/media\/2026\/07\/abc\.jpg$/);
  });

  it("leaves an already-absolute URL untouched", () => {
    expect(resolveMediaUrl("https://cdn.example.com/x.jpg")).toBe("https://cdn.example.com/x.jpg");
  });

  it("returns an empty string for an empty input", () => {
    expect(resolveMediaUrl("")).toBe("");
  });
});

describe("formatFileSize", () => {
  it("formats bytes, kilobytes and megabytes in Persian units", () => {
    expect(formatFileSize(500)).toMatch(/بایت/);
    expect(formatFileSize(2048)).toMatch(/کیلوبایت/);
    expect(formatFileSize(5 * 1024 * 1024)).toMatch(/مگابایت/);
  });

  it("returns an em dash for invalid input", () => {
    expect(formatFileSize(-1)).toBe("—");
    expect(formatFileSize(Number.NaN)).toBe("—");
  });
});

describe("labelForMediaContentType", () => {
  it("labels known content types and falls back to the raw value", () => {
    expect(labelForMediaContentType("image/jpeg")).toBe("JPEG");
    expect(labelForMediaContentType("image/png")).toBe("PNG");
    expect(labelForMediaContentType("image/webp")).toBe("WebP");
    expect(labelForMediaContentType("image/gif")).toBe("image/gif");
  });
});

const listItemDto: MediaAssetListItemRawDto = {
  id: "m1",
  originalFileName: "cover.jpg",
  contentType: "image/jpeg",
  sizeBytes: 12345,
  width: 800,
  height: 600,
  publicUrl: "/media/2026/07/m1.jpg",
  altText: null,
  uploadedByUserId: "u1",
  createdAtUtc: "2026-07-01T00:00:00Z",
  status: "Active",
};

describe("mapAdminMediaListItem", () => {
  it("maps null altText to an empty string and resolves the URL", () => {
    const item = mapAdminMediaListItem(listItemDto);
    expect(item.altText).toBe("");
    expect(item.publicUrl).toBe("/media/2026/07/m1.jpg");
    expect(item.absoluteUrl).toContain("/media/2026/07/m1.jpg");
    expect(item.width).toBe(800);
    expect(item.height).toBe(600);
  });

  it("never exposes a storage key or filesystem path field", () => {
    const item = mapAdminMediaListItem(listItemDto);
    expect(Object.keys(item)).not.toContain("storageKey");
    expect(Object.keys(item)).not.toContain("filePath");
  });
});

describe("mapAdminMediaPagedResult", () => {
  it("maps items and computes totalPages when missing", () => {
    const page = mapAdminMediaPagedResult({
      items: [listItemDto],
      page: 1,
      pageSize: 24,
      totalCount: 50,
      totalPages: undefined as unknown as number,
    });
    expect(page.items).toHaveLength(1);
    expect(page.totalPages).toBe(3);
  });
});

describe("mapAdminMediaDetail", () => {
  const detailDto: MediaAssetRawDto = {
    ...listItemDto,
    caption: null,
    updatedAtUtc: "2026-07-02T00:00:00Z",
  };

  it("coerces null caption to an empty string", () => {
    const detail = mapAdminMediaDetail(detailDto);
    expect(detail.caption).toBe("");
    expect(detail.updatedAtUtc).toBe("2026-07-02T00:00:00Z");
  });
});

describe("toMediaPickerSelection", () => {
  it("returns only picker-safe fields (id, publicUrl, absoluteUrl, altText, width, height)", () => {
    const item = mapAdminMediaListItem(listItemDto);
    const selection = toMediaPickerSelection(item);
    expect(selection).toEqual({
      id: "m1",
      publicUrl: "/media/2026/07/m1.jpg",
      absoluteUrl: item.absoluteUrl,
      altText: "",
      width: 800,
      height: 600,
    });
  });
});
