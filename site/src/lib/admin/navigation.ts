import { ADMIN_ROUTES } from "@/lib/admin/routes";
import type { AdminPermission } from "@/lib/admin/permissions";
import { getPermissionsForRole, hasPermission } from "@/lib/admin/permissions";
import type { UserRole } from "@/types/auth";

/**
 * Names of Admin icons. The concrete SVGs live in the presentation layer
 * (`components/admin/shared/admin-icons`). Navigation config references icons by
 * name so the config stays serializable, pure and testable.
 */
export type AdminIconName =
  | "dashboard"
  | "content"
  | "plus"
  | "calendar"
  | "folder"
  | "tag"
  | "media"
  | "seo"
  | "learning"
  | "lessons"
  | "enrollments"
  | "progress"
  | "toolbox"
  | "runs"
  | "prompt"
  | "versions"
  | "users"
  | "roles"
  | "shield"
  | "activity"
  | "analytics"
  | "search"
  | "bell"
  | "flag"
  | "settings"
  | "audit"
  | "outbox"
  | "health"
  | "version"
  | "news"
  | "announcement"
  | "sun"
  | "moon"
  | "logout"
  | "chevron"
  | "check"
  | "command"
  | "menu"
  | "close"
  | "external"
  | "collapse"
  | "expand";

export type AdminNavTone = "neutral" | "info" | "success" | "warning" | "danger";

export type AdminNavBadge = {
  value: number | string;
  tone?: AdminNavTone;
};

/** `ready` items link to a working route; `future` items are shown disabled. */
export type AdminNavStatus = "ready" | "future";

export type AdminNavItem = {
  /** Stable, non-translated identifier. Never derive this from the title. */
  id: string;
  title: string;
  href?: string;
  icon: AdminIconName;
  children?: AdminNavItem[];
  permission?: AdminPermission;
  badge?: AdminNavBadge;
  keywords?: string[];
  status?: AdminNavStatus;
  /** When true, active matching requires an exact path match (e.g. dashboard). */
  exact?: boolean;
};

export type AdminNavGroup = {
  id: string;
  title?: string;
  items: AdminNavItem[];
};

export const ADMIN_NAVIGATION: readonly AdminNavGroup[] = [
  {
    id: "overview",
    title: "داشبورد",
    items: [
      {
        id: "dashboard",
        title: "نمای کلی",
        href: ADMIN_ROUTES.dashboard,
        icon: "dashboard",
        permission: "dashboard.view",
        exact: true,
        status: "ready",
        keywords: ["dashboard", "overview", "home", "خانه", "داشبورد", "نمای کلی"],
      },
    ],
  },
  {
    id: "content",
    title: "محتوا",
    items: [
      {
        id: "content-articles",
        title: "مقالات",
        href: ADMIN_ROUTES.contentArticles,
        icon: "content",
        permission: "content.view",
        status: "ready",
        keywords: ["articles", "مقاله", "مقالات", "article"],
      },
      {
        id: "content-news",
        title: "اخبار",
        href: ADMIN_ROUTES.contentNews,
        icon: "news",
        permission: "content.view",
        status: "ready",
        keywords: ["news", "خبر", "اخبار"],
      },
      {
        id: "content-tools",
        title: "ابزارها",
        href: ADMIN_ROUTES.contentTools,
        icon: "toolbox",
        permission: "content.view",
        status: "ready",
        keywords: ["tools", "ابزار", "ابزارها", "tool content"],
      },
      {
        id: "content-roadmaps",
        title: "نقشه راه",
        href: ADMIN_ROUTES.contentRoadmaps,
        icon: "learning",
        permission: "content.view",
        status: "ready",
        keywords: ["roadmap", "نقشه راه", "roadmapstep"],
      },
      {
        id: "content-prompts",
        title: "Prompt Lab",
        href: ADMIN_ROUTES.contentPrompts,
        icon: "prompt",
        permission: "content.view",
        status: "ready",
        keywords: ["prompt", "prompts", "پرامپت", "prompt lab"],
      },
      {
        id: "content-comparisons",
        title: "مقایسه‌ها",
        href: ADMIN_ROUTES.contentComparisons,
        icon: "analytics",
        permission: "content.view",
        status: "ready",
        keywords: ["comparison", "مقایسه", "مقایسه‌ها"],
      },
      {
        id: "content-tutorials",
        title: "آموزش‌ها",
        href: ADMIN_ROUTES.contentTutorials,
        icon: "lessons",
        permission: "content.view",
        status: "ready",
        keywords: ["tutorial", "tutorials", "آموزش", "آموزش‌ها"],
      },
      {
        id: "content-hub",
        title: "پلتفرم محتوا",
        href: ADMIN_ROUTES.content,
        icon: "dashboard",
        permission: "content.view",
        status: "ready",
        exact: true,
        keywords: ["content platform", "hub", "پلتفرم محتوا"],
      },
      {
        id: "content-all",
        title: "همه محتواها",
        href: ADMIN_ROUTES.contentAll,
        icon: "folder",
        permission: "content.view",
        status: "ready",
        keywords: ["content", "all", "همه محتواها", "فهرست کامل"],
      },
      {
        id: "content-workflows",
        title: "گردش کار AI",
        href: ADMIN_ROUTES.contentWorkflows,
        icon: "prompt",
        permission: "content.create",
        status: "ready",
        keywords: ["ai", "workflow", "idea", "draft", "گردش کار", "هوش مصنوعی"],
      },
      {
        id: "content-calendar",
        title: "تقویم انتشار",
        icon: "calendar",
        permission: "content.view",
        status: "future",
        keywords: ["calendar", "schedule", "تقویم", "زمان‌بندی"],
      },
      {
        id: "content-categories",
        title: "دسته‌بندی‌ها",
        icon: "folder",
        permission: "content.view",
        status: "future",
        keywords: ["categories", "دسته‌بندی"],
      },
      {
        id: "content-tags",
        title: "برچسب‌ها",
        icon: "tag",
        permission: "content.view",
        status: "future",
        keywords: ["tags", "برچسب"],
      },
      {
        id: "content-media",
        title: "رسانه‌ها",
        href: ADMIN_ROUTES.media,
        icon: "media",
        permission: "content.view",
        status: "ready",
        keywords: ["media", "files", "images", "رسانه", "تصاویر"],
      },
      {
        id: "content-seo",
        title: "تحلیل SEO",
        href: ADMIN_ROUTES.seo,
        icon: "seo",
        permission: "content.view",
        status: "ready",
        keywords: ["seo", "meta", "سئو"],
      },
      {
        id: "content-knowledge",
        title: "دانش جستجو",
        href: ADMIN_ROUTES.searchKnowledge,
        icon: "search",
        permission: "content.view",
        status: "ready",
        keywords: ["semantic search", "rag", "knowledge", "دانش", "جستجوی معنایی"],
      },
    ],
  },
  {
    id: "learning",
    title: "آموزش",
    items: [
      {
        id: "learning-courses",
        title: "دوره‌ها",
        href: ADMIN_ROUTES.learning,
        icon: "learning",
        permission: "learning.view",
        status: "ready",
        keywords: ["courses", "learning", "دوره", "آموزش"],
      },
      {
        id: "learning-lessons",
        title: "فصل‌ها و درس‌ها",
        icon: "lessons",
        permission: "learning.view",
        status: "future",
        keywords: ["lessons", "chapters", "درس", "فصل"],
      },
      {
        id: "learning-enrollments",
        title: "ثبت‌نام‌ها",
        icon: "enrollments",
        permission: "learning.view",
        status: "future",
        keywords: ["enrollments", "ثبت‌نام"],
      },
      {
        id: "learning-progress",
        title: "پیشرفت کاربران",
        icon: "progress",
        permission: "learning.view",
        status: "future",
        keywords: ["progress", "پیشرفت"],
      },
    ],
  },
  {
    id: "toolbox",
    title: "ابزارها",
    items: [
      {
        id: "toolbox-all",
        title: "همه ابزارها",
        href: ADMIN_ROUTES.toolbox,
        icon: "toolbox",
        permission: "toolbox.view",
        status: "ready",
        keywords: ["toolbox", "tools", "ابزار"],
      },
      {
        id: "toolbox-categories",
        title: "دسته‌بندی‌ها",
        icon: "folder",
        permission: "toolbox.view",
        status: "future",
        keywords: ["categories", "دسته‌بندی ابزار"],
      },
      {
        id: "toolbox-runs",
        title: "اجرای ابزارها",
        icon: "runs",
        permission: "toolbox.view",
        status: "future",
        keywords: ["runs", "executions", "اجرا"],
      },
    ],
  },
  {
    id: "prompt-lab",
    title: "Prompt Lab",
    items: [
      {
        id: "prompt-all",
        title: "همه پرامپت‌ها",
        href: ADMIN_ROUTES.promptLab,
        icon: "prompt",
        permission: "promptLab.view",
        status: "ready",
        keywords: ["prompt", "prompts", "پرامپت"],
      },
      {
        id: "prompt-review",
        title: "بازبینی پرامپت‌ها",
        href: ADMIN_ROUTES.prompts,
        icon: "flag",
        permission: "promptLab.view",
        status: "ready",
        keywords: ["review", "approve", "reject", "بازبینی", "تأیید", "رد"],
      },
      {
        id: "prompt-versions",
        title: "نسخه‌ها",
        icon: "versions",
        permission: "promptLab.view",
        status: "future",
        keywords: ["versions", "نسخه"],
      },
      {
        id: "prompt-categories",
        title: "دسته‌بندی‌ها",
        icon: "folder",
        permission: "promptLab.view",
        status: "future",
        keywords: ["categories", "دسته‌بندی پرامپت"],
      },
    ],
  },
  {
    id: "users",
    title: "کاربران و دسترسی",
    items: [
      {
        id: "users-all",
        title: "کاربران",
        href: ADMIN_ROUTES.users,
        icon: "users",
        permission: "users.view",
        status: "ready",
        keywords: ["users", "members", "کاربر", "اعضا"],
      },
      {
        id: "users-roles",
        title: "نقش‌ها",
        icon: "roles",
        permission: "users.view",
        status: "future",
        keywords: ["roles", "نقش"],
      },
      {
        id: "users-permissions",
        title: "دسترسی‌ها",
        icon: "shield",
        permission: "users.view",
        status: "future",
        keywords: ["permissions", "access", "دسترسی"],
      },
      {
        id: "users-activity",
        title: "فعالیت کاربران",
        icon: "activity",
        permission: "users.view",
        status: "future",
        keywords: ["activity", "فعالیت"],
      },
    ],
  },
  {
    id: "analytics",
    title: "تحلیل‌ها",
    items: [
      {
        id: "analytics-overview",
        title: "نمای کلی",
        href: ADMIN_ROUTES.analytics,
        icon: "analytics",
        permission: "analytics.view",
        status: "ready",
        keywords: ["analytics", "metrics", "تحلیل", "آمار"],
      },
      {
        id: "analytics-content",
        title: "محتوا",
        href: ADMIN_ROUTES.analyticsContent,
        icon: "content",
        permission: "analytics.view",
        status: "ready",
        keywords: ["content analytics", "تحلیل محتوا"],
      },
      {
        id: "analytics-ai",
        title: "AI",
        href: ADMIN_ROUTES.ai,
        icon: "activity",
        permission: "analytics.view",
        status: "ready",
        keywords: ["ai", "llm", "هوش مصنوعی", "operations"],
      },
      {
        id: "analytics-search",
        title: "جستجو",
        icon: "search",
        permission: "analytics.view",
        status: "future",
        keywords: ["search analytics", "تحلیل جستجو"],
      },
      {
        id: "analytics-learning",
        title: "آموزش",
        icon: "learning",
        permission: "analytics.view",
        status: "future",
        keywords: ["learning analytics", "تحلیل آموزش"],
      },
      {
        id: "analytics-toolbox",
        title: "ابزارها",
        icon: "toolbox",
        permission: "analytics.view",
        status: "future",
        keywords: ["toolbox analytics", "تحلیل ابزار"],
      },
      {
        id: "analytics-prompt",
        title: "Prompt Lab",
        icon: "prompt",
        permission: "analytics.view",
        status: "future",
        keywords: ["prompt analytics", "تحلیل پرامپت"],
      },
    ],
  },
  {
    id: "system",
    title: "سیستم",
    items: [
      {
        id: "system-announcements",
        title: "اعلان‌ها",
        icon: "announcement",
        permission: "system.view",
        status: "future",
        keywords: ["announcements", "اعلان"],
      },
      {
        id: "system-feature-flags",
        title: "Feature Flags",
        icon: "flag",
        permission: "system.view",
        status: "future",
        keywords: ["feature flags", "flags", "فلگ"],
      },
      {
        id: "system-about",
        title: "درباره ما",
        href: ADMIN_ROUTES.about,
        icon: "announcement",
        permission: "system.view",
        status: "ready",
        keywords: ["about", "درباره", "درباره ما"],
      },
      {
        id: "system-settings",
        title: "تنظیمات",
        href: ADMIN_ROUTES.settings,
        icon: "settings",
        permission: "system.view",
        status: "ready",
        keywords: ["settings", "configuration", "تنظیمات"],
      },
      {
        id: "system-audit",
        title: "Audit",
        href: ADMIN_ROUTES.audit,
        icon: "audit",
        permission: "system.view",
        status: "ready",
        keywords: ["audit", "logs", "ممیزی", "گزارش"],
      },
      {
        id: "system-health",
        title: "سلامت سیستم",
        href: ADMIN_ROUTES.operations,
        icon: "health",
        permission: "system.view",
        status: "ready",
        keywords: ["health", "operations", "status", "سلامت", "عملیات"],
      },
      {
        id: "system-outbox",
        title: "Outbox",
        icon: "outbox",
        permission: "system.view",
        status: "future",
        keywords: ["outbox", "messaging", "صف پیام"],
      },
      {
        id: "system-version",
        title: "نسخه و انتشار",
        icon: "version",
        permission: "system.view",
        status: "future",
        keywords: ["version", "release", "نسخه", "انتشار"],
      },
    ],
  },
] as const;

/** Depth allowed for nav items (top-level item + one nested level). */
export const MAX_NAV_DEPTH = 2;

/**
 * Filters navigation for a given role, keeping only items the role is permitted
 * to see. Children are filtered recursively and empty groups are dropped.
 * This is UX only — the backend still authorizes every request.
 */
export function filterAdminNavigation(
  groups: readonly AdminNavGroup[],
  role: UserRole | null | undefined,
): AdminNavGroup[] {
  const permissions = getPermissionsForRole(role);

  const filterItems = (items: readonly AdminNavItem[]): AdminNavItem[] =>
    items
      .filter((item) => hasPermission(permissions, item.permission))
      .map((item) =>
        item.children
          ? { ...item, children: filterItems(item.children) }
          : item,
      );

  return groups
    .map((group) => ({ ...group, items: filterItems(group.items) }))
    .filter((group) => group.items.length > 0);
}

/** Flattens all nav items (including nested children) into a single list. */
export function flattenNavItems(
  groups: readonly AdminNavGroup[],
): AdminNavItem[] {
  const out: AdminNavItem[] = [];
  const walk = (items: readonly AdminNavItem[]) => {
    for (const item of items) {
      out.push(item);
      if (item.children) walk(item.children);
    }
  };
  for (const group of groups) walk(group.items);
  return out;
}
