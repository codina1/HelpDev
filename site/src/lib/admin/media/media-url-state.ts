import {
  ADMIN_MEDIA_PAGE_SIZE_DEFAULT,
  ADMIN_MEDIA_PAGE_SIZES,
  DEFAULT_ADMIN_MEDIA_LIST_QUERY,
  type AdminMediaListQuery,
  type AdminMediaPageSize,
} from "@/lib/admin/media/media-types";

const PAGE_SIZE_SET = new Set<number>(ADMIN_MEDIA_PAGE_SIZES);

function parsePositiveInt(raw: string | null): number | null {
  if (raw == null || raw.trim() === "") return null;
  const n = Number.parseInt(raw, 10);
  return Number.isFinite(n) && n > 0 ? n : null;
}

function parsePageSize(raw: string | null): AdminMediaPageSize {
  const n = parsePositiveInt(raw);
  if (n != null && PAGE_SIZE_SET.has(n)) {
    return n as AdminMediaPageSize;
  }
  return ADMIN_MEDIA_PAGE_SIZE_DEFAULT;
}

/**
 * Parses workspace URL search params into a normalized query.
 * Invalid page → 1; unsupported pageSize → default.
 */
export function parseAdminMediaListQuery(
  params: URLSearchParams | ReadonlyURLSearchParams | null | undefined,
): AdminMediaListQuery {
  if (!params) return { ...DEFAULT_ADMIN_MEDIA_LIST_QUERY };

  const page = parsePositiveInt(params.get("page")) ?? 1;
  const pageSize = parsePageSize(params.get("pageSize"));
  const search = (params.get("search") ?? "").trim();

  return { page, pageSize, search };
}

/**
 * Serializes a query to URLSearchParams. Empty/default values are omitted so
 * shareable URLs stay short. Keys are appended in a deterministic order.
 */
export function serializeAdminMediaListQuery(query: AdminMediaListQuery): URLSearchParams {
  const params = new URLSearchParams();

  if (query.page > 1) params.set("page", String(query.page));
  if (query.pageSize !== ADMIN_MEDIA_PAGE_SIZE_DEFAULT) {
    params.set("pageSize", String(query.pageSize));
  }
  if (query.search.trim()) params.set("search", query.search.trim());

  return params;
}

/** Builds `/admin/media?...` (or bare path when there are no params). */
export function buildAdminMediaListHref(
  query: AdminMediaListQuery,
  basePath = "/admin/media",
): string {
  const params = serializeAdminMediaListQuery(query);
  const qs = params.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}

/**
 * Merges a partial patch into the current query. Changing search/pageSize
 * resets page to 1 unless `page` is explicitly provided in the patch.
 */
export function mergeAdminMediaListQuery(
  current: AdminMediaListQuery,
  patch: Partial<AdminMediaListQuery>,
): AdminMediaListQuery {
  const next: AdminMediaListQuery = {
    ...current,
    ...patch,
    search: patch.search !== undefined ? patch.search.trim() : current.search,
  };

  const filterChanged =
    (patch.search !== undefined && patch.search.trim() !== current.search) ||
    (patch.pageSize !== undefined && patch.pageSize !== current.pageSize);

  if (filterChanged && patch.page === undefined) {
    next.page = 1;
  }

  if (next.page < 1) next.page = 1;
  if (!PAGE_SIZE_SET.has(next.pageSize)) {
    next.pageSize = ADMIN_MEDIA_PAGE_SIZE_DEFAULT;
  }

  return next;
}

export function isAdminMediaListFiltered(query: AdminMediaListQuery): boolean {
  return query.search.trim() !== "";
}

/** Stable string key for request dedupe / effect deps. */
export function adminMediaListQueryKey(query: AdminMediaListQuery): string {
  return [query.page, query.pageSize, query.search.trim().toLowerCase()].join("|");
}

// Narrow ReadonlyURLSearchParams without importing Next.js types into pure utils.
type ReadonlyURLSearchParams = {
  get(name: string): string | null;
};
