import { describe, expect, it } from "vitest";
import { validateAltText, validateCaption, validateMediaFile } from "./media-validation";
import { MAX_MEDIA_UPLOAD_SIZE_BYTES } from "./media-types";

function makeFile(
  name: string,
  type: string,
  size: number,
): File {
  const file = new File([new Uint8Array(Math.max(size, 0))], name, { type });
  // jsdom computes `size` from the blob content; override defensively for the
  // "empty file" case where we still want a real File instance.
  Object.defineProperty(file, "size", { value: size });
  return file;
}

describe("validateMediaFile", () => {
  it("rejects a missing file", () => {
    const result = validateMediaFile(null);
    expect(result.valid).toBe(false);
  });

  it("rejects an empty file", () => {
    const result = validateMediaFile(makeFile("empty.png", "image/png", 0));
    expect(result.valid).toBe(false);
  });

  it("rejects a file over the size limit", () => {
    const result = validateMediaFile(
      makeFile("big.png", "image/png", MAX_MEDIA_UPLOAD_SIZE_BYTES + 1),
    );
    expect(result.valid).toBe(false);
    if (!result.valid) expect(result.error).toMatch(/۵ مگابایت/);
  });

  it("accepts a file exactly at the size limit", () => {
    const result = validateMediaFile(
      makeFile("edge.png", "image/png", MAX_MEDIA_UPLOAD_SIZE_BYTES),
    );
    expect(result.valid).toBe(true);
  });

  it("rejects SVG by MIME type even with a misleading extension", () => {
    const result = validateMediaFile(makeFile("cover.png", "image/svg+xml", 1024));
    expect(result.valid).toBe(false);
    if (!result.valid) expect(result.error).toMatch(/SVG/);
  });

  it("rejects SVG by file extension even with a misleading MIME type", () => {
    const result = validateMediaFile(makeFile("cover.svg", "image/png", 1024));
    expect(result.valid).toBe(false);
    if (!result.valid) expect(result.error).toMatch(/SVG/);
  });

  it("rejects unsupported MIME types (e.g. GIF, PDF)", () => {
    expect(validateMediaFile(makeFile("a.gif", "image/gif", 1024)).valid).toBe(false);
    expect(validateMediaFile(makeFile("a.pdf", "application/pdf", 1024)).valid).toBe(false);
  });

  it("accepts JPEG, PNG and WebP", () => {
    expect(validateMediaFile(makeFile("a.jpg", "image/jpeg", 1024)).valid).toBe(true);
    expect(validateMediaFile(makeFile("a.png", "image/png", 1024)).valid).toBe(true);
    expect(validateMediaFile(makeFile("a.webp", "image/webp", 1024)).valid).toBe(true);
  });
});

describe("validateAltText / validateCaption", () => {
  it("passes short values", () => {
    expect(validateAltText("لوگو")).toBeNull();
    expect(validateCaption("توضیح کوتاه")).toBeNull();
  });

  it("flags over-long alt text and caption", () => {
    expect(validateAltText("a".repeat(301))).toBeTruthy();
    expect(validateCaption("a".repeat(501))).toBeTruthy();
  });
});
