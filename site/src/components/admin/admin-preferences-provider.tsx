"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import {
  DEFAULT_ADMIN_PREFERENCES,
  readAdminPreferences,
  resolveEffectiveTheme,
  writeAdminPreferences,
  type AdminPreferences,
  type AdminTheme,
} from "@/lib/admin/preferences";

type AdminPreferencesContextValue = {
  preferences: AdminPreferences;
  hydrated: boolean;
  effectiveTheme: "light" | "dark";
  setTheme: (theme: AdminTheme) => void;
  toggleSidebar: () => void;
  setSidebarCollapsed: (collapsed: boolean) => void;
  toggleGroup: (groupId: string) => void;
  isGroupCollapsed: (groupId: string) => boolean;
};

const AdminPreferencesContext =
  createContext<AdminPreferencesContextValue | null>(null);

const DARK_QUERY = "(prefers-color-scheme: dark)";

function applyThemeAttribute(effective: "light" | "dark") {
  if (typeof document === "undefined") return;
  document.documentElement.setAttribute("data-admin-theme", effective);
}

function applySidebarAttribute(collapsed: boolean) {
  if (typeof document === "undefined") return;
  document.documentElement.setAttribute(
    "data-admin-sidebar",
    collapsed ? "collapsed" : "expanded",
  );
}

export function AdminPreferencesProvider({ children }: { children: ReactNode }) {
  // SSR + first client render use defaults so markup matches; the pre-paint
  // inline script has already applied the correct attributes to <html>, so
  // there is no visual flash while we hydrate the stored values below.
  const [preferences, setPreferences] = useState<AdminPreferences>(
    DEFAULT_ADMIN_PREFERENCES,
  );
  const [hydrated, setHydrated] = useState(false);
  const [prefersDark, setPrefersDark] = useState(true);

  useEffect(() => {
    const media = window.matchMedia(DARK_QUERY);
    setPrefersDark(media.matches);
    setPreferences(readAdminPreferences());
    setHydrated(true);

    const onChange = (event: MediaQueryListEvent) => setPrefersDark(event.matches);
    media.addEventListener("change", onChange);
    return () => media.removeEventListener("change", onChange);
  }, []);

  const effectiveTheme = useMemo(
    () => resolveEffectiveTheme(preferences.theme, prefersDark),
    [preferences.theme, prefersDark],
  );

  useEffect(() => {
    if (!hydrated) return;
    applyThemeAttribute(effectiveTheme);
  }, [hydrated, effectiveTheme]);

  useEffect(() => {
    if (!hydrated) return;
    applySidebarAttribute(preferences.sidebarCollapsed);
  }, [hydrated, preferences.sidebarCollapsed]);

  const update = useCallback(
    (next: AdminPreferences) => {
      setPreferences(next);
      writeAdminPreferences(next);
    },
    [],
  );

  const setTheme = useCallback(
    (theme: AdminTheme) => update({ ...preferences, theme }),
    [preferences, update],
  );

  const setSidebarCollapsed = useCallback(
    (collapsed: boolean) =>
      update({ ...preferences, sidebarCollapsed: collapsed }),
    [preferences, update],
  );

  const toggleSidebar = useCallback(
    () => setSidebarCollapsed(!preferences.sidebarCollapsed),
    [preferences.sidebarCollapsed, setSidebarCollapsed],
  );

  const toggleGroup = useCallback(
    (groupId: string) => {
      const collapsed = new Set(preferences.collapsedGroups);
      if (collapsed.has(groupId)) collapsed.delete(groupId);
      else collapsed.add(groupId);
      update({ ...preferences, collapsedGroups: [...collapsed] });
    },
    [preferences, update],
  );

  const isGroupCollapsed = useCallback(
    (groupId: string) => preferences.collapsedGroups.includes(groupId),
    [preferences.collapsedGroups],
  );

  const value = useMemo<AdminPreferencesContextValue>(
    () => ({
      preferences,
      hydrated,
      effectiveTheme,
      setTheme,
      toggleSidebar,
      setSidebarCollapsed,
      toggleGroup,
      isGroupCollapsed,
    }),
    [
      preferences,
      hydrated,
      effectiveTheme,
      setTheme,
      toggleSidebar,
      setSidebarCollapsed,
      toggleGroup,
      isGroupCollapsed,
    ],
  );

  return (
    <AdminPreferencesContext.Provider value={value}>
      {children}
    </AdminPreferencesContext.Provider>
  );
}

export function useAdminPreferences(): AdminPreferencesContextValue {
  const context = useContext(AdminPreferencesContext);
  if (!context) {
    throw new Error(
      "useAdminPreferences must be used within an AdminPreferencesProvider",
    );
  }
  return context;
}
