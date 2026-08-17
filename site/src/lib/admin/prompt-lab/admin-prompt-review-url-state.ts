import { ADMIN_ROUTES } from "@/lib/admin/routes";
import {
  ADMIN_PROMPT_REVIEW_PAGE_SIZES,
  ADMIN_PROMPT_REVIEW_TABS,
  DEFAULT_ADMIN_PROMPT_REVIEW_QUERY,
  type AdminPromptReviewPageSize,
  type AdminPromptReviewQuery,
  type AdminPromptReviewTab,
} from "./admin-prompt-review-types";

const PAGE_SIZE_SET = new Set<number>(ADMIN_PROMPT_REVIEW_PAGE_SIZES);

function parsePositiveInt(raw: string | null): number | null {
  if (raw == null || raw.trim() === "") return null;
  const n = Number.parseInt(raw, 10);
  return Number.isFinite(n) && n > 0 ? n : null;
}

function parseTab(raw: string | null): AdminPromptReviewTab {
  if (!raw) return "pending";
  const match = ADMIN_PROMPT_REVIEW_TABS.find((tab) => tab === raw.toLowerCase());
  return match ?? "pending";
}

function parsePageSize(raw: string | null): AdminPromptReviewPageSize {
  const n = parsePositiveInt(raw);
  if (n != null && PAGE_SIZE_SET.has(n)) {
    return n as AdminPromptReviewPageSize;
  }
  return DEFAULT_ADMIN_PROMPT_REVIEW_QUERY.pageSize;
}

export function parseAdminPromptReviewQuery(
  params: { get(name: string): string | null } | null | undefined,
): AdminPromptReviewQuery {
  if (!params) return { ...DEFAULT_ADMIN_PROMPT_REVIEW_QUERY };
  return {
    tab: parseTab(params.get("tab")),
    page: parsePositiveInt(params.get("page")) ?? 1,
    pageSize: parsePageSize(params.get("pageSize")),
  };
}

export function buildAdminPromptReviewHref(
  query: AdminPromptReviewQuery,
  basePath = ADMIN_ROUTES.prompts,
): string {
  const params = new URLSearchParams();
  if (query.tab !== "pending") params.set("tab", query.tab);
  if (query.page > 1) params.set("page", String(query.page));
  if (query.pageSize !== DEFAULT_ADMIN_PROMPT_REVIEW_QUERY.pageSize) {
    params.set("pageSize", String(query.pageSize));
  }
  const qs = params.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}

export function mergeAdminPromptReviewQuery(
  current: AdminPromptReviewQuery,
  patch: Partial<AdminPromptReviewQuery>,
): AdminPromptReviewQuery {
  const next: AdminPromptReviewQuery = { ...current, ...patch };
  const filterChanged =
    (patch.tab !== undefined && patch.tab !== current.tab) ||
    (patch.pageSize !== undefined && patch.pageSize !== current.pageSize);
  if (filterChanged && patch.page === undefined) {
    next.page = 1;
  }
  if (next.page < 1) next.page = 1;
  if (!PAGE_SIZE_SET.has(next.pageSize)) {
    next.pageSize = DEFAULT_ADMIN_PROMPT_REVIEW_QUERY.pageSize;
  }
  return next;
}

export function adminPromptReviewQueryKey(query: AdminPromptReviewQuery): string {
  return `${query.tab}|${query.page}|${query.pageSize}`;
}
