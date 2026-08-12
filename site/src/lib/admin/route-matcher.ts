import {
  flattenNavItems,
  type AdminNavGroup,
  type AdminNavItem,
} from "@/lib/admin/navigation";

/** Strips query/hash and any trailing slash (except the root "/"). */
export function normalizePath(pathname: string): string {
  const path = pathname.split("?")[0].split("#")[0];
  if (path.length > 1) {
    return path.replace(/\/+$/, "");
  }
  return path;
}

/**
 * Active-route matching.
 *
 * - `exact` items (e.g. the dashboard `/admin`) match only their own path.
 * - Non-exact items match their path and any nested sub-path, but never a
 *   sibling that merely shares a string prefix (`/admin/content` does not match
 *   `/admin/content-archive`).
 * - Query strings never affect matching.
 */
export function isRouteActive(
  pathname: string,
  href: string,
  exact = false,
): boolean {
  const current = normalizePath(pathname);
  const target = normalizePath(href);

  if (current === target) return true;
  if (exact) return false;

  return current.startsWith(`${target}/`);
}

/**
 * Returns the id of the single most-specific active nav item for a pathname, or
 * `null` when nothing matches. Specificity is decided by the longest matching
 * href so deep routes highlight the leaf rather than an ancestor.
 */
export function findActiveNavItemId(
  groups: readonly AdminNavGroup[],
  pathname: string,
): string | null {
  const candidates = flattenNavItems(groups).filter(
    (item): item is AdminNavItem & { href: string } =>
      typeof item.href === "string" &&
      isRouteActive(pathname, item.href, item.exact),
  );

  if (candidates.length === 0) return null;

  return candidates.reduce((best, item) =>
    item.href.length > best.href.length ? item : best,
  ).id;
}

/** True when any item within a group is active for the pathname. */
export function isGroupActive(
  group: AdminNavGroup,
  pathname: string,
): boolean {
  const activeId = findActiveNavItemId([group], pathname);
  return activeId !== null;
}
