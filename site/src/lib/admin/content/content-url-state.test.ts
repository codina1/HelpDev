import { describe, expect, it } from "vitest";
import {
  adminContentListQueryKey,
  buildAdminContentListHref,
  isAdminContentListFiltered,
  mergeAdminContentListQuery,
  parseAdminContentListQuery,
  serializeAdminContentListQuery,
} from "./content-url-state";
import { DEFAULT_ADMIN_CONTENT_LIST_QUERY } from "./content-types";

describe("parseAdminContentListQuery", () => {
  it("applies defaults for empty params", () => {
    expect(parseAdminContentListQuery(new URLSearchParams())).toEqual(
      DEFAULT_ADMIN_CONTENT_LIST_QUERY,
    );
  });

  it("parses a full valid query", () => {
    const params = new URLSearchParams(
      "page=2&pageSize=50&search=cursor&status=Draft&type=Article",
    );
    expect(parseAdminContentListQuery(params)).toEqual({
      page: 2,
      pageSize: 50,
      search: "cursor",
      status: "Draft",
      type: "Article",
    });
  });

  it("falls back invalid page to 1", () => {
    expect(parseAdminContentListQuery(new URLSearchParams("page=0")).page).toBe(1);
    expect(parseAdminContentListQuery(new URLSearchParams("page=-3")).page).toBe(1);
    expect(parseAdminContentListQuery(new URLSearchParams("page=abc")).page).toBe(1);
  });

  it("falls back unsupported pageSize to the default", () => {
    expect(parseAdminContentListQuery(new URLSearchParams("pageSize=15")).pageSize).toBe(20);
    expect(parseAdminContentListQuery(new URLSearchParams("pageSize=1000")).pageSize).toBe(20);
  });

  it("omits unsupported status/type enums", () => {
    const query = parseAdminContentListQuery(
      new URLSearchParams("status=Deleted&type=Podcast"),
    );
    expect(query.status).toBe("all");
    expect(query.type).toBe("all");
  });

  it("trims search whitespace", () => {
    expect(parseAdminContentListQuery(new URLSearchParams("search=%20hello%20")).search).toBe(
      "hello",
    );
  });

  it("accepts case-insensitive status/type", () => {
    const query = parseAdminContentListQuery(
      new URLSearchParams("status=published&type=news"),
    );
    expect(query.status).toBe("Published");
    expect(query.type).toBe("News");
  });
});

describe("serializeAdminContentListQuery", () => {
  it("omits defaults and empty filters", () => {
    const params = serializeAdminContentListQuery(DEFAULT_ADMIN_CONTENT_LIST_QUERY);
    expect(params.toString()).toBe("");
  });

  it("serializes only non-default fields in deterministic order", () => {
    const params = serializeAdminContentListQuery({
      page: 3,
      pageSize: 50,
      search: "cursor",
      status: "Draft",
      type: "Article",
    });
    expect(params.toString()).toBe(
      "page=3&pageSize=50&search=cursor&status=Draft&type=Article",
    );
  });

  it("round-trips with parse", () => {
    const original = {
      page: 2,
      pageSize: 10 as const,
      search: "help",
      status: "Published" as const,
      type: "Tool" as const,
    };
    const roundTrip = parseAdminContentListQuery(serializeAdminContentListQuery(original));
    expect(roundTrip).toEqual(original);
  });
});

describe("mergeAdminContentListQuery", () => {
  it("resets page to 1 when filters or pageSize change", () => {
    const current = { ...DEFAULT_ADMIN_CONTENT_LIST_QUERY, page: 4 };
    expect(mergeAdminContentListQuery(current, { status: "Draft" }).page).toBe(1);
    expect(mergeAdminContentListQuery(current, { type: "News" }).page).toBe(1);
    expect(mergeAdminContentListQuery(current, { search: "x" }).page).toBe(1);
    expect(mergeAdminContentListQuery(current, { pageSize: 50 }).page).toBe(1);
  });

  it("preserves page when only page changes", () => {
    const current = { ...DEFAULT_ADMIN_CONTENT_LIST_QUERY, page: 2 };
    expect(mergeAdminContentListQuery(current, { page: 5 }).page).toBe(5);
  });
});

describe("buildAdminContentListHref / helpers", () => {
  it("builds a bare path when there are no params", () => {
    expect(buildAdminContentListHref(DEFAULT_ADMIN_CONTENT_LIST_QUERY)).toBe("/admin/content");
  });

  it("detects filtered state", () => {
    expect(isAdminContentListFiltered(DEFAULT_ADMIN_CONTENT_LIST_QUERY)).toBe(false);
    expect(
      isAdminContentListFiltered({ ...DEFAULT_ADMIN_CONTENT_LIST_QUERY, status: "Draft" }),
    ).toBe(true);
  });

  it("builds a stable query key", () => {
    expect(
      adminContentListQueryKey({
        page: 1,
        pageSize: 20,
        search: "  Cursor ",
        status: "all",
        type: "Article",
      }),
    ).toBe("1|20|cursor|all|Article");
  });
});
