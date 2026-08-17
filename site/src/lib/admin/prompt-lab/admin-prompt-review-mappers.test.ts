import { describe, expect, it } from "vitest";
import type { AdminPromptReviewPageDto } from "@/lib/api/promptlab-admin-review";
import { mapAdminPromptReviewPage } from "./admin-prompt-review-mappers";

describe("admin-prompt-review-mappers", () => {
  it("maps pending list fields used by the review table", () => {
    const raw: AdminPromptReviewPageDto = {
      page: 1,
      pageSize: 20,
      total: 1,
      items: [
        {
          id: "p1",
          title: "بازبینی مرز سیستم",
          slug: "system-boundary-review",
          authorId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          categoryId: "c1",
          categoryName: "Coding",
          preview: "You are a reviewer…",
          status: "Submitted",
          rejectionReason: null,
          createdAt: "2026-08-17T10:00:00Z",
          updatedAt: "2026-08-17T10:00:00Z",
          publishedAt: null,
        },
      ],
    };

    const page = mapAdminPromptReviewPage(raw);
    expect(page.totalCount).toBe(1);
    expect(page.items[0]).toMatchObject({
      title: "بازبینی مرز سیستم",
      authorId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
      categoryName: "Coding",
      preview: "You are a reviewer…",
      status: "Submitted",
    });
  });
});
