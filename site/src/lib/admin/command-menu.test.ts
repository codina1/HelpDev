import { describe, expect, it } from "vitest";
import {
  buildCommandRegistry,
  normalizeSearchText,
  searchCommands,
} from "./command-menu";
import { ADMIN_NAVIGATION, filterAdminNavigation } from "./navigation";
import { getPermissionsForRole } from "./permissions";

const adminCommands = buildCommandRegistry(
  filterAdminNavigation(ADMIN_NAVIGATION, "Admin"),
  getPermissionsForRole("Admin"),
);

describe("buildCommandRegistry", () => {
  it("includes only ready navigation routes", () => {
    for (const command of adminCommands) {
      expect(typeof command.href).toBe("string");
      expect(command.href.startsWith("/admin")).toBe(true);
    }
  });

  it("includes ready quick-create commands", () => {
    expect(adminCommands.some((c) => c.kind === "create")).toBe(true);
  });

  it("respects permission filtering (non-admin gets nothing)", () => {
    const userCommands = buildCommandRegistry(
      filterAdminNavigation(ADMIN_NAVIGATION, "User"),
      getPermissionsForRole("User"),
    );
    expect(userCommands).toHaveLength(0);
  });
});

describe("normalizeSearchText", () => {
  it("normalizes arabic/persian yeh and kaf", () => {
    expect(normalizeSearchText("كاربري")).toBe(normalizeSearchText("کاربری"));
  });
});

describe("searchCommands", () => {
  it("matches by english keyword", () => {
    const results = searchCommands(adminCommands, "users");
    expect(results.some((c) => c.href === "/admin/users")).toBe(true);
  });

  it("matches by persian keyword", () => {
    const results = searchCommands(adminCommands, "کاربر");
    expect(results.some((c) => c.href === "/admin/users")).toBe(true);
  });

  it("returns everything for an empty query", () => {
    expect(searchCommands(adminCommands, "")).toHaveLength(adminCommands.length);
  });

  it("returns nothing for an unmatched query", () => {
    expect(searchCommands(adminCommands, "zzzznotacommand")).toHaveLength(0);
  });
});
