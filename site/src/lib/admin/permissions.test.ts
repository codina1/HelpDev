import { describe, expect, it } from "vitest";
import {
  ALL_ADMIN_PERMISSIONS,
  canAccessAdmin,
  canAccessAdminRoute,
  getPermissionsForRole,
  hasPermission,
} from "./permissions";

describe("admin permissions", () => {
  it("grants all permissions to Admin", () => {
    const perms = getPermissionsForRole("Admin");
    for (const permission of ALL_ADMIN_PERMISSIONS) {
      expect(perms.has(permission)).toBe(true);
    }
  });

  it("grants no permissions to non-admins", () => {
    expect(getPermissionsForRole("User").size).toBe(0);
    expect(getPermissionsForRole("Writer").size).toBe(0);
    expect(getPermissionsForRole(null).size).toBe(0);
  });

  it("gates admin access by role", () => {
    expect(canAccessAdmin("Admin")).toBe(true);
    expect(canAccessAdmin("User")).toBe(false);
    expect(canAccessAdmin(undefined)).toBe(false);
  });

  it("treats undefined permission requirement as allowed", () => {
    expect(hasPermission(getPermissionsForRole("Admin"), undefined)).toBe(true);
  });

  it("checks specific route permissions", () => {
    expect(canAccessAdminRoute("Admin", "content.create")).toBe(true);
    expect(canAccessAdminRoute("User", "content.create")).toBe(false);
    expect(canAccessAdminRoute("Admin")).toBe(true);
  });
});
