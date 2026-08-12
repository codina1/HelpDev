import { ADMIN_ROUTES } from "@/lib/admin/routes";
import {
  ADMIN_NAVIGATION,
  type AdminNavGroup,
  type AdminNavItem,
} from "@/lib/admin/navigation";
import { isRouteActive, normalizePath } from "@/lib/admin/route-matcher";

export type AdminBreadcrumb = {
  title: string;
  href?: string;
  current: boolean;
};

const ROOT_CRUMB = { title: "مدیریت", href: ADMIN_ROUTES.dashboard } as const;

type LocatedItem = {
  group: AdminNavGroup;
  item: AdminNavItem & { href: string };
};

function locateActiveItem(
  groups: readonly AdminNavGroup[],
  pathname: string,
): LocatedItem | null {
  let best: LocatedItem | null = null;

  for (const group of groups) {
    for (const item of group.items) {
      if (typeof item.href !== "string") continue;
      if (!isRouteActive(pathname, item.href, item.exact)) continue;
      if (!best || item.href.length > best.item.href.length) {
        best = { group, item: item as AdminNavItem & { href: string } };
      }
    }
  }

  return best;
}

/** Preferred group landing: content platform hub, else legacy all-list, else first href. */
function groupLandingHref(group: AdminNavGroup): string | undefined {
  const hub = group.items.find(
    (item) => item.id === "content-hub" && typeof item.href === "string",
  );
  if (hub?.href) return hub.href;
  const allContent = group.items.find(
    (item) => item.id === "content-all" && typeof item.href === "string",
  );
  if (allContent?.href) return allContent.href;
  return group.items.find((item) => typeof item.href === "string")?.href;
}

/**
 * Builds breadcrumbs from centralized route metadata (never by splitting the raw
 * URL). The current (last) crumb is not a link; earlier crumbs are.
 */
export function buildAdminBreadcrumbs(pathname: string): AdminBreadcrumb[] {
  const current = normalizePath(pathname);

  // The dashboard root collapses to a single, non-clickable crumb.
  if (current === normalizePath(ADMIN_ROUTES.dashboard)) {
    return [{ title: "داشبورد", current: true }];
  }

  const located = locateActiveItem(ADMIN_NAVIGATION, pathname);

  if (!located) {
    return [
      { ...ROOT_CRUMB, current: false },
      { title: "صفحه ناشناخته", current: true },
    ];
  }

  const crumbs: AdminBreadcrumb[] = [{ ...ROOT_CRUMB, current: false }];

  const landing = groupLandingHref(located.group);
  const isLeafTheLanding =
    landing && normalizePath(landing) === normalizePath(located.item.href);

  if (located.group.title && !isLeafTheLanding) {
    crumbs.push({ title: located.group.title, href: landing, current: false });
  }

  crumbs.push({ title: located.item.title, current: true });

  return crumbs;
}
