import { describe, expect, it } from "vitest";
import {
  hasWriterPromptFormErrors,
  labelForWriterPromptStatus,
  mapWriterPromptListItem,
  mapWriterPromptPagedResult,
  validateWriterPromptForm,
} from "./writer-prompt-mappers";

describe("writer-prompt-mappers", () => {
  it("maps Persian status labels", () => {
    expect(labelForWriterPromptStatus("Draft")).toBe("پیش‌نویس");
    expect(labelForWriterPromptStatus("Submitted")).toBe("در انتظار بررسی");
    expect(labelForWriterPromptStatus("Approved")).toBe("منتشرشده");
    expect(labelForWriterPromptStatus("Rejected")).toBe("ردشده");
  });

  it("maps list items and paged results", () => {
    const item = mapWriterPromptListItem({
      id: "a",
      title: "T",
      slug: "t",
      description: null,
      coverImage: null,
      mediaType: "Text",
      categoryId: "c",
      aiModelId: "m",
      status: "Submitted",
      views: 3,
      copyCount: 7,
      createdAt: "2026-07-01T00:00:00Z",
      updatedAt: "2026-07-02T00:00:00Z",
      publishedAt: null,
    });

    expect(item.statusLabel).toBe("در انتظار بررسی");
    expect(item.copyCount).toBe(7);

    const page = mapWriterPromptPagedResult({
      page: 1,
      pageSize: 20,
      total: 41,
      items: [item],
    });

    expect(page.totalCount).toBe(41);
    expect(page.totalPages).toBe(3);
    expect(page.items).toHaveLength(1);
  });

  it("validates required writer prompt fields", () => {
    const errors = validateWriterPromptForm({
      title: "",
      slug: "",
      description: "",
      coverImage: "",
      content: "",
      aiModelId: "",
      categoryId: "",
      mediaType: "Text",
      tags: "",
    });
    expect(errors.title).toBeTruthy();
    expect(errors.slug).toBeTruthy();
    expect(errors.content).toBeTruthy();
    expect(errors.aiModelId).toBeTruthy();
    expect(errors.categoryId).toBeTruthy();
  });

  it("accepts a complete writer prompt form", () => {
    const errors = validateWriterPromptForm({
      title: "بازبینی مرز سیستم",
      slug: "system-boundary-review",
      description: "توضیح",
      coverImage: "",
      content: "You are a reviewer.",
      aiModelId: "model-1",
      categoryId: "cat-1",
      mediaType: "Text",
      tags: "کدنویسی، معماری",
    });
    expect(hasWriterPromptFormErrors(errors)).toBe(false);
  });
});
