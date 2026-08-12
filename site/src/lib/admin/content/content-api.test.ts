import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { CONTENT_CAPABILITIES, analyzeContentSeo, runContentAi } from "./content-api";
import { ApiClientError } from "@/lib/api/errors";

const API_FILE = join(process.cwd(), "src/lib/admin/content/content-api.ts");
const RESOURCE_FILE = join(process.cwd(), "src/lib/api/content.ts");
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

describe("content API adapter", () => {
  it("reuses the shared client module and does not duplicate fetch logic", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain('from "@/lib/api/content"');
    expect(content).not.toMatch(/\bfetch\s*\(/);
    expect(content).not.toMatch(/["'`]\/api\/(?!v1)/);
  });

  it("advertises the real backend capabilities", () => {
    expect(CONTENT_CAPABILITIES.list).toBe(true);
    expect(CONTENT_CAPABILITIES.adminList).toBe(true);
    expect(CONTENT_CAPABILITIES.getBySlug).toBe(true);
    expect(CONTENT_CAPABILITIES.getById).toBe(true);
    expect(CONTENT_CAPABILITIES.create).toBe(true);
    expect(CONTENT_CAPABILITIES.update).toBe(true);
    expect(CONTENT_CAPABILITIES.publishExisting).toBe(true);
    expect(CONTENT_CAPABILITIES.seo).toBe(true);
    expect(CONTENT_CAPABILITIES.seoAnalysis).toBe(true);
    expect(CONTENT_CAPABILITIES.aiAssistant).toBe(true);
    expect(CONTENT_CAPABILITIES.articleMetadata).toBe(true);
    expect(CONTENT_CAPABILITIES.newsMetadata).toBe(true);
    expect(CONTENT_CAPABILITIES.toolLibrary).toBe(true);
    expect(CONTENT_CAPABILITIES.roadmapEngine).toBe(true);
  });
});

describe("content sources are honest about backend fields", () => {
  it("never fabricates SEO scores/grades or AI-derived judgements", () => {
    // The analyzer reports factual measurements only — no score/grade/rating.
    const forbidden = [
      /\bseoscore\b/i,
      /\bseo_score\b/i,
      /\breadabilityscore\b/i,
      /\bqualityscore\b/i,
      /\bcontentscore\b/i,
    ];
    const offenders: string[] = [];
    for (const dir of CONTENT_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        for (const re of forbidden) {
          if (re.test(text)) offenders.push(`${file} -> ${re}`);
        }
      }
    }
    expect(offenders, `Fabricated SEO scoring found:\n${offenders.join("\n")}`).toHaveLength(0);
  });

  it("does not add an AI/ML provider dependency in the content layer", () => {
    const forbidden = [/openai/i, /anthropic/i, /\bllm\b/i, /gpt-/i];
    const offenders: string[] = [];
    for (const dir of CONTENT_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        for (const re of forbidden) {
          if (re.test(text)) offenders.push(`${file} -> ${re}`);
        }
      }
    }
    expect(offenders).toHaveLength(0);
  });

  it("uses no unversioned /api/ literals", () => {
    const offenders: string[] = [];
    for (const dir of CONTENT_DIRS) {
      for (const file of collect(dir)) {
        if (/["'`]\/api\/(?!v1)/.test(readFileSync(file, "utf8"))) offenders.push(file);
      }
    }
    expect(offenders).toHaveLength(0);
  });
});

describe("content resource endpoints target the real backend routes/verbs", () => {
  const source = readFileSync(RESOURCE_FILE, "utf8");

  it("lists via GET /admin/content", () => {
    expect(source).toContain("getAdminContentList");
    expect(source).toContain('path: "/admin/content"');
  });

  it("reads admin detail via GET /admin/content/{id}", () => {
    expect(source).toContain("getAdminContentById");
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}`");
  });

  it("edits via PUT /admin/content/{id}", () => {
    expect(source).toMatch(/method:\s*["']PUT["']/);
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}`");
  });

  it("publishes via POST /admin/content/{id}/publish", () => {
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}/publish`");
  });

  it("updates SEO via PUT /admin/content/{id}/seo", () => {
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}/seo`");
  });

  it("analyzes SEO via POST /admin/content/{id}/seo-analysis", () => {
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}/seo-analysis`");
    expect(source).toContain("analyzeContentSeo");
  });

  it("runs Content AI via POST /admin/content/{id}/ai/{action}", () => {
    expect(source).toContain("/admin/content/${encodeURIComponent(id)}/ai/${action}`");
    expect(source).toContain("runContentAiAction");
  });
});

describe("runContentAi (POST /admin/content/{id}/ai/{action})", () => {
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

  it("POSTs to the AI action route and returns the DTO without inventing fields", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        taskType: "ContentAnalysis",
        generatedText: "suggestion",
        createdAtUtc: "2026-07-22T12:00:00Z",
        model: "fake-v1",
        provider: "Fake",
      }),
    );

    const result = await runContentAi("jwt-token", "content-1", "analyze");

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/admin\/content\/content-1\/ai\/analyze$/);
    expect(init.method).toBe("POST");
    expect(result.generatedText).toBe("suggestion");
    expect(result).not.toHaveProperty("apiKey");
    expect(result).not.toHaveProperty("systemInstruction");
  });
});

describe("analyzeContentSeo (POST /admin/content/{id}/seo-analysis)", () => {
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

  const reportBody = {
    contentId: "content-1",
    generatedAtUtc: "2026-07-01T10:00:00Z",
    summary: { errorCount: 0, warningCount: 1, infoCount: 2 },
    findings: [],
  };

  it("POSTs to /admin/content/{id}/seo-analysis with the bearer token and returns the report", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(reportBody));

    const report = await analyzeContentSeo("jwt-token", "content-1");

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/admin\/content\/content-1\/seo-analysis$/);
    expect(init.method).toBe("POST");
    expect((init.headers as Record<string, string>).Authorization).toBe("Bearer jwt-token");
    expect(report.summary.warningCount).toBe(1);
    expect(report.contentId).toBe("content-1");
  });

  it("propagates an AbortSignal to the underlying fetch call", async () => {
    const controller = new AbortController();
    fetchMock.mockImplementationOnce(
      (_url: string, init: RequestInit) =>
        new Promise((_resolve, reject) => {
          init.signal?.addEventListener("abort", () =>
            reject(new DOMException("aborted", "AbortError")),
          );
        }),
    );

    const promise = analyzeContentSeo("jwt-token", "content-1", controller.signal);
    controller.abort();

    await expect(promise).rejects.toBeInstanceOf(ApiClientError);
    await promise.catch((error: ApiClientError) => {
      expect(error.code).toBe("request_aborted");
    });
  });

  it("surfaces a non-2xx response as an ApiClientError with the safe backend message/code", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ message: "محتوا یافت نشد.", code: "content_not_found" }, 404),
    );

    try {
      await analyzeContentSeo("jwt-token", "missing-id");
      throw new Error("Expected the request to reject, but it resolved.");
    } catch (error) {
      expect(error).toBeInstanceOf(ApiClientError);
      expect((error as ApiClientError).status).toBe(404);
      expect((error as ApiClientError).isNotFound).toBe(true);
      expect((error as ApiClientError).code).toBe("content_not_found");
    }
  });
});
