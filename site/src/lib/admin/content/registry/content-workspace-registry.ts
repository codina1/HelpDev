import { ADMIN_ROUTES } from "@/lib/admin/routes";
import type { AdminIconName } from "@/lib/admin/navigation";
import type { ContentTypeValue } from "@/lib/admin/content/content-types";

/**
 * Sprint 47A — Content Platform Workspace Registry (metadata).
 * Editor/list React components are wired in the presentation layer to avoid
 * circular imports; this module stays pure and testable.
 */

export type ContentWorkspaceId =
  | "article"
  | "news"
  | "tool"
  | "roadmap"
  | "prompt"
  | "comparison"
  | "tutorial";

/** How the workspace persists data — never invent APIs. */
export type ContentWorkspacePersistence =
  | "content-api"
  | "prompt-lab"
  | "none";

export type ContentWorkspaceRegistryEntry = {
  id: ContentWorkspaceId;
  title: string;
  shortTitle: string;
  description: string;
  route: string;
  createRoute: string;
  icon: AdminIconName;
  persistence: ContentWorkspacePersistence;
  /** Backend ContentType when persistence === content-api */
  contentType?: ContentTypeValue;
  createLabel: string;
};

export const CONTENT_WORKSPACE_IDS = [
  "article",
  "news",
  "tool",
  "roadmap",
  "prompt",
  "comparison",
  "tutorial",
] as const satisfies readonly ContentWorkspaceId[];

export const ContentWorkspaceRegistry: Record<
  ContentWorkspaceId,
  ContentWorkspaceRegistryEntry
> = {
  article: {
    id: "article",
    title: "مدیریت مقالات",
    shortTitle: "مقالات",
    description: "مقالات Markdown با SEO و رسانه — روی API محتوا.",
    route: ADMIN_ROUTES.contentArticles,
    createRoute: ADMIN_ROUTES.contentArticlesNew,
    icon: "content",
    persistence: "content-api",
    contentType: "Article",
    createLabel: "مقاله جدید",
  },
  news: {
    id: "news",
    title: "مدیریت اخبار",
    shortTitle: "اخبار",
    description: "اخبار و اطلاعیه‌های کوتاه — روی API محتوا.",
    route: ADMIN_ROUTES.contentNews,
    createRoute: ADMIN_ROUTES.contentNewsNew,
    icon: "news",
    persistence: "content-api",
    contentType: "News",
    createLabel: "خبر جدید",
  },
  tool: {
    id: "tool",
    title: "مدیریت ابزارها",
    shortTitle: "ابزارها",
    description: "کاتالوگ ابزار روی Content Core + Tool Metadata (ویژگی‌ها و جایگزین‌ها).",
    route: ADMIN_ROUTES.contentTools,
    createRoute: ADMIN_ROUTES.contentToolsNew,
    icon: "toolbox",
    persistence: "content-api",
    contentType: "Tool",
    createLabel: "ابزار جدید",
  },
  roadmap: {
    id: "roadmap",
    title: "سازندهٔ نقشه راه",
    shortTitle: "نقشه راه",
    description: "سازندهٔ فازبندی‌شده روی Content Core + Roadmap Metadata (گام، موضوع، منبع).",
    route: ADMIN_ROUTES.contentRoadmaps,
    createRoute: ADMIN_ROUTES.contentRoadmapsNew,
    icon: "learning",
    persistence: "content-api",
    contentType: "Roadmap",
    createLabel: "نقشه راه جدید",
  },
  prompt: {
    id: "prompt",
    title: "Prompt Lab",
    shortTitle: "Prompt Lab",
    description: "مدیریت پرامپت در ماژول Prompt Lab — بدون تکرار API.",
    route: ADMIN_ROUTES.contentPrompts,
    createRoute: ADMIN_ROUTES.contentPromptsNew,
    icon: "prompt",
    persistence: "prompt-lab",
    createLabel: "باز کردن Prompt Lab",
  },
  comparison: {
    id: "comparison",
    title: "مقایسه‌ها",
    shortTitle: "مقایسه‌ها",
    description: "فضای کار مقایسهٔ ابزارها/فناوری‌ها — فاندیشن UI بدون ذخیره.",
    route: ADMIN_ROUTES.contentComparisons,
    createRoute: ADMIN_ROUTES.contentComparisonsNew,
    icon: "analytics",
    persistence: "none",
    createLabel: "مقایسه جدید",
  },
  tutorial: {
    id: "tutorial",
    title: "آموزش‌های کوتاه",
    shortTitle: "آموزش‌ها",
    description: "محتوای آموزشی Markdown با SEO و رسانه — روی API محتوا.",
    route: ADMIN_ROUTES.contentTutorials,
    createRoute: ADMIN_ROUTES.contentTutorialsNew,
    icon: "lessons",
    persistence: "content-api",
    contentType: "Course",
    createLabel: "آموزش جدید",
  },
};

export function getContentWorkspace(
  id: ContentWorkspaceId,
): ContentWorkspaceRegistryEntry {
  return ContentWorkspaceRegistry[id];
}

export function listContentWorkspaces(): ContentWorkspaceRegistryEntry[] {
  return CONTENT_WORKSPACE_IDS.map((id) => ContentWorkspaceRegistry[id]);
}

export function getWorkspaceByRoute(
  pathname: string,
): ContentWorkspaceRegistryEntry | undefined {
  const normalized = pathname.replace(/\/$/, "");
  return listContentWorkspaces().find(
    (w) =>
      normalized === w.route ||
      normalized.startsWith(`${w.route}/`) ||
      normalized === w.createRoute,
  );
}
