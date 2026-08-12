import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { verifyOtp } from "./auth";
import { fetchMyProfile } from "./profile";
import { listPublishedContent } from "./content";
import { search } from "./search";
import { executeTool } from "./toolbox";
import { renderPrompt } from "./promptlab";
import { adminApi } from "./admin";
import { ApiClientError } from "./errors";

function json(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: new Headers({ "content-type": "application/json" }),
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

/**
 * Representative DTO contract checks. These guard against silent drift between
 * the frontend's assumed shapes and the backend response contracts.
 */
describe("representative DTO contracts", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("Auth verify response", async () => {
    fetchMock.mockResolvedValueOnce(
      json({ accessToken: "j", expiresIn: 3600, user: { id: "1", mobile: "0912", role: "User" } }),
    );
    const dto = await verifyOtp("0912", "1");
    expect(dto).toMatchObject({ accessToken: expect.any(String), expiresIn: expect.any(Number) });
    expect(dto.user.id).toBe("1");
  });

  it("User profile", async () => {
    fetchMock.mockResolvedValueOnce(json({ id: "1", mobile: "0912", role: "User" }));
    const dto = await fetchMyProfile("t");
    expect(dto.role).toBe("User");
  });

  it("Content summary list", async () => {
    fetchMock.mockResolvedValueOnce(
      json([{ id: "1", title: "t", slug: "t", type: "Article", status: "Published", views: 0, saves: 0, createdAt: "x" }]),
    );
    const dto = await listPublishedContent();
    expect(Array.isArray(dto)).toBe(true);
    expect(dto[0].slug).toBe("t");
  });

  it("Search result", async () => {
    fetchMock.mockResolvedValueOnce(
      json({ query: "react", total: 0, page: 1, pageSize: 10, items: [] }),
    );
    const dto = await search({ q: "react" });
    expect(dto).toMatchObject({ total: expect.any(Number), items: expect.any(Array) });
  });

  it("Tool execution response", async () => {
    fetchMock.mockResolvedValueOnce(json({ output: "result" }));
    const dto = await executeTool("slug", { value: "x" });
    expect(dto.output).toBe("result");
  });

  it("Prompt render response", async () => {
    fetchMock.mockResolvedValueOnce(json({ renderedPrompt: "hello" }));
    const dto = await renderPrompt("slug", { name: "x" });
    expect(dto.renderedPrompt).toBe("hello");
  });

  it("Operational version response", async () => {
    fetchMock.mockResolvedValueOnce(
      json({ version: "1.0.0", channel: "production", environment: "Production", uptimeSeconds: 10 }),
    );
    const dto = await adminApi.getVersion("t");
    expect(dto.version).toBe("1.0.0");
    expect(dto.channel).toBe("production");
  });

  it("API error response shape (message + code)", async () => {
    fetchMock.mockResolvedValueOnce(json({ message: "no", code: "not_found" }, 404));
    const error = await captureError(listPublishedContent());
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.code).toBe("not_found");
    expect(error.status).toBe(404);
  });
});
