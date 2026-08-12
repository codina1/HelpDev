/**
 * Centralized Admin route definitions.
 *
 * Every Admin URL used by the shell, navigation, breadcrumbs and command menu
 * must be sourced from here. Components must not hardcode scattered route
 * strings. This keeps the route tree in one place and makes refactors safe.
 */

export const ADMIN_BASE = "/admin";

export const ADMIN_ROUTES = {
  dashboard: "/admin",
  content: "/admin/content",
  contentNew: "/admin/content/new",
  contentWorkflows: "/admin/content/workflows",
  /** Content workspaces (CMS UX) — reuse existing Content APIs. */
  contentArticles: "/admin/content/articles",
  contentArticlesNew: "/admin/content/articles/new",
  contentNews: "/admin/content/news",
  contentNewsNew: "/admin/content/news/new",
  contentTools: "/admin/content/tools",
  contentToolsNew: "/admin/content/tools/new",
  contentRoadmaps: "/admin/content/roadmaps",
  contentRoadmapsNew: "/admin/content/roadmaps/new",
  contentPrompts: "/admin/content/prompts",
  contentPromptsNew: "/admin/content/prompts/new",
  contentComparisons: "/admin/content/comparisons",
  contentComparisonsNew: "/admin/content/comparisons/new",
  contentTutorials: "/admin/content/tutorials",
  contentTutorialsNew: "/admin/content/tutorials/new",
  /** Legacy full content list (kept; hub lives at /admin/content). */
  contentAll: "/admin/content/all",
  media: "/admin/media",
  seo: "/admin/seo",
  learning: "/admin/learning",
  toolbox: "/admin/toolbox",
  promptLab: "/admin/prompt-lab",
  users: "/admin/users",
  analytics: "/admin/analytics",
  analyticsContent: "/admin/analytics/content",
  ai: "/admin/ai",
  searchKnowledge: "/admin/search/knowledge",
  audit: "/admin/audit",
  operations: "/admin/operations",
  settings: "/admin/settings",
} as const;

export type AdminRouteKey = keyof typeof ADMIN_ROUTES;
export type AdminRoute = (typeof ADMIN_ROUTES)[AdminRouteKey];

export function adminContentWorkflowRoute(id: string): string {
  return `${ADMIN_ROUTES.contentWorkflows}/${encodeURIComponent(id)}`;
}

export function adminContentArticleRoute(id: string): string {
  return `${ADMIN_ROUTES.contentArticles}/${encodeURIComponent(id)}`;
}

export function adminContentNewsRoute(id: string): string {
  return `${ADMIN_ROUTES.contentNews}/${encodeURIComponent(id)}`;
}

export function adminContentItemRoute(id: string): string {
  return `${ADMIN_ROUTES.content}/${encodeURIComponent(id)}`;
}

/** Route to the normal (non-admin) user panel. */
export const USER_PANEL_ROUTE = "/profile";

/** Home route; hosts the login modal used for admin re-authentication. */
export const HOME_ROUTE = "/";

export function adminUserDetailRoute(userId: string): string {
  return `${ADMIN_ROUTES.users}/${encodeURIComponent(userId)}`;
}

/** True when the given pathname belongs to the Admin area. */
export function isAdminPath(pathname: string): boolean {
  return pathname === ADMIN_BASE || pathname.startsWith(`${ADMIN_BASE}/`);
}

/**
 * Validates a post-login return URL. Only same-origin, in-app Admin paths are
 * allowed to prevent open-redirect / href injection. External URLs, protocol
 * hrefs and protocol-relative (`//host`) URLs are rejected.
 */
export function isSafeAdminReturnUrl(url: string | null | undefined): boolean {
  if (!url) return false;
  if (!url.startsWith("/")) return false;
  if (url.startsWith("//")) return false;
  if (url.includes("\\")) return false;
  return isAdminPath(url.split("?")[0].split("#")[0]);
}

/** Builds a safe login URL that returns to the given admin path after auth. */
export function buildAdminLoginUrl(returnUrl: string): string {
  const safe = isSafeAdminReturnUrl(returnUrl) ? returnUrl : ADMIN_ROUTES.dashboard;
  const params = new URLSearchParams({ next: safe });
  return `${HOME_ROUTE}?${params.toString()}`;
}
