import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { apiRequest, buildRequestUrl, parseRetryAfter } from "./client";
import { CORRELATION_ID_HEADER } from "./correlation";
import { ApiClientError } from "./errors";

function jsonResponse(
  body: unknown,
  init: { status?: number; headers?: Record<string, string> } = {},
): Response {
  const headers = new Headers({ "content-type": "application/json", ...init.headers });
  return new Response(body === undefined ? undefined : JSON.stringify(body), {
    status: init.status ?? 200,
    headers,
  });
}

async function captureError(promise: Promise<unknown>): Promise<ApiClientError> {
  try {
    await promise;
    throw new Error("Expected the request to reject, but it resolved.");
  } catch (error) {
    return error as ApiClientError;
  }
}

describe("buildRequestUrl", () => {
  it("targets the canonical /api/v1 base and rejects non-rooted paths", () => {
    expect(buildRequestUrl("/auth/send-otp")).toMatch(/\/api\/v1\/auth\/send-otp$/);
    expect(() => buildRequestUrl("auth/send-otp")).toThrow(/must start with/);
  });

  it("appends query params, skipping null/undefined", () => {
    const url = buildRequestUrl("/search", { q: "react", type: undefined, page: 2 });
    expect(url).toContain("q=react");
    expect(url).toContain("page=2");
    expect(url).not.toContain("type=");
  });
});

describe("parseRetryAfter", () => {
  it("parses numeric seconds", () => {
    expect(parseRetryAfter("30")).toBe(30);
  });

  it("parses HTTP date into a bounded, non-negative delta", () => {
    const future = new Date(Date.now() + 60_000).toUTCString();
    const seconds = parseRetryAfter(future);
    expect(seconds).not.toBeNull();
    expect(seconds!).toBeGreaterThanOrEqual(0);
  });

  it("returns null for missing/garbage headers", () => {
    expect(parseRetryAfter(null)).toBeNull();
    expect(parseRetryAfter("not-a-date")).toBeNull();
  });
});

describe("apiRequest", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("injects a Bearer token when provided and generates a correlation id", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ ok: true }));

    await apiRequest({ path: "/profile/me", token: "jwt-token" });

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/profile\/me$/);
    const headers = init.headers as Record<string, string>;
    expect(headers.Authorization).toBe("Bearer jwt-token");
    expect(headers[CORRELATION_ID_HEADER]).toMatch(/^[A-Za-z0-9._-]{1,100}$/);
  });

  it("omits Authorization on a public request with no token", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse([]));

    await apiRequest({ path: "/content" });

    const [, init] = fetchMock.mock.calls[0];
    const headers = init.headers as Record<string, string>;
    expect(headers.Authorization).toBeUndefined();
  });

  it("captures the response X-Correlation-ID on errors", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse(
        { message: "بدون دسترسی", code: "forbidden" },
        { status: 403, headers: { [CORRELATION_ID_HEADER]: "server-cid-123" } },
      ),
    );

    const error = await captureError(apiRequest({ path: "/admin/dashboard", token: "t" }));
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.status).toBe(403);
    expect(error.code).toBe("forbidden");
    expect(error.correlationId).toBe("server-cid-123");
    expect(error.isForbidden).toBe(true);
  });

  it("maps 401 to an unauthorized ApiClientError", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ message: "unauth" }, { status: 401 }));
    const error = await captureError(apiRequest({ path: "/profile/me", token: "t" }));
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.isUnauthorized).toBe(true);
  });

  it("falls back to a safe message on a malformed error body", async () => {
    const headers = new Headers({ "content-type": "application/json" });
    fetchMock.mockResolvedValueOnce(new Response("not-json", { status: 500, headers }));

    const error = await captureError(apiRequest({ path: "/content" }));
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.status).toBe(500);
    expect(error.message.length).toBeGreaterThan(0);
    expect(error.code).toBeNull();
  });

  it("parses Retry-After on 429", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ message: "slow down", code: "rate_limited" }, {
        status: 429,
        headers: { "Retry-After": "42" },
      }),
    );

    const error = await captureError(apiRequest({ path: "/search" }));
    expect(error.isRateLimited).toBe(true);
    expect(error.retryAfterSeconds).toBe(42);
  });

  it("maps network failures to a network ApiClientError", async () => {
    fetchMock.mockRejectedValueOnce(new TypeError("Failed to fetch"));
    const error = await captureError(apiRequest({ path: "/content" }));
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.isNetworkError).toBe(true);
    expect(error.code).toBe("network_error");
  });

  it("aborts when a caller AbortSignal fires", async () => {
    const controller = new AbortController();
    fetchMock.mockImplementationOnce(
      (_url: string, init: RequestInit) =>
        new Promise((_resolve, reject) => {
          init.signal?.addEventListener("abort", () =>
            reject(new DOMException("aborted", "AbortError")),
          );
        }),
    );

    const promise = captureError(apiRequest({ path: "/content", signal: controller.signal }));
    controller.abort();
    const error = await promise;
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.code).toBe("request_aborted");
  });

  it("never logs the token or Authorization header", async () => {
    const logSpy = vi.spyOn(console, "log").mockImplementation(() => {});
    const errorSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
    fetchMock.mockResolvedValueOnce(jsonResponse({ ok: true }));

    await apiRequest({ path: "/profile/me", token: "super-secret-token" });

    const allArgs = [...logSpy.mock.calls, ...errorSpy.mock.calls, ...warnSpy.mock.calls]
      .flat()
      .map((a) => String(a))
      .join(" ");
    expect(allArgs).not.toContain("super-secret-token");
  });
});

