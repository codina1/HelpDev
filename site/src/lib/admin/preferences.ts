/**
 * Admin UI preferences persistence.
 *
 * Only safe, non-sensitive UI state is stored (sidebar collapse, theme,
 * collapsed nav groups). Tokens, permissions, roles, filters and API responses
 * are never persisted here. Uses a versioned key and validates malformed data.
 */

export type AdminTheme = "light" | "dark" | "system";

export type AdminPreferences = {
  sidebarCollapsed: boolean;
  theme: AdminTheme;
  collapsedGroups: string[];
};

export const ADMIN_PREFERENCES_STORAGE_KEY = "helpdev.admin.preferences.v1";

export const DEFAULT_ADMIN_PREFERENCES: AdminPreferences = {
  sidebarCollapsed: false,
  theme: "system",
  collapsedGroups: [],
};

const THEMES: readonly AdminTheme[] = ["light", "dark", "system"];

function isTheme(value: unknown): value is AdminTheme {
  return typeof value === "string" && THEMES.includes(value as AdminTheme);
}

/** Safely parses stored preferences, falling back to defaults per field. */
export function parseAdminPreferences(raw: string | null): AdminPreferences {
  if (!raw) return { ...DEFAULT_ADMIN_PREFERENCES };

  let data: unknown;
  try {
    data = JSON.parse(raw);
  } catch {
    return { ...DEFAULT_ADMIN_PREFERENCES };
  }

  if (typeof data !== "object" || data === null) {
    return { ...DEFAULT_ADMIN_PREFERENCES };
  }

  const record = data as Record<string, unknown>;

  const collapsedGroups = Array.isArray(record.collapsedGroups)
    ? record.collapsedGroups.filter((id): id is string => typeof id === "string")
    : DEFAULT_ADMIN_PREFERENCES.collapsedGroups;

  return {
    sidebarCollapsed:
      typeof record.sidebarCollapsed === "boolean"
        ? record.sidebarCollapsed
        : DEFAULT_ADMIN_PREFERENCES.sidebarCollapsed,
    theme: isTheme(record.theme)
      ? record.theme
      : DEFAULT_ADMIN_PREFERENCES.theme,
    collapsedGroups,
  };
}

export function readAdminPreferences(): AdminPreferences {
  if (typeof window === "undefined") return { ...DEFAULT_ADMIN_PREFERENCES };
  try {
    return parseAdminPreferences(
      window.localStorage.getItem(ADMIN_PREFERENCES_STORAGE_KEY),
    );
  } catch {
    return { ...DEFAULT_ADMIN_PREFERENCES };
  }
}

export function writeAdminPreferences(preferences: AdminPreferences): void {
  if (typeof window === "undefined") return;
  try {
    window.localStorage.setItem(
      ADMIN_PREFERENCES_STORAGE_KEY,
      JSON.stringify(preferences),
    );
  } catch {
    // Storage may be unavailable (private mode / quota). Preferences are
    // non-critical, so failing to persist is silently ignored.
  }
}

/** Resolves the effective light/dark theme, expanding "system". */
export function resolveEffectiveTheme(
  theme: AdminTheme,
  prefersDark: boolean,
): "light" | "dark" {
  if (theme === "system") return prefersDark ? "dark" : "light";
  return theme;
}
