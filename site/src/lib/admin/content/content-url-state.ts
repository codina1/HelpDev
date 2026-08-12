import {
  ADMIN_CONTENT_PAGE_SIZE_DEFAULT,
  ADMIN_CONTENT_PAGE_SIZES,
  CONTENT_STATUSES,
  CONTENT_TYPES,
  DEFAULT_ADMIN_CONTENT_LIST_QUERY,
  type AdminContentListQuery,
  type AdminContentPageSize,
  type ContentStatusValue,
  type ContentTypeValue,
} from "@/lib/admin/content/content-types";

const PAGE_SIZE_SET = new Set<number>(ADMIN_CONTENT_PAGE_SIZES);

function parsePositiveInt(raw: string | null): number | null {
  if (raw == null || raw.trim() === "") return null;
  const n = Number.parseInt(raw, 10);
  return Number.isFinite(n) && n > 0 ? n : null;
}

function parsePageSize(raw: string | null): AdminContentPageSize {
  const n = parsePositiveInt(raw);
  if (n != null && PAGE_SIZE_SET.has(n)) {
    return n as AdminContentPageSize;
  }
  return ADMIN_CONTENT_PAGE_SIZE_DEFAULT;
}

function parseStatus(raw: string | null): ContentStatusValue | "all" {
  if (!raw) return "all";
  const match = CONTENT_STATUSES.find((s) => s.toLowerCase() === raw.toLowerCase());
  return match ?? "all";
}

function parseType(raw: string | null): ContentTypeValue | "all" {
  if (!raw) return "all";
  const match = CONTENT_TYPES.find((t) => t.toLowerCase() === raw.toLowerCase());
  return match ?? "all";
}

/**
 * Parses workspace URL search params into a normalized query.
 * Invalid page → 1; unsupported pageSize/status/type → defaults/omitted ("all").
 */
export function parseAdminContentListQuery(
  params: URLSearchParams | ReadonlyURLSearchParams | null | undefined,
): AdminContentListQuery {
  if (!params) return { ...DEFAULT_ADMIN_CONTENT_LIST_QUERY };

  const page = parsePositiveInt(params.get("page")) ?? 1;
  const pageSize = parsePageSize(params.get("pageSize"));
  const search = (params.get("search") ?? "").trim();
  const status = parseStatus(params.get("status"));
  const type = parseType(params.get("type"));

  return { page, pageSize, search, status, type };
}

/**
 * Serializes a query to URLSearchParams. Empty / default values are omitted
 * so shareable URLs stay short. Keys are appended in a deterministic order.
 */
export function serializeAdminContentListQuery(query: AdminContentListQuery): URLSearchParams {
  const params = new URLSearchParams();

  if (query.page > 1) params.set("page", String(query.page));
  if (query.pageSize !== ADMIN_CONTENT_PAGE_SIZE_DEFAULT) {
    params.set("pageSize", String(query.pageSize));
  }
  if (query.search.trim()) params.set("search", query.search.trim());
  if (query.status !== "all") params.set("status", query.status);
  if (query.type !== "all") params.set("type", query.type);

  return params;
}

/** Builds `/admin/content?...` (or bare path when there are no params). */
export function buildAdminContentListHref(
  query: AdminContentListQuery,
  basePath = "/admin/content",
): string {
  const params = serializeAdminContentListQuery(query);
  const qs = params.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}

/**
 * Merges a partial patch into the current query. Changing search/status/type/
 * pageSize resets page to 1 unless `page` is explicitly provided in the patch.
 */
export function mergeAdminContentListQuery(
  current: AdminContentListQuery,
  patch: Partial<AdminContentListQuery>,
): AdminContentListQuery {
  const next: AdminContentListQuery = {
    ...current,
    ...patch,
    search: patch.search !== undefined ? patch.search.trim() : current.search,
  };

  const filterChanged =
    (patch.search !== undefined && patch.search.trim() !== current.search) ||
    (patch.status !== undefined && patch.status !== current.status) ||
    (patch.type !== undefined && patch.type !== current.type) ||
    (patch.pageSize !== undefined && patch.pageSize !== current.pageSize);

  if (filterChanged && patch.page === undefined) {
    next.page = 1;
  }

  if (next.page < 1) next.page = 1;
  if (!PAGE_SIZE_SET.has(next.pageSize)) {
    next.pageSize = ADMIN_CONTENT_PAGE_SIZE_DEFAULT;
  }

  return next;
}

export function isAdminContentListFiltered(query: AdminContentListQuery): boolean {
  return (
    query.search.trim() !== "" ||
    query.status !== "all" ||
    query.type !== "all"
  );
}

/** Stable string key for request dedupe / effect deps. */
export function adminContentListQueryKey(query: AdminContentListQuery): string {
  return [
    query.page,
    query.pageSize,
    query.search.trim().toLowerCase(),
    query.status,
    query.type,
  ].join("|");
}

// Narrow ReadonlyURLSearchParams without importing Next.js types into pure utils.
type ReadonlyURLSearchParams = {
  get(name: string): string | null;
};
