import { describe, expect, it } from "vitest";
import {
  ADMIN_PREFERENCES_STORAGE_KEY,
  DEFAULT_ADMIN_PREFERENCES,
  parseAdminPreferences,
  resolveEffectiveTheme,
} from "./preferences";

describe("admin preferences storage key", () => {
  it("uses a versioned key", () => {
    expect(ADMIN_PREFERENCES_STORAGE_KEY).toBe("helpdev.admin.preferences.v1");
  });
});

describe("parseAdminPreferences", () => {
  it("returns defaults for null", () => {
    expect(parseAdminPreferences(null)).toEqual(DEFAULT_ADMIN_PREFERENCES);
  });

  it("returns defaults for malformed JSON", () => {
    expect(parseAdminPreferences("{not json")).toEqual(DEFAULT_ADMIN_PREFERENCES);
  });

  it("parses a valid, complete payload", () => {
    const parsed = parseAdminPreferences(
      JSON.stringify({
        sidebarCollapsed: true,
        theme: "light",
        collapsedGroups: ["content", "system"],
      }),
    );
    expect(parsed).toEqual({
      sidebarCollapsed: true,
      theme: "light",
      collapsedGroups: ["content", "system"],
    });
  });

  it("falls back per-field for invalid values", () => {
    const parsed = parseAdminPreferences(
      JSON.stringify({
        sidebarCollapsed: "yes",
        theme: "neon",
        collapsedGroups: ["ok", 5, null],
      }),
    );
    expect(parsed.sidebarCollapsed).toBe(false);
    expect(parsed.theme).toBe("system");
    expect(parsed.collapsedGroups).toEqual(["ok"]);
  });
});

describe("resolveEffectiveTheme", () => {
  it("expands system based on the media preference", () => {
    expect(resolveEffectiveTheme("system", true)).toBe("dark");
    expect(resolveEffectiveTheme("system", false)).toBe("light");
  });

  it("returns explicit themes unchanged", () => {
    expect(resolveEffectiveTheme("light", true)).toBe("light");
    expect(resolveEffectiveTheme("dark", false)).toBe("dark");
  });
});
