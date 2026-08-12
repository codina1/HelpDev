import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  approveContentItem,
  CONTENT_WORKFLOW_CAPABILITIES,
  fetchContentWorkflowHistory,
  rejectContentItem,
  submitContentForReviewItem,
} from "./workflow-api";
import { ApiClientError } from "@/lib/api/errors";

const API_FILE = join(process.cwd(), "src/lib/admin/content/workflow/workflow-api.ts");
const RESOURCE_FILE = join(process.cwd(), "src/lib/api/content.ts");
const WORKFLOW_DIRS = [
  join(process.cwd(), "src/lib/admin/content/workflow"),
  join(process.cwd(), "src/components/admin/content/workflow"),
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

describe("content workflow API adapter", () => {
  it("reuses the shared client module and does not duplicate fetch logic", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain('from "@/lib/api/content"');
    expect(content).not.toMatch(/\bfetch\s*\(/);
    expect(content).not.toMatch(/["'`]\/api\/(?!v1)/);
  });

  it("advertises workflow capabilities", () => {
    expect(CONTENT_WORKFLOW_CAPABILITIES.submitReview).toBe(true);
    expect(CONTENT_WORKFLOW_CAPABILITIES.approve).toBe(true);
    expect(CONTENT_WORKFLOW_CAPABILITIES.reject).toBe(true);
    expect(CONTENT_WORKFLOW_CAPABILITIES.publish).toBe(true);
    expect(CONTENT_WORKFLOW_CAPABILITIES.archive).toBe(true);
    expect(CONTENT_WORKFLOW_CAPABILITIES.history).toBe(true);
  });

  it("keeps workflow modules free of raw fetch paths", () => {
    for (const file of WORKFLOW_DIRS.flatMap((dir) => collect(dir))) {
      const text = readFileSync(file, "utf8");
      expect(text).not.toMatch(/fetch\s*\(\s*["'`]\/admin/);
    }
  });
});

describe("content workflow resource endpoints", () => {
  const source = readFileSync(RESOURCE_FILE, "utf8");

  it("submits review via POST /admin/content/{id}/submit-review", () => {
    expect(source).toContain("submitContentForReview");
    expect(source).toContain("/submit-review");
  });

  it("approves via POST /admin/content/{id}/approve", () => {
    expect(source).toContain("approveContent");
    expect(source).toContain("/approve");
  });

  it("rejects via POST /admin/content/{id}/reject", () => {
    expect(source).toContain("rejectContent");
    expect(source).toContain("/reject");
  });

  it("archives via POST /admin/content/{id}/archive", () => {
    expect(source).toContain("archiveContent");
    expect(source).toContain("/archive");
  });

  it("loads history via GET /admin/content/{id}/workflow-history", () => {
    expect(source).toContain("getContentWorkflowHistory");
    expect(source).toContain("/workflow-history");
  });
});

describe("workflow HTTP integration (mocked fetch)", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  function jsonResponse(body: unknown, status = 200): Response {
    return new Response(JSON.stringify(body), {
      status,
      headers: { "Content-Type": "application/json" },
    });
  }

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  const token = "test-token";
  const contentId = "11111111-1111-1111-1111-111111111111";
  const detailBody = {
    id: contentId,
    title: "t",
    slug: "t",
    body: "b",
    excerpt: "",
    coverImage: null,
    contentType: "Article",
    contentStatus: "ReviewPending",
    authorId: contentId,
    views: 0,
    saves: 0,
    createdAtUtc: "2026-01-01T00:00:00Z",
    updatedAtUtc: "2026-01-01T00:00:00Z",
    publishedAtUtc: null,
    seo: {
      seoTitle: null,
      seoDescription: null,
      canonicalUrl: null,
      ogImage: null,
      focusKeyword: null,
    },
  };

  it("fetchContentWorkflowHistory calls the history endpoint", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        items: [
          {
            id: "22222222-2222-2222-2222-222222222222",
            fromStatus: "Draft",
            toStatus: "ReviewPending",
            actorUserId: contentId,
            comment: null,
            createdAtUtc: "2026-01-02T00:00:00Z",
          },
        ],
      }),
    );

    const history = await fetchContentWorkflowHistory(token, contentId);
    expect(history.items).toHaveLength(1);
    expect(fetchMock).toHaveBeenCalledOnce();
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toContain(`/admin/content/${contentId}/workflow-history`);
    expect(init.method ?? "GET").toBe("GET");
  });

  it("submitContentForReviewItem posts to submit-review", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(detailBody));
    await submitContentForReviewItem(token, contentId);
    const [url, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(url).toContain("/submit-review");
    expect(init.method).toBe("POST");
  });

  it("rejectContentItem posts comment body", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ ...detailBody, contentStatus: "Draft" }));
    await rejectContentItem(token, contentId, { comment: "نیاز به بازنویسی" });
    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect(init.method).toBe("POST");
    expect(init.body).toContain("نیاز به بازنویسی");
  });

  it("surfaces API errors from approve", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ title: "Forbidden", status: 403, detail: "no" }, 403),
    );
    await expect(approveContentItem(token, contentId)).rejects.toBeInstanceOf(ApiClientError);
  });
});
