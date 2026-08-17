import { describe, expect, it } from "vitest";
import {
  labelForWriterPromptStatus,
  mapWriterPromptListItem,
  mapWriterPromptPagedResult,
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
});
