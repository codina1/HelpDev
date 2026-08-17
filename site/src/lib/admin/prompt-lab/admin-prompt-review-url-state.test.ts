import { describe, expect, it } from "vitest";
import {
  buildAdminPromptReviewHref,
  mergeAdminPromptReviewQuery,
  parseAdminPromptReviewQuery,
} from "./admin-prompt-review-url-state";
import { DEFAULT_ADMIN_PROMPT_REVIEW_QUERY } from "./admin-prompt-review-types";

describe("admin-prompt-review-url-state", () => {
  it("parses tab defaults and published/rejected filters", () => {
    expect(parseAdminPromptReviewQuery(null)).toEqual(DEFAULT_ADMIN_PROMPT_REVIEW_QUERY);

    expect(parseAdminPromptReviewQuery(new URLSearchParams("tab=published&page=2"))).toEqual({
      tab: "published",
      page: 2,
      pageSize: DEFAULT_ADMIN_PROMPT_REVIEW_QUERY.pageSize,
    });

    expect(parseAdminPromptReviewQuery(new URLSearchParams("tab=rejected"))).toEqual({
      tab: "rejected",
      page: 1,
      pageSize: DEFAULT_ADMIN_PROMPT_REVIEW_QUERY.pageSize,
    });
  });

  it("builds hrefs and resets page when the tab changes", () => {
    expect(buildAdminPromptReviewHref(DEFAULT_ADMIN_PROMPT_REVIEW_QUERY)).toBe("/admin/prompts");
    expect(buildAdminPromptReviewHref({ ...DEFAULT_ADMIN_PROMPT_REVIEW_QUERY, tab: "published" })).toBe(
      "/admin/prompts?tab=published",
    );

    const merged = mergeAdminPromptReviewQuery(
      { tab: "pending", page: 3, pageSize: 20 },
      { tab: "rejected" },
    );
    expect(merged.tab).toBe("rejected");
    expect(merged.page).toBe(1);
  });
});
