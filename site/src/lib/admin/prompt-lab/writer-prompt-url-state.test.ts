import { describe, expect, it } from "vitest";
import {
  buildWriterPromptListHref,
  isWriterPromptListFiltered,
  mergeWriterPromptListQuery,
  parseWriterPromptListQuery,
} from "./writer-prompt-url-state";
import { DEFAULT_WRITER_PROMPT_LIST_QUERY } from "./writer-prompt-types";

describe("writer-prompt-url-state", () => {
  it("parses defaults and filters", () => {
    expect(parseWriterPromptListQuery(null)).toEqual(DEFAULT_WRITER_PROMPT_LIST_QUERY);

    const params = new URLSearchParams("page=2&pageSize=50&status=Draft");
    expect(parseWriterPromptListQuery(params)).toEqual({
      page: 2,
      pageSize: 50,
      status: "Draft",
    });
  });

  it("builds hrefs and merges filter resets", () => {
    expect(buildWriterPromptListHref(DEFAULT_WRITER_PROMPT_LIST_QUERY)).toBe("/admin/prompt-lab");

    const filtered = { ...DEFAULT_WRITER_PROMPT_LIST_QUERY, status: "Submitted" as const, page: 3 };
    expect(buildWriterPromptListHref(filtered)).toContain("status=Submitted");
    expect(buildWriterPromptListHref(filtered)).toContain("page=3");

    const merged = mergeWriterPromptListQuery(
      { page: 3, pageSize: 20, status: "all" },
      { status: "Approved" },
    );
    expect(merged.page).toBe(1);
    expect(merged.status).toBe("Approved");
  });

  it("detects active filters", () => {
    expect(isWriterPromptListFiltered(DEFAULT_WRITER_PROMPT_LIST_QUERY)).toBe(false);
    expect(
      isWriterPromptListFiltered({ ...DEFAULT_WRITER_PROMPT_LIST_QUERY, status: "Rejected" }),
    ).toBe(true);
  });
});
