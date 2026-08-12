import {
  analyzeContentSeo as analyzeContentSeoRequest,
  createContent as createContentRequest,
  getAdminContentById as getAdminContentByIdRequest,
  getAdminContentList as getAdminContentListRequest,
  getArticleMetadata as getArticleMetadataRequest,
  getContentBySlug as getContentBySlugRequest,
  getNewsMetadata as getNewsMetadataRequest,
  listPublishedContent,
  publishContent as publishContentRequest,
  runContentAiAction as runContentAiActionRequest,
  updateArticleMetadata as updateArticleMetadataRequest,
  updateContent as updateContentRequest,
  updateContentSeoMetadata as updateContentSeoMetadataRequest,
  updateNewsMetadata as updateNewsMetadataRequest,
  type AdminContentListOptions,
  type ContentAiAction,
  type ContentAiResultDto,
} from "@/lib/api/content";
import { fetchDashboardOverview } from "@/lib/admin/dashboard/dashboard-api";
import type {
  AdminContentDetailRawDto,
  AdminContentListQuery,
  AdminContentPagedResultRawDto,
  ArticleMetadataRawDto,
  ContentDetailRawDto,
  ContentListRawDto,
  ContentStats,
  CreateContentPayload,
  NewsMetadataRawDto,
  SeoAuditReportRawDto,
  UpdateArticleMetadataPayload,
  UpdateContentPayload,
  UpdateNewsMetadataPayload,
  UpdateSeoMetadataPayload,
} from "@/lib/admin/content/content-types";
import { mapContentStats } from "@/lib/admin/content/content-mappers";

/**
 * Content CMS data access. Reuses the shared typed API client via the existing
 * `@/lib/api/content` module (no duplicated fetch logic) and targets only
 * canonical `/api/v1` routes. The Admin workspace list uses
 * `GET /admin/content` (not the public published-only list).
 */

export const CONTENT_CAPABILITIES = {
  list: true,
  adminList: true,
  getBySlug: true,
  getById: true,
  create: true,
  update: true,
  publishExisting: true,
  seo: true,
  seoAnalysis: true,
  aiAssistant: true,
  articleMetadata: true,
  newsMetadata: true,
  toolLibrary: true,
  roadmapEngine: true,
} as const;

/** Raised when a not-yet-supported content operation is invoked. */
export class ContentOperationUnsupportedError extends Error {
  constructor(message = "این عملیات هنوز توسط سرور پشتیبانی نمی‌شود.") {
    super(message);
    this.name = "ContentOperationUnsupportedError";
  }
}

/** Maps the workspace query to API options (omits empty filters). */
export function toAdminContentListOptions(query: AdminContentListQuery): AdminContentListOptions {
  return {
    page: query.page,
    pageSize: query.pageSize,
    search: query.search.trim() || undefined,
    status: query.status === "all" ? undefined : query.status,
    type: query.type === "all" ? undefined : query.type,
  };
}

// GET /content (published only) — public site / legacy; NOT used by Admin workspace.
export async function fetchContentList(
  signal?: AbortSignal,
): Promise<ContentListRawDto[]> {
  const items = await listPublishedContent(signal);
  return items as unknown as ContentListRawDto[];
}

// GET /admin/content — Admin CMS paged list (drafts + published, ownership-scoped).
export async function fetchAdminContentList(
  token: string,
  query: AdminContentListQuery,
  signal?: AbortSignal,
): Promise<AdminContentPagedResultRawDto> {
  const result = await getAdminContentListRequest(
    token,
    toAdminContentListOptions(query),
    signal,
  );
  return result as unknown as AdminContentPagedResultRawDto;
}

// GET /content/{slug} (published only).
export async function fetchContentBySlug(
  slug: string,
  signal?: AbortSignal,
): Promise<ContentDetailRawDto> {
  const detail = await getContentBySlugRequest(slug, signal);
  return detail as unknown as ContentDetailRawDto;
}

// GET /admin/content/{id} — Admin Read Model (drafts + SEO + timestamps).
export async function fetchAdminContentById(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await getAdminContentByIdRequest(token, id, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

// POST /content — supports Status = "Draft" | "Published" at creation time.
export async function createContentItem(
  token: string,
  payload: CreateContentPayload,
  signal?: AbortSignal,
): Promise<ContentDetailRawDto> {
  const detail = await createContentRequest(token, payload, signal);
  return detail as unknown as ContentDetailRawDto;
}

// PUT /admin/content/{id} — edit an existing item (writer/admin, ownership enforced).
export async function updateContentItem(
  token: string,
  id: string,
  payload: UpdateContentPayload,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await updateContentRequest(token, id, payload, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

// POST /admin/content/{id}/publish — Draft → Published (idempotent).
export async function publishContentItem(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await publishContentRequest(token, id, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

// PUT /admin/content/{id}/seo — update SEO metadata.
export async function updateSeoMetadata(
  token: string,
  id: string,
  payload: UpdateSeoMetadataPayload,
  signal?: AbortSignal,
): Promise<AdminContentDetailRawDto> {
  const detail = await updateContentSeoMetadataRequest(token, id, payload, signal);
  return detail as unknown as AdminContentDetailRawDto;
}

export async function fetchArticleMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<ArticleMetadataRawDto | null> {
  const dto = await getArticleMetadataRequest(token, id, signal);
  return (dto ?? null) as ArticleMetadataRawDto | null;
}

export async function updateArticleMetadata(
  token: string,
  id: string,
  payload: UpdateArticleMetadataPayload,
  signal?: AbortSignal,
): Promise<ArticleMetadataRawDto> {
  const dto = await updateArticleMetadataRequest(token, id, payload, signal);
  return dto as unknown as ArticleMetadataRawDto;
}

export async function fetchNewsMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<NewsMetadataRawDto | null> {
  const dto = await getNewsMetadataRequest(token, id, signal);
  return (dto ?? null) as NewsMetadataRawDto | null;
}

export async function updateNewsMetadata(
  token: string,
  id: string,
  payload: UpdateNewsMetadataPayload,
  signal?: AbortSignal,
): Promise<NewsMetadataRawDto> {
  const dto = await updateNewsMetadataRequest(token, id, payload, signal);
  return dto as unknown as NewsMetadataRawDto;
}

// POST /admin/content/{id}/seo-analysis — analyzes the SAVED server version
// only. An explicit user action (Analyze/Rerun); never invoked on keystroke.
export async function analyzeContentSeo(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<SeoAuditReportRawDto> {
  const report = await analyzeContentSeoRequest(token, id, signal);
  return report as unknown as SeoAuditReportRawDto;
}

/** POST /admin/content/{id}/ai/{action} — suggestion only; never auto-applies. */
export async function runContentAi(
  token: string,
  id: string,
  action: ContentAiAction,
  signal?: AbortSignal,
): Promise<ContentAiResultDto> {
  return runContentAiActionRequest(token, id, action, signal);
}

// Content statistics reuse the existing admin dashboard endpoint.
export async function fetchContentStats(
  token: string,
  signal?: AbortSignal,
): Promise<ContentStats> {
  const dashboard = await fetchDashboardOverview(token, signal);
  return mapContentStats(dashboard);
}
