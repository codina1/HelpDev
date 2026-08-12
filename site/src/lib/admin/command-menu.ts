import { ADMIN_ROUTES } from "@/lib/admin/routes";
import {
  flattenNavItems,
  type AdminIconName,
  type AdminNavGroup,
} from "@/lib/admin/navigation";
import { hasPermission, type AdminPermission } from "@/lib/admin/permissions";

export type AdminCommandKind = "navigate" | "create";

export type AdminCommand = {
  id: string;
  title: string;
  subtitle?: string;
  href: string;
  icon: AdminIconName;
  kind: AdminCommandKind;
  keywords: string[];
  permission?: AdminPermission;
};

export type AdminQuickCreateItem = {
  id: string;
  title: string;
  href?: string;
  icon: AdminIconName;
  permission?: AdminPermission;
  status: "ready" | "future";
};

/**
 * Quick-create targets. Only entries with a real route are actionable; future
 * ones are shown disabled and clearly labelled. Routes are sourced centrally.
 */
export const ADMIN_QUICK_CREATE: readonly AdminQuickCreateItem[] = [
  {
    id: "create-article",
    title: "مقاله",
    href: ADMIN_ROUTES.contentArticlesNew,
    icon: "content",
    permission: "content.create",
    status: "ready",
  },
  {
    id: "create-news",
    title: "خبر",
    href: ADMIN_ROUTES.contentNewsNew,
    icon: "news",
    permission: "content.create",
    status: "ready",
  },
  {
    id: "create-course",
    title: "دوره",
    icon: "learning",
    permission: "learning.view",
    status: "future",
  },
  {
    id: "create-tool",
    title: "ابزار",
    href: ADMIN_ROUTES.contentToolsNew,
    icon: "toolbox",
    permission: "content.create",
    status: "ready",
  },
  {
    id: "create-prompt",
    title: "Prompt",
    href: ADMIN_ROUTES.contentPromptsNew,
    icon: "prompt",
    permission: "promptLab.view",
    status: "ready",
  },
  {
    id: "create-announcement",
    title: "اعلان",
    icon: "announcement",
    permission: "system.view",
    status: "future",
  },
] as const;

/**
 * Normalizes text for tolerant Persian/English search: lowercases, unifies
 * Arabic/Persian yeh & kaf, strips Arabic diacritics and zero-width joiners,
 * and collapses whitespace.
 */
export function normalizeSearchText(value: string): string {
  return value
    .toLowerCase()
    .replace(/[\u064A\u0649]/g, "\u06CC") // arabic yeh/alef-maksura -> farsi yeh
    .replace(/\u0643/g, "\u06A9") // arabic kaf -> farsi keheh
    .replace(/[\u064B-\u0652\u200C\u200D]/g, "") // diacritics + zero-width chars
    .replace(/\s+/g, " ")
    .trim();
}

/**
 * Builds the command registry from the (already permission-filtered) navigation
 * plus quick-create actions. Only `ready` routes become commands, and
 * quick-create commands are additionally gated by the supplied permission set
 * so a non-admin never sees create actions.
 */
export function buildCommandRegistry(
  groups: readonly AdminNavGroup[],
  permissions: ReadonlySet<AdminPermission> = new Set<AdminPermission>(),
): AdminCommand[] {
  const commands: AdminCommand[] = [];

  for (const item of flattenNavItems(groups)) {
    if (item.status === "future") continue;
    if (typeof item.href !== "string") continue;

    commands.push({
      id: `nav:${item.id}`,
      title: item.title,
      subtitle: "رفتن به",
      href: item.href,
      icon: item.icon,
      kind: "navigate",
      keywords: item.keywords ?? [],
      permission: item.permission,
    });
  }

  for (const quick of ADMIN_QUICK_CREATE) {
    if (quick.status === "future") continue;
    if (typeof quick.href !== "string") continue;
    if (!hasPermission(permissions, quick.permission)) continue;

    commands.push({
      id: `create:${quick.id}`,
      title: `ایجاد ${quick.title}`,
      subtitle: "ایجاد سریع",
      href: quick.href,
      icon: quick.icon,
      kind: "create",
      keywords: [quick.title, "create", "new", "ایجاد"],
      permission: quick.permission,
    });
  }

  return commands;
}

/** Filters commands by a normalized query over title, subtitle and keywords. */
export function searchCommands(
  commands: readonly AdminCommand[],
  query: string,
): AdminCommand[] {
  const q = normalizeSearchText(query);
  if (!q) return [...commands];

  return commands.filter((command) => {
    const haystack = normalizeSearchText(
      [command.title, command.subtitle ?? "", ...command.keywords].join(" "),
    );
    return haystack.includes(q);
  });
}
