import { describe, expect, it } from "vitest";
import {
  adminMediaListQueryKey,
  buildAdminMediaListHref,
  isAdminMediaListFiltered,
  mergeAdminMediaListQuery,
  parseAdminMediaListQuery,
  serializeAdminMediaListQuery,
} from "./media-url-state";
import { DEFAULT_ADMIN_MEDIA_LIST_QUERY } from "./media-types";

describe("parseAdminMediaListQuery", () => {
  it("applies defaults for empty params", () => {
    expect(parseAdminMediaListQuery(new URLSearchParams())).toEqual(
      DEFAULT_ADMIN_MEDIA_LIST_QUERY,
    );
  });

  it("parses a full valid query", () => {
    const params = new URLSearchParams("page=2&pageSize=48&search=cover");
    expect(parseAdminMediaListQuery(params)).toEqual({
      page: 2,
      pageSize: 48,
      search: "cover",
    });
  });

  it("falls back invalid page to 1", () => {
    expect(parseAdminMediaListQuery(new URLSearchParams("page=0")).page).toBe(1);
    expect(parseAdminMediaListQuery(new URLSearchParams("page=-3")).page).toBe(1);
    expect(parseAdminMediaListQuery(new URLSearchParams("page=abc")).page).toBe(1);
  });

  it("falls back unsupported pageSize to the default", () => {
    expect(parseAdminMediaListQuery(new URLSearchParams("pageSize=15")).pageSize).toBe(24);
    expect(parseAdminMediaListQuery(new URLSearchParams("pageSize=1000")).pageSize).toBe(24);
  });

  it("trims search whitespace", () => {
    expect(parseAdminMediaListQuery(new URLSearchParams("search=%20cover%20")).search).toBe(
      "cover",
    );
  });
});

describe("serializeAdminMediaListQuery", () => {
  it("omits defaults and empty filters", () => {
    const params = serializeAdminMediaListQuery(DEFAULT_ADMIN_MEDIA_LIST_QUERY);
    expect(params.toString()).toBe("");
  });

  it("serializes only non-default fields in deterministic order", () => {
    const params = serializeAdminMediaListQuery({ page: 3, pageSize: 48, search: "cover" });
    expect(params.toString()).toBe("page=3&pageSize=48&search=cover");
  });

  it("round-trips with parse", () => {
    const original = { page: 2, pageSize: 12 as const, search: "hero" };
    const roundTrip = parseAdminMediaListQuery(serializeAdminMediaListQuery(original));
    expect(roundTrip).toEqual(original);
  });
});

describe("mergeAdminMediaListQuery", () => {
  it("resets page to 1 when search or pageSize changes", () => {
    const current = { ...DEFAULT_ADMIN_MEDIA_LIST_QUERY, page: 4 };
    expect(mergeAdminMediaListQuery(current, { search: "x" }).page).toBe(1);
    expect(mergeAdminMediaListQuery(current, { pageSize: 48 }).page).toBe(1);
  });

  it("preserves page when only page changes", () => {
    const current = { ...DEFAULT_ADMIN_MEDIA_LIST_QUERY, page: 2 };
    expect(mergeAdminMediaListQuery(current, { page: 5 }).page).toBe(5);
  });

  it("falls back an out-of-range pageSize to the default", () => {
    const current = { ...DEFAULT_ADMIN_MEDIA_LIST_QUERY };
    expect(
      mergeAdminMediaListQuery(current, { pageSize: 999 as unknown as 24 }).pageSize,
    ).toBe(24);
  });
});

describe("buildAdminMediaListHref / helpers", () => {
  it("builds a bare path when there are no params", () => {
    expect(buildAdminMediaListHref(DEFAULT_ADMIN_MEDIA_LIST_QUERY)).toBe("/admin/media");
  });

  it("detects filtered state", () => {
    expect(isAdminMediaListFiltered(DEFAULT_ADMIN_MEDIA_LIST_QUERY)).toBe(false);
    expect(
      isAdminMediaListFiltered({ ...DEFAULT_ADMIN_MEDIA_LIST_QUERY, search: "logo" }),
    ).toBe(true);
  });

  it("builds a stable query key", () => {
    expect(
      adminMediaListQueryKey({ page: 1, pageSize: 24, search: "  Cover " }),
    ).toBe("1|24|cover");
  });
});
