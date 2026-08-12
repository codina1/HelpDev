import { describe, expect, it } from "vitest";
import { evaluateRouteAccess } from "./route-guard";

describe("evaluateRouteAccess", () => {
  it("allows public routes for anonymous users", () => {
    expect(evaluateRouteAccess({ status: "anonymous", role: null })).toBe("allowed");
  });

  it("shows loading (no protected-page flash) while auth state is unknown", () => {
    expect(
      evaluateRouteAccess({ status: "unknown", role: null, requireAuth: true }),
    ).toBe("loading");
  });

  it("blocks unauthenticated users from protected routes", () => {
    expect(
      evaluateRouteAccess({ status: "anonymous", role: null, requireAuth: true }),
    ).toBe("unauthenticated");
    expect(
      evaluateRouteAccess({ status: "expired", role: null, requireAuth: true }),
    ).toBe("unauthenticated");
  });

  it("allows authenticated non-admin users on protected (non-admin) routes", () => {
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "User", requireAuth: true }),
    ).toBe("allowed");
  });

  it("forbids non-admin users on admin routes (403 experience)", () => {
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "User", requireAdmin: true }),
    ).toBe("forbidden");
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "Writer", requireAdmin: true }),
    ).toBe("forbidden");
  });

  it("allows admin users on admin routes", () => {
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "Admin", requireAdmin: true }),
    ).toBe("allowed");
  });
});
