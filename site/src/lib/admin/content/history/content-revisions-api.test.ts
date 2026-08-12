import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  CONTENT_REVISION_CAPABILITIES,
  fetchContentRevisionDetail,
  fetchContentRevisions,
  restoreContentRevisionItem,
  toContentRevisionListOptions,
} from "./history-api";
import { DEFAULT_CONTENT_REVISION_LIST_QUERY } from "./history-types";
import { ApiClientError } from "@/lib/api/errors";

const API_FILE = join(process.cwd(), "src/lib/admin/content/history/history-api.ts");
const RESOURCE_FILE = join(process.cwd(), "src/lib/api/content.ts");
const HISTORY_DIRS = [
  join(process.cwd(), "src/lib/admin/content/history"),
  join(process.cwd(), "src/components/admin/content/history"),
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

describe("content revision API adapter", () => {
  it("reuses the shared client module and does not duplicate fetch logic", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain('from "@/lib/api/content"');
    expect(content).not.toMatch(/\bfetch\s*\(/);
    expect(content).not.toMatch(/["'`]\/api\/(?!v1)/);
  });

  it("advertises revision capabilities", () => {
    expect(CONTENT_REVISION_CAPABILITIES.list).toBe(true);
    expect(CONTENT_REVISION_CAPABILITIES.detail).toBe(true);
    expect(CONTENT_REVISION_CAPABILITIES.restore).toBe(true);
  });

  it("maps workspace query to API options", () => {
    expect(toContentRevisionListOptions(DEFAULT_CONTENT_REVISION_LIST_QUERY)).toEqual({
      page: 1,
      pageSize: 10,
    });
  });
});

describe("content revision resource endpoints", () => {
  const source = readFileSync(RESOURCE_FILE, "utf8");

  it("lists via GET /admin/content/{id}/revisions", () => {
    expect(source).toContain("getContentRevisions");
    expect(source).toContain("/revisions");
  });

  it("reads detail via GET /admin/content/{id}/revisions/{version}", () => {
    expect(source).toContain("getContentRevision");
    expect(source).toContain("/revisions/${version}`");
  });

  it("restores via POST /admin/content/{id}/revisions/{version}/restore", () => {
    expect(source).toContain("restoreContentRevision");
    expect(source).toContain("/restore");
  });
});

describe("revision HTTP integration (mocked fetch)", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  function jsonResponse(body: unknown, status = 200): Response {
    return new Response(JSON.stringify(body), {
      status,
      headers: new Headers({ "content-type": "application/json" }),
    });
  }

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("GETs revisions with paging query params", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        items: [{ versionNumber: 2, createdByUserId: "u1", createdAtUtc: "2026-07-01T00:00:00Z", changeReason: null }],
        page: 1,
        pageSize: 10,
        totalCount: 1,
        totalPages: 1,
      }),
    );

    const result = await fetchContentRevisions("jwt", "c1", { page: 1, pageSize: 10 });
    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/admin\/content\/c1\/revisions\?/);
    expect(String(url)).toMatch(/page=1/);
    expect(String(url)).toMatch(/pageSize=10/);
    expect(init.method).toBe("GET");
    expect((init.headers as Record<string, string>).Authorization).toBe("Bearer jwt");
    expect(result.items[0].versionNumber).toBe(2);
  });

  it("GETs a single revision by version", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        contentId: "c1",
        versionNumber: 1,
        snapshot: {
          title: "T",
          slug: "t",
          body: "b",
          excerpt: "",
          coverImage: null,
          contentType: "Article",
          seoMetadata: {
            seoTitle: null,
            seoDescription: null,
            canonicalUrl: null,
            ogImage: null,
            focusKeyword: null,
          },
        },
        changeReason: null,
        createdByUserId: "u1",
        createdAtUtc: "2026-07-01T00:00:00Z",
      }),
    );

    const detail = await fetchContentRevisionDetail("jwt", "c1", 1);
    const [url] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/admin\/content\/c1\/revisions\/1$/);
    expect(detail.versionNumber).toBe(1);
  });

  it("POSTs restore with optional changeReason and propagates AbortSignal", async () => {
    const controller = new AbortController();
    fetchMock.mockImplementationOnce(
      (_url: string, init: RequestInit) =>
        new Promise((_resolve, reject) => {
          init.signal?.addEventListener("abort", () =>
            reject(new DOMException("aborted", "AbortError")),
          );
        }),
    );

    const promise = restoreContentRevisionItem(
      "jwt",
      "c1",
      2,
      { changeReason: "undo" },
      controller.signal,
    );
    controller.abort();

    await expect(promise).rejects.toBeInstanceOf(ApiClientError);
  });

  it("uses no unversioned /api/ literals in history layer", () => {
    const offenders: string[] = [];
    for (const dir of HISTORY_DIRS) {
      for (const file of collect(dir)) {
        if (/["'`]\/api\/(?!v1)/.test(readFileSync(file, "utf8"))) offenders.push(file);
      }
    }
    expect(offenders).toHaveLength(0);
  });
});
