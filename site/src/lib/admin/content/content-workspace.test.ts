import { describe, expect, it } from "vitest";
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { CONTENT_CAPABILITIES, toAdminContentListOptions } from "./content-api";
import { DEFAULT_ADMIN_CONTENT_LIST_QUERY } from "./content-types";

const API_FILE = join(process.cwd(), "src/lib/admin/content/content-api.ts");
const RESOURCE_FILE = join(process.cwd(), "src/lib/api/content.ts");
const DASHBOARD_FILE = join(
  process.cwd(),
  "src/components/admin/content/content-dashboard.tsx",
);
const HOOKS_FILE = join(process.cwd(), "src/lib/admin/content/content-hooks.ts");
const CONTENT_DIRS = [
  join(process.cwd(), "src/lib/admin/content"),
  join(process.cwd(), "src/components/admin/content"),
];

function collect(dir: string, acc: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) {
      collect(full, acc);
      continue;
    }
    if (!/\.(ts|tsx)$/.test(entry)) continue;
    if (/\.test\.(ts|tsx)$/.test(entry)) continue;
    acc.push(full);
  }
  return acc;
}

describe("content API adapter — admin list", () => {
  it("advertises adminList capability", () => {
    expect(CONTENT_CAPABILITIES.adminList).toBe(true);
    expect(CONTENT_CAPABILITIES.list).toBe(true);
    expect(CONTENT_CAPABILITIES.getById).toBe(true);
  });

  it("maps workspace query to API options and omits empty filters", () => {
    expect(toAdminContentListOptions(DEFAULT_ADMIN_CONTENT_LIST_QUERY)).toEqual({
      page: 1,
      pageSize: 20,
      search: undefined,
      status: undefined,
      type: undefined,
    });

    expect(
      toAdminContentListOptions({
        page: 2,
        pageSize: 50,
        search: "  cursor  ",
        status: "Draft",
        type: "Article",
      }),
    ).toEqual({
      page: 2,
      pageSize: 50,
      search: "cursor",
      status: "Draft",
      type: "Article",
    });
  });
});

describe("content resource — admin list endpoint", () => {
  const source = readFileSync(RESOURCE_FILE, "utf8");

  it("lists via GET /admin/content with shared apiRequest", () => {
    expect(source).toContain("getAdminContentList");
    expect(source).toContain('path: "/admin/content"');
    expect(source).toContain("query");
  });

  it("lists via GET /admin/content and omits empty filters in getAdminContentList", () => {
    expect(source).toContain("getAdminContentList");
    expect(source).toContain('path: "/admin/content"');
    expect(source).toContain("if (options.search) query.search = options.search");
    expect(source).toContain("if (options.status) query.status = options.status");
    expect(source).toContain("if (options.type) query.type = options.type");
    expect(source).toContain("signal");
  });
});

describe("Admin workspace must not use the public list API", () => {
  it("dashboard does not import or call fetchContentList / listPublishedContent / useContentList", () => {
    const dashboard = readFileSync(DASHBOARD_FILE, "utf8");
    expect(dashboard).toContain("useAdminContentList");
    expect(dashboard).not.toContain("useContentList");
    expect(dashboard).not.toContain("fetchContentList");
    expect(dashboard).not.toContain("listPublishedContent");
    expect(dashboard).not.toMatch(/["'`]\/content["'`]/);
  });

  it("hooks workspace list uses fetchAdminContentList", () => {
    const hooks = readFileSync(HOOKS_FILE, "utf8");
    expect(hooks).toContain("fetchAdminContentList");
    expect(hooks).toContain("useAdminContentList");
  });

  it("admin list adapter targets /admin/content", () => {
    const api = readFileSync(API_FILE, "utf8");
    expect(api).toContain("fetchAdminContentList");
    expect(api).toContain("getAdminContentListRequest");
  });
});

describe("workspace honesty guardrails", () => {
  it("does not invent unsupported statuses or SEO scores", () => {
    const forbidden = [
      /\bDeleted\b/,
      /\bseoscore\b/i,
      /\bseo_score\b/i,
      /\bcontentscore\b/i,
    ];
    const offenders: string[] = [];
    for (const dir of CONTENT_DIRS) {
      for (const file of collect(dir)) {
        if (file.endsWith("content-url-state.ts")) continue;
        const text = readFileSync(file, "utf8");
        for (const re of forbidden) {
          if (re.test(text)) offenders.push(`${file} -> ${re}`);
        }
      }
    }
    expect(offenders, offenders.join("\n")).toHaveLength(0);
  });

  it("does not compute global totals from page items in the dashboard", () => {
    const dashboard = readFileSync(DASHBOARD_FILE, "utf8");
    expect(dashboard).not.toMatch(/items\.filter\s*\(\s*.*Draft/);
    expect(dashboard).not.toMatch(/items\.filter\s*\(\s*.*Published/);
    expect(dashboard).toContain("useContentStats");
    expect(dashboard).toContain("totalCount");
  });

  it("admin action links use content id, not slug", () => {
    const actions = readFileSync(
      join(process.cwd(), "src/components/admin/content/shared/content-actions.tsx"),
      "utf8",
    );
    expect(actions).toContain("encodeURIComponent(id)");
    expect(actions).not.toContain("encodeURIComponent(slug)");
  });
});
