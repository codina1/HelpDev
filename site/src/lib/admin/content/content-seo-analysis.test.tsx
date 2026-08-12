/**
 * Behavioral tests for the SEO Analyzer Engine v1 hook. Uses a tiny hand-rolled
 * `renderHook` (react-dom + `act`) instead of an external testing-library
 * dependency, per the "do not add packages" constraint.
 */
import { act } from "react";
import { createRoot, type Root } from "react-dom/client";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiClientError } from "@/lib/api/errors";

// Silences React's "not configured to support act()" warning under Vitest/jsdom
// (there is no test-renderer here — this file drives react-dom directly).
(globalThis as unknown as { IS_REACT_ACT_ENVIRONMENT?: boolean }).IS_REACT_ACT_ENVIRONMENT = true;

const authState: { token: string | null } = { token: "jwt-token" };

vi.mock("@/components/auth", () => ({
  useAuth: () => ({ token: authState.token }),
}));

import { useContentSeoAnalysis, type ContentSeoAnalysisState } from "./content-hooks";

function renderHook(
  callback: (id: string | null) => ContentSeoAnalysisState,
  initialId: string | null,
): {
  result: { current: ContentSeoAnalysisState };
  rerender: (nextId: string | null) => void;
  unmount: () => void;
} {
  const result: { current: ContentSeoAnalysisState } = {
    current: undefined as unknown as ContentSeoAnalysisState,
  };
  let currentId = initialId;
  let root!: Root;
  const container = document.createElement("div");
  document.body.appendChild(container);

  function TestComponent({ id }: { id: string | null }) {
    result.current = callback(id);
    return null;
  }

  act(() => {
    root = createRoot(container);
    root.render(<TestComponent id={currentId} />);
  });

  return {
    result,
    rerender(nextId: string | null) {
      currentId = nextId;
      act(() => {
        root.render(<TestComponent id={currentId} />);
      });
    },
    unmount() {
      act(() => root.unmount());
      container.remove();
    },
  };
}

/** Flushes the microtask queue so pending fetch/json promises settle. */
async function flush() {
  await act(async () => {
    for (let i = 0; i < 10; i += 1) {
      await Promise.resolve();
    }
  });
}

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: new Headers({ "content-type": "application/json" }),
  });
}

const reportBody = {
  contentId: "content-1",
  generatedAtUtc: "2026-07-01T10:15:00Z",
  summary: { errorCount: 1, warningCount: 2, infoCount: 1 },
  findings: [
    {
      ruleId: "focus-keyword-missing",
      category: "Metadata",
      severity: "Warning",
      message: "کلمه کلیدی کانونی در متن یافت نشد.",
      suggestion: "کلمه کلیدی را در پاراگراف اول بیاورید.",
      field: "focusKeyword",
    },
  ],
};

describe("useContentSeoAnalysis", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    authState.token = "jwt-token";
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("starts idle with no report, and never calls the network on mount", async () => {
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");
    await flush();

    expect(hook.result.current.status).toBe("idle");
    expect(hook.result.current.report).toBeNull();
    expect(fetchMock).not.toHaveBeenCalled();

    hook.unmount();
  });

  it("transitions idle -> analyzing -> success on an explicit analyze() call", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(reportBody));
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => {
      hook.result.current.analyze();
    });
    expect(hook.result.current.status).toBe("analyzing");

    await flush();

    expect(hook.result.current.status).toBe("success");
    expect(hook.result.current.report?.summary.warningCount).toBe(2);
    expect(hook.result.current.report?.findings[0].categoryLabel).toBe("متادیتا");
    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/admin\/content\/content-1\/seo-analysis$/);
    expect(init.method).toBe("POST");

    hook.unmount();
  });

  it("transitions to error on a failed analyze() call, without throwing", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ message: "خطای سرور", code: "server_error" }, 500),
    );
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => {
      hook.result.current.analyze();
    });
    await flush();

    expect(hook.result.current.status).toBe("error");
    expect(hook.result.current.error).toBeInstanceOf(ApiClientError);
    expect(hook.result.current.report).toBeNull();

    hook.unmount();
  });

  it("moves a successful report to 'stale' via markStale(), without any network call", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(reportBody));
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => {
      hook.result.current.analyze();
    });
    await flush();
    expect(hook.result.current.status).toBe("success");

    act(() => {
      hook.result.current.markStale();
    });

    expect(hook.result.current.status).toBe("stale");
    // The (now stale) report is still available for display with the label.
    expect(hook.result.current.report).not.toBeNull();
    expect(fetchMock).toHaveBeenCalledTimes(1);

    hook.unmount();
  });

  it("markStale() is a no-op before any successful analysis (nothing to go stale)", async () => {
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => {
      hook.result.current.markStale();
    });
    await flush();

    expect(hook.result.current.status).toBe("idle");
    expect(fetchMock).not.toHaveBeenCalled();

    hook.unmount();
  });

  it("rerunning analyze() from 'stale' returns to 'success' with a fresh report", async () => {
    fetchMock
      .mockResolvedValueOnce(jsonResponse(reportBody))
      .mockResolvedValueOnce(
        jsonResponse({
          ...reportBody,
          summary: { errorCount: 0, warningCount: 0, infoCount: 9 },
        }),
      );
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => hook.result.current.analyze());
    await flush();
    act(() => hook.result.current.markStale());
    expect(hook.result.current.status).toBe("stale");

    act(() => hook.result.current.analyze());
    expect(hook.result.current.status).toBe("analyzing");
    await flush();

    expect(hook.result.current.status).toBe("success");
    expect(hook.result.current.report?.summary.infoCount).toBe(9);
    expect(fetchMock).toHaveBeenCalledTimes(2);

    hook.unmount();
  });

  it("resets to idle with no report when the content id changes", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse(reportBody));
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => hook.result.current.analyze());
    await flush();
    expect(hook.result.current.status).toBe("success");

    hook.rerender("content-2");
    await flush();

    expect(hook.result.current.status).toBe("idle");
    expect(hook.result.current.report).toBeNull();

    hook.unmount();
  });

  it("surfaces a friendly error and does not call fetch when there is no auth token", async () => {
    authState.token = null;
    const hook = renderHook((id) => useContentSeoAnalysis(id), "content-1");

    act(() => {
      hook.result.current.analyze();
    });
    await flush();

    expect(hook.result.current.status).toBe("error");
    expect(fetchMock).not.toHaveBeenCalled();

    hook.unmount();
  });
});
