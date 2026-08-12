import { describe, expect, it } from "vitest";
import { findActiveNavItemId, isRouteActive } from "./route-matcher";
import { ADMIN_NAVIGATION } from "./navigation";

describe("isRouteActive", () => {
  it("matches the dashboard only exactly", () => {
    expect(isRouteActive("/admin", "/admin", true)).toBe(true);
    expect(isRouteActive("/admin/content", "/admin", true)).toBe(false);
  });

  it("matches nested routes for prefix (non-exact) items", () => {
    expect(isRouteActive("/admin/content", "/admin/content")).toBe(true);
    expect(isRouteActive("/admin/content/new", "/admin/content")).toBe(true);
  });

  it("does not falsely match sibling routes sharing a prefix", () => {
    expect(isRouteActive("/admin/content-archive", "/admin/content")).toBe(false);
  });

  it("ignores query strings and trailing slashes", () => {
    expect(isRouteActive("/admin/content?page=2", "/admin/content")).toBe(true);
    expect(isRouteActive("/admin/content/", "/admin/content")).toBe(true);
  });
});

describe("findActiveNavItemId", () => {
  it("activates the dashboard for /admin", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin")).toBe("dashboard");
  });

  it("activates the content platform hub for /admin/content", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/content")).toBe(
      "content-hub",
    );
  });

  it("activates the legacy all-list for /admin/content/all", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/content/all")).toBe(
      "content-all",
    );
  });

  it("activates the most specific leaf for deep routes", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/content/articles/new")).toBe(
      "content-articles",
    );
  });

  it("returns null for unknown routes", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/unknown")).toBeNull();
  });
});
