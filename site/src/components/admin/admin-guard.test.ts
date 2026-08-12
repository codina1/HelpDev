import { describe, expect, it } from "vitest";
import { evaluateRouteAccess } from "@/components/auth/route-guard";

// The Admin guard is a thin wrapper around evaluateRouteAccess with
// requireAdmin. These tests lock in the state machine it depends on.
describe("admin guard access states", () => {
  it("loading while auth is unknown", () => {
    expect(
      evaluateRouteAccess({ status: "unknown", role: null, requireAdmin: true }),
    ).toBe("loading");
  });

  it("unauthenticated for anonymous and expired sessions", () => {
    expect(
      evaluateRouteAccess({ status: "anonymous", role: null, requireAdmin: true }),
    ).toBe("unauthenticated");
    expect(
      evaluateRouteAccess({ status: "expired", role: null, requireAdmin: true }),
    ).toBe("unauthenticated");
  });

  it("forbidden for authenticated non-admins", () => {
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "User", requireAdmin: true }),
    ).toBe("forbidden");
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "Writer", requireAdmin: true }),
    ).toBe("forbidden");
  });

  it("allowed for authenticated admins", () => {
    expect(
      evaluateRouteAccess({ status: "authenticated", role: "Admin", requireAdmin: true }),
    ).toBe("allowed");
  });
});
