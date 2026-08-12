import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { MEDIA_CAPABILITIES, toAdminMediaListOptions, uploadMediaAssetItem } from "./media-api";
import { DEFAULT_ADMIN_MEDIA_LIST_QUERY } from "./media-types";
import { ApiClientError } from "@/lib/api/errors";

const API_FILE = join(process.cwd(), "src/lib/admin/media/media-api.ts");
const RESOURCE_FILE = join(process.cwd(), "src/lib/api/media.ts");
const CLIENT_FILE = join(process.cwd(), "src/lib/api/client.ts");
const MEDIA_DIRS = [
  join(process.cwd(), "src/lib/admin/media"),
  join(process.cwd(), "src/components/admin/media"),
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

describe("media API adapter", () => {
  it("reuses the shared client module and does not duplicate fetch logic", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain('from "@/lib/api/media"');
    expect(content).not.toMatch(/\bfetch\s*\(/);
    expect(content).not.toMatch(/["'`]\/api\/(?!v1)/);
  });

  it("advertises the real backend capabilities and explicitly has no delete", () => {
    expect(MEDIA_CAPABILITIES.upload).toBe(true);
    expect(MEDIA_CAPABILITIES.list).toBe(true);
    expect(MEDIA_CAPABILITIES.getById).toBe(true);
    expect(MEDIA_CAPABILITIES.delete).toBe(false);
  });

  it("maps workspace query to API options and omits an empty search", () => {
    expect(toAdminMediaListOptions(DEFAULT_ADMIN_MEDIA_LIST_QUERY)).toEqual({
      page: 1,
      pageSize: 24,
      search: undefined,
    });
    expect(toAdminMediaListOptions({ page: 2, pageSize: 48, search: "  cover  " })).toEqual({
      page: 2,
      pageSize: 48,
      search: "cover",
    });
  });
});

describe("media resource endpoints target the real backend routes/verbs", () => {
  const source = readFileSync(RESOURCE_FILE, "utf8");

  it("lists via GET /admin/media", () => {
    expect(source).toContain("getAdminMediaList");
    expect(source).toContain('path: "/admin/media"');
  });

  it("reads detail via GET /admin/media/{id}", () => {
    expect(source).toContain("getAdminMediaById");
    expect(source).toContain("/admin/media/${encodeURIComponent(id)}`");
  });

  it("uploads via POST /admin/media with a FormData body", () => {
    expect(source).toContain("uploadMediaAsset");
    expect(source).toMatch(/method:\s*["']POST["']/);
    expect(source).toContain('path: "/admin/media"');
    expect(source).toContain("new FormData()");
    expect(source).toContain('formData.append("file"');
  });

  it("never sets the multipart Content-Type header manually", () => {
    expect(source).not.toMatch(/Content-Type.*multipart/i);
  });

  it("has no delete endpoint defined anywhere in the media source files", () => {
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        expect(text).not.toMatch(/method:\s*["']DELETE["']/);
        expect(text).not.toMatch(/deleteMediaAsset/i);
      }
    }
  });
});

describe("shared client supports FormData without manually setting Content-Type", () => {
  const source = readFileSync(CLIENT_FILE, "utf8");

  it("detects FormData bodies and skips the JSON Content-Type header", () => {
    expect(source).toContain("instanceof FormData");
    expect(source).toMatch(/if \(hasBody && !isFormData\)/);
  });
});

describe("guardrails: no storage key, no SVG, no delete, no AI/cloud SDK", () => {
  it("never exposes a storage key or filesystem path field in Media types/UI", () => {
    const forbidden = [/storageKey/i, /filePath/i, /diskPath/i];
    const offenders: string[] = [];
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        for (const re of forbidden) {
          if (re.test(text)) offenders.push(`${file} -> ${re}`);
        }
      }
    }
    expect(offenders, offenders.join("\n")).toHaveLength(0);
  });

  it("never accepts SVG as an allowed content type", () => {
    const types = readFileSync(
      join(process.cwd(), "src/lib/admin/media/media-types.ts"),
      "utf8",
    );
    expect(types).not.toMatch(/image\/svg/);
  });

  it("does not add an AI/ML or cloud storage SDK dependency in the media layer", () => {
    const forbidden = [
      /openai/i,
      /anthropic/i,
      /\bllm\b/i,
      /gpt-/i,
      /aws-sdk/i,
      /@azure\/storage/i,
      /cloudinary/i,
      /firebase/i,
    ];
    const offenders: string[] = [];
    for (const dir of MEDIA_DIRS) {
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
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        if (/["'`]\/api\/(?!v1)/.test(readFileSync(file, "utf8"))) offenders.push(file);
      }
    }
    expect(offenders).toHaveLength(0);
  });
});

describe("uploadMediaAssetItem (POST /admin/media, multipart/form-data)", () => {
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

  const assetBody = {
    id: "m1",
    originalFileName: "cover.jpg",
    contentType: "image/jpeg",
    sizeBytes: 12345,
    width: 800,
    height: 600,
    publicUrl: "/media/2026/07/m1.jpg",
    altText: "لوگو",
    caption: null,
    uploadedByUserId: "u1",
    createdAtUtc: "2026-07-01T00:00:00Z",
    updatedAtUtc: "2026-07-01T00:00:00Z",
    status: "Active",
  };

  it("POSTs a FormData body without a manual Content-Type header", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(assetBody, 201));

    const file = new File([new Uint8Array(4)], "cover.jpg", { type: "image/jpeg" });
    const detail = await uploadMediaAssetItem("jwt-token", {
      file,
      altText: "لوگو",
      caption: null,
    });

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/admin\/media$/);
    expect(init.method).toBe("POST");
    expect(init.body).toBeInstanceOf(FormData);
    expect((init.body as FormData).get("file")).toBe(file);
    expect((init.body as FormData).get("altText")).toBe("لوگو");
    // The browser must set its own multipart boundary — never set manually.
    expect((init.headers as Record<string, string>)["Content-Type"]).toBeUndefined();
    expect((init.headers as Record<string, string>).Authorization).toBe("Bearer jwt-token");
    expect(detail.id).toBe("m1");
    expect(detail.publicUrl).toBe("/media/2026/07/m1.jpg");
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

    const file = new File([new Uint8Array(4)], "cover.jpg", { type: "image/jpeg" });
    const promise = uploadMediaAssetItem(
      "jwt-token",
      { file, altText: null, caption: null },
      controller.signal,
    );
    controller.abort();

    await expect(promise).rejects.toBeInstanceOf(ApiClientError);
    await promise.catch((error: ApiClientError) => {
      expect(error.code).toBe("request_aborted");
    });
  });

  it("surfaces a non-2xx response as an ApiClientError with the safe backend message/code", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ message: "فرمت فایل نامعتبر است.", code: "unsupported_media_type" }, 415),
    );

    const file = new File([new Uint8Array(4)], "bad.svg", { type: "image/svg+xml" });

    try {
      await uploadMediaAssetItem("jwt-token", { file, altText: null, caption: null });
      throw new Error("Expected the request to reject, but it resolved.");
    } catch (error) {
      expect(error).toBeInstanceOf(ApiClientError);
      expect((error as ApiClientError).status).toBe(415);
      expect((error as ApiClientError).code).toBe("unsupported_media_type");
    }
  });
});
