import { existsSync } from "node:fs";
import { join } from "node:path";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { HEADER_NAV } from "@/lib/constants";
import {
  approveLearningRoadmap,
  fetchLearningProfile,
  fetchLearningRecommendations,
  fetchLearningRoadmap,
  generateLearningRoadmap,
  updateLearningProfile,
} from "@/lib/api/learning-personalization";
import { sendOtp, verifyOtp } from "@/lib/api/auth";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: new Headers({ "content-type": "application/json" }),
  });
}

describe("Sprint 44 — critical user/admin frontend flows", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("user auth uses canonical /api/v1 OTP routes", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ message: "ok", expiresInSeconds: 120 }));
    await sendOtp("09120000044");
    expect(String(fetchMock.mock.calls[0][0])).toMatch(/\/api\/v1\/auth\/send-otp$/);

    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        accessToken: "jwt",
        expiresIn: 3600,
        user: { id: "u1", mobile: "09120000044", role: "User" },
      }),
    );
    const session = await verifyOtp("09120000044", "123456");
    expect(String(fetchMock.mock.calls[1][0])).toMatch(/\/api\/v1\/auth\/verify-otp$/);
    expect(session.accessToken).toBe("jwt");
  });

  it("learning profile / assistant / roadmap clients hit /api/v1/me/*", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        userId: "u1",
        experienceLevel: "Beginner",
        learningGoals: "AI",
        currentSkills: "C#",
        preferredTopics: [],
        createdAtUtc: new Date().toISOString(),
        updatedAtUtc: new Date().toISOString(),
      }),
    );
    await fetchLearningProfile("token");
    expect(String(fetchMock.mock.calls[0][0])).toMatch(/\/api\/v1\/me\/learning-profile$/);

    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        userId: "u1",
        experienceLevel: "Intermediate",
        learningGoals: "AI",
        currentSkills: "C#",
        preferredTopics: [{ topic: ".NET", priority: 1, interestLevel: 5 }],
        createdAtUtc: new Date().toISOString(),
        updatedAtUtc: new Date().toISOString(),
      }),
    );
    await updateLearningProfile("token", {
      experienceLevel: "Intermediate",
      learningGoals: "AI",
      currentSkills: "C#",
      preferredTopics: [{ topic: ".NET", priority: 1, interestLevel: 5 }],
    });
    expect(String(fetchMock.mock.calls[1][0])).toMatch(/\/api\/v1\/me\/learning-profile$/);
    expect(fetchMock.mock.calls[1][1].method).toBe("PUT");

    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        recommendedItems: [],
        reason: "Based on profile",
        nextSteps: ["Practice"],
        generatedAtUtc: new Date().toISOString(),
      }),
    );
    await fetchLearningRecommendations("token");
    expect(String(fetchMock.mock.calls[2][0])).toMatch(/\/api\/v1\/me\/recommendations$/);

    fetchMock.mockResolvedValueOnce(new Response(null, { status: 204 }));
    await fetchLearningRoadmap("token");
    expect(String(fetchMock.mock.calls[3][0])).toMatch(/\/api\/v1\/me\/roadmap$/);

    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        id: "r1",
        goal: "AI",
        status: "Suggested",
        steps: [],
        createdAtUtc: new Date().toISOString(),
        updatedAtUtc: new Date().toISOString(),
        approvedAtUtc: null,
      }),
    );
    await generateLearningRoadmap("token", "AI");
    expect(String(fetchMock.mock.calls[4][0])).toMatch(/\/api\/v1\/me\/roadmap\/generate$/);

    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        id: "r1",
        goal: "AI",
        status: "Approved",
        steps: [],
        createdAtUtc: new Date().toISOString(),
        updatedAtUtc: new Date().toISOString(),
        approvedAtUtc: new Date().toISOString(),
      }),
    );
    await approveLearningRoadmap("token");
    expect(String(fetchMock.mock.calls[5][0])).toMatch(/\/api\/v1\/me\/roadmap\/approve$/);
  });

  it("user learning pages and nav entries exist", () => {
    const appRoot = join(process.cwd(), "src", "app");
    expect(existsSync(join(appRoot, "learning", "profile", "page.tsx"))).toBe(true);
    expect(existsSync(join(appRoot, "learning", "assistant", "page.tsx"))).toBe(true);
    expect(existsSync(join(appRoot, "profile", "page.tsx"))).toBe(true);
    expect(HEADER_NAV.some((item) => item.href === "/learning/assistant")).toBe(true);
  });

  it("admin critical shell routes resolve to page modules", () => {
    const appRoot = join(process.cwd(), "src", "app");
    const critical: Array<{ route: string; relative: string }> = [
      { route: ADMIN_ROUTES.dashboard, relative: "admin/page.tsx" },
      { route: ADMIN_ROUTES.content, relative: "admin/content/page.tsx" },
      { route: ADMIN_ROUTES.seo, relative: "admin/seo/page.tsx" },
      { route: ADMIN_ROUTES.media, relative: "admin/media/page.tsx" },
      { route: ADMIN_ROUTES.contentWorkflows, relative: "admin/content/workflows/page.tsx" },
      { route: ADMIN_ROUTES.ai, relative: "admin/ai/page.tsx" },
      { route: ADMIN_ROUTES.analytics, relative: "admin/analytics/page.tsx" },
      { route: ADMIN_ROUTES.searchKnowledge, relative: "admin/search/knowledge/page.tsx" },
      { route: ADMIN_ROUTES.learning, relative: "admin/learning/page.tsx" },
    ];

    for (const item of critical) {
      expect(item.route.startsWith("/admin")).toBe(true);
      expect(existsSync(join(appRoot, item.relative)), `Missing page for ${item.route}`).toBe(
        true,
      );
    }
  });
});
