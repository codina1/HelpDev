import type { UserRole } from "@/types/auth";

/**
 * Admin permission model.
 *
 * IMPORTANT: Backend authorization is authoritative. These permissions exist
 * only to shape the Admin UX (which navigation entries and commands are shown).
 * They are derived from the authenticated user's role and are never read from
 * the query string or localStorage.
 *
 * Today the backend exposes a single `Admin` role, so an Admin receives every
 * permission. The structure is intentionally granular so future backend roles
 * can map to a permission subset without changing the navigation config.
 */
export type AdminPermission =
  | "dashboard.view"
  | "content.view"
  | "content.create"
  | "learning.view"
  | "toolbox.view"
  | "promptLab.view"
  | "users.view"
  | "analytics.view"
  | "system.view";

export const ALL_ADMIN_PERMISSIONS: readonly AdminPermission[] = [
  "dashboard.view",
  "content.view",
  "content.create",
  "learning.view",
  "toolbox.view",
  "promptLab.view",
  "users.view",
  "analytics.view",
  "system.view",
] as const;

/** Resolves the permission set granted to a role. */
export function getPermissionsForRole(
  role: UserRole | null | undefined,
): ReadonlySet<AdminPermission> {
  if (role === "Admin") {
    return new Set(ALL_ADMIN_PERMISSIONS);
  }
  return new Set<AdminPermission>();
}

/** True when the role may enter the Admin area at all. */
export function canAccessAdmin(role: UserRole | null | undefined): boolean {
  return role === "Admin";
}

export function hasPermission(
  permissions: ReadonlySet<AdminPermission>,
  permission: AdminPermission | undefined,
): boolean {
  if (!permission) return true;
  return permissions.has(permission);
}

/**
 * Whether a role can access a specific Admin route guarded by an optional
 * permission. Requires Admin access AND the specific permission (if any).
 */
export function canAccessAdminRoute(
  role: UserRole | null | undefined,
  permission?: AdminPermission,
): boolean {
  if (!canAccessAdmin(role)) return false;
  return hasPermission(getPermissionsForRole(role), permission);
}
