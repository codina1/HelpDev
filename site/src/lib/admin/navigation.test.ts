import { describe, expect, it } from "vitest";
import {
  ADMIN_NAVIGATION,
  MAX_NAV_DEPTH,
  filterAdminNavigation,
  flattenNavItems,
  type AdminNavItem,
} from "./navigation";

function depthOf(item: AdminNavItem, depth = 1): number {
  if (!item.children || item.children.length === 0) return depth;
  return Math.max(...item.children.map((child) => depthOf(child, depth + 1)));
}

describe("admin navigation config", () => {
  it("has unique item ids across all groups", () => {
    const ids = flattenNavItems(ADMIN_NAVIGATION).map((item) => item.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it("has unique group ids", () => {
    const ids = ADMIN_NAVIGATION.map((group) => group.id);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it("has unique hrefs for all navigable items", () => {
    const hrefs = flattenNavItems(ADMIN_NAVIGATION)
      .map((item) => item.href)
      .filter((href): href is string => typeof href === "string");
    expect(new Set(hrefs).size).toBe(hrefs.length);
  });

  it("never nests deeper than the allowed depth", () => {
    for (const item of flattenNavItems(ADMIN_NAVIGATION)) {
      expect(depthOf(item)).toBeLessThanOrEqual(MAX_NAV_DEPTH);
    }
  });

  it("marks non-navigable items as future (no href)", () => {
    for (const item of flattenNavItems(ADMIN_NAVIGATION)) {
      if (!item.href) {
        expect(item.status).toBe("future");
      }
    }
  });

  it("gives every ready item an href", () => {
    for (const item of flattenNavItems(ADMIN_NAVIGATION)) {
      if (item.status === "ready") {
        expect(typeof item.href).toBe("string");
      }
    }
  });
});

describe("filterAdminNavigation", () => {
  it("shows all admin sections to an Admin", () => {
    const filtered = filterAdminNavigation(ADMIN_NAVIGATION, "Admin");
    expect(filtered.map((group) => group.id)).toEqual(
      ADMIN_NAVIGATION.map((group) => group.id),
    );
  });

  it("shows no admin navigation to a non-admin user", () => {
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "User")).toHaveLength(0);
    expect(filterAdminNavigation(ADMIN_NAVIGATION, "Writer")).toHaveLength(0);
    expect(filterAdminNavigation(ADMIN_NAVIGATION, null)).toHaveLength(0);
  });
});
