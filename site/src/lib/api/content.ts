import { apiRequest } from "./client";

export type ContentSummaryDto = {
  id: string;
  title: string;
  slug: string;
  type: string;
  status: string;
  views: number;
  saves: number;
  createdAt: string;
  coverImage?: string | null;
};

export type ContentDetailDto = ContentSummaryDto & {
  body: string;
  authorId: string;
  contentHtml?: string | null;
  contentFormat?: string | null;
  wordCount?: number | null;
  readingTimeMinutes?: number | null;
};

export type CreateContentRequestDto = {
  title: string;
  slug: string;
  body: string;
  type: string;
  status: string;
};

export type SeoMetadataDto = {
  seoTitle: string | null;
  seoDescription: string | null;
  canonicalUrl: string | null;
  ogImage: string | null;
  focusKeyword: string | null;
};

// AdminContentDetailDto — returned by admin update / publish / seo endpoints.
export type AdminContentDetailDto = {
  id: string;
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  coverImage: string | null;
  contentType: string;
  contentStatus: string;
  authorId: string;
  views: number;
  saves: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  publishedAtUtc: string | null;
  seo: SeoMetadataDto;
  contentJson?: string | null;
  contentHtml?: string | null;
  contentFormat?: string | null;
  editorVersion?: string | null;
  wordCount?: number | null;
  readingTimeMinutes?: number | null;
  lastAutosavedAtUtc?: string | null;
};

export type UpdateContentRequestDto = {
  title: string;
  slug: string;
  type: string;
  body: string;
  excerpt: string | null;
  coverImage: string | null;
  contentJson?: string | null;
  contentFormat?: string | null;
  editorVersion?: string | null;
  autosave?: boolean;
};

export type PreviewArticleRequestDto = {
  contentJson?: string | null;
  body?: string | null;
};

export type PreviewHeadingDto = {
  id: string;
  level: number;
  text: string;
};

export type PreviewArticleDto = {
  html: string;
  plainText: string;
  wordCount: number;
  readingTimeMinutes: number;
  headings: PreviewHeadingDto[];
};

export type UpdateSeoMetadataRequestDto = {
  seoTitle: string | null;
  seoDescription: string | null;
  canonicalUrl: string | null;
  ogImage: string | null;
  focusKeyword: string | null;
};

export type ArticleMetadataDto = {
  id: string;
  contentId: string;
  categoryId: string | null;
  difficultyLevel: string;
  readingTimeMinutes: number;
  isFeatured: boolean;
  allowComments: boolean;
  tableOfContentsEnabled: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type UpdateArticleMetadataRequestDto = {
  categoryId: string | null;
  difficultyLevel: string;
  readingTimeMinutes: number;
  isFeatured: boolean;
  allowComments: boolean;
  tableOfContentsEnabled: boolean;
};

export type NewsMetadataDto = {
  id: string;
  contentId: string;
  sourceName: string;
  sourceUrl: string | null;
  newsDateUtc: string;
  priority: string;
  externalReference: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type UpdateNewsMetadataRequestDto = {
  sourceName: string;
  sourceUrl: string | null;
  newsDateUtc: string;
  priority: string;
  externalReference: string | null;
};

export type ToolFeatureDto = {
  id: string;
  title: string;
  description: string | null;
  order: number;
};

export type ToolAlternativeDto = {
  id: string;
  alternativeToolContentId: string;
  alternativeToolName: string | null;
  alternativeToolSlug: string | null;
  order: number;
};

export type ToolDetailDto = {
  id: string;
  contentId: string;
  toolName: string;
  officialWebsiteUrl: string;
  githubUrl: string | null;
  logoMediaId: string | null;
  companyName: string | null;
  pricingModel: string;
  toolCategory: string;
  platforms: string[];
  licenseType: string;
  features: ToolFeatureDto[];
  alternatives: ToolAlternativeDto[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type UpdateToolRequestDto = {
  toolName: string;
  officialWebsiteUrl: string;
  githubUrl: string | null;
  logoMediaId: string | null;
  companyName: string | null;
  pricingModel: string;
  toolCategory: string;
  platforms: string[];
  licenseType: string;
  alternatives?: Array<{ alternativeToolContentId: string; order: number }> | null;
};

export type CreateToolFeatureRequestDto = {
  title: string;
  description: string | null;
  order: number | null;
};

export type ToolListItemDto = {
  id: string;
  contentId: string;
  toolName: string;
  toolCategory: string;
  pricingModel: string;
  licenseType: string;
  contentSlug: string;
  contentStatus: string;
  updatedAtUtc: string;
};

export type ToolAiSuggestionDto = {
  kind: string;
  title: string;
  body: string;
  bulletSuggestions: string[];
  requiresHumanApply: boolean;
};

export type RoadmapTopicDto = {
  id: string;
  title: string;
  description: string | null;
  order: number;
};

export type RoadmapResourceDto = {
  id: string;
  title: string;
  url: string;
  resourceType: string;
  order: number;
};

export type RoadmapStepDto = {
  id: string;
  title: string;
  description: string | null;
  order: number;
  estimatedHours: number;
  projectTitle: string | null;
  projectDescription: string | null;
  topics: RoadmapTopicDto[];
  resources: RoadmapResourceDto[];
};

export type RoadmapDetailDto = {
  id: string;
  contentId: string;
  level: string;
  estimatedDuration: string;
  goal: string;
  prerequisites: string | null;
  steps: RoadmapStepDto[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type UpdateRoadmapRequestDto = {
  level: string;
  estimatedDuration: string;
  goal: string;
  prerequisites: string | null;
};

export type UpsertRoadmapTopicItemDto = {
  id?: string | null;
  title: string;
  description: string | null;
  order: number;
};

export type UpsertRoadmapResourceItemDto = {
  id?: string | null;
  title: string;
  url: string;
  resourceType: string;
  order: number;
};

export type CreateRoadmapStepRequestDto = {
  title: string;
  description: string | null;
  order: number | null;
  estimatedHours: number;
  projectTitle: string | null;
  projectDescription: string | null;
  topics?: UpsertRoadmapTopicItemDto[] | null;
  resources?: UpsertRoadmapResourceItemDto[] | null;
};

export type UpdateRoadmapStepRequestDto = {
  title: string;
  description: string | null;
  order: number;
  estimatedHours: number;
  projectTitle: string | null;
  projectDescription: string | null;
  topics?: UpsertRoadmapTopicItemDto[] | null;
  resources?: UpsertRoadmapResourceItemDto[] | null;
};

export type RoadmapAiSuggestionDto = {
  kind: string;
  title: string;
  body: string;
  bulletSuggestions: string[];
  requiresHumanApply: boolean;
};

// SeoFindingSeverity (Domain enum) — POST /admin/content/{id}/seo-analysis.
export type SeoFindingSeverityDto = "Info" | "Warning" | "Error";

// SeoAuditReportDto — POST /admin/content/{id}/seo-analysis (no score, no AI).
export type SeoAuditFindingDto = {
  ruleId: string;
  category: "Metadata" | "ContentStructure" | "Images" | "Links" | "Technical";
  severity: SeoFindingSeverityDto;
  message: string;
  suggestion: string | null;
  field: string | null;
};

export type SeoAuditSummaryDto = {
  errorCount: number;
  warningCount: number;
  infoCount: number;
};

export type SeoAuditReportDto = {
  contentId: string;
  generatedAtUtc: string;
  summary: SeoAuditSummaryDto;
  findings: SeoAuditFindingDto[];
};

/** Query options for GET /admin/content (server-side pagination + filters). */
export type AdminContentListOptions = {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  type?: string;
};

/** AdminContentListItemDto — list projection (no body/SEO/views). */
export type AdminContentListItemDto = {
  id: string;
  title: string;
  slug: string;
  contentType: string;
  contentStatus: string;
  authorId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  publishedAtUtc: string | null;
};

/** PagedResult&lt;AdminContentListItemDto&gt; (camelCase JSON). */
export type AdminContentPagedResultDto = {
  items: AdminContentListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export function listPublishedContent(signal?: AbortSignal): Promise<ContentSummaryDto[]> {
  return apiRequest<ContentSummaryDto[]>({
    path: "/content",
    signal,
  });
}

/**
 * Admin CMS list — drafts + published, ownership-scoped, server-paginated.
 * Empty filters are omitted; query keys are appended in deterministic order.
 */
export function getAdminContentList(
  token: string,
  options: AdminContentListOptions = {},
  signal?: AbortSignal,
): Promise<AdminContentPagedResultDto> {
  const query: Record<string, string | number | undefined> = {};

  if (options.page != null) query.page = options.page;
  if (options.pageSize != null) query.pageSize = options.pageSize;
  if (options.search) query.search = options.search;
  if (options.status) query.status = options.status;
  if (options.type) query.type = options.type;

  return apiRequest<AdminContentPagedResultDto>({
    path: "/admin/content",
    token,
    query,
    signal,
  });
}

export function getContentBySlug(slug: string, signal?: AbortSignal): Promise<ContentDetailDto> {
  return apiRequest<ContentDetailDto>({
    path: `/content/${encodeURIComponent(slug)}`,
    signal,
  });
}

export function createContent(
  token: string,
  request: CreateContentRequestDto,
  signal?: AbortSignal,
): Promise<ContentDetailDto> {
  return apiRequest<ContentDetailDto>({
    method: "POST",
    path: "/content",
    token,
    body: request,
    signal,
  });
}

/** Admin Read Model — full detail including excerpt, cover, SEO and timestamps. */
export function getAdminContentById(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    path: `/admin/content/${encodeURIComponent(id)}`,
    token,
    signal,
  });
}

export function updateContent(
  token: string,
  id: string,
  request: UpdateContentRequestDto,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}`,
    token,
    body: request,
    signal,
  });
}

/** POST /admin/content/preview — compiles TipTap JSON without persisting. */
export function previewArticle(
  token: string,
  request: PreviewArticleRequestDto,
  signal?: AbortSignal,
): Promise<PreviewArticleDto> {
  return apiRequest<PreviewArticleDto>({
    method: "POST",
    path: "/admin/content/preview",
    token,
    body: request,
    signal,
  });
}

export function publishContent(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/publish`,
    token,
    signal,
  });
}

export type RejectContentRequestDto = {
  comment: string;
};

export type ContentWorkflowTransitionDto = {
  id: string;
  fromStatus: string;
  toStatus: string;
  actorUserId: string;
  comment: string | null;
  createdAtUtc: string;
};

export type WorkflowHistoryDto = {
  items: ContentWorkflowTransitionDto[];
};

/** POST /admin/content/{id}/submit-review — Draft → ReviewPending. */
export function submitContentForReview(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/submit-review`,
    token,
    signal,
  });
}

/** POST /admin/content/{id}/approve — ReviewPending → Approved (Admin). */
export function approveContent(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/approve`,
    token,
    signal,
  });
}

/** POST /admin/content/{id}/reject — ReviewPending → Draft (Admin). */
export function rejectContent(
  token: string,
  id: string,
  request: RejectContentRequestDto,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/reject`,
    token,
    body: request,
    signal,
  });
}

/** POST /admin/content/{id}/archive — Published → Archived (Admin). */
export function archiveContent(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/archive`,
    token,
    signal,
  });
}

/** GET /admin/content/{id}/workflow-history — immutable transition timeline. */
export function getContentWorkflowHistory(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<WorkflowHistoryDto> {
  return apiRequest<WorkflowHistoryDto>({
    path: `/admin/content/${encodeURIComponent(id)}/workflow-history`,
    token,
    signal,
  });
}

export function updateContentSeoMetadata(
  token: string,
  id: string,
  request: UpdateSeoMetadataRequestDto,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/seo`,
    token,
    body: request,
    signal,
  });
}

export function getArticleMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<ArticleMetadataDto | null> {
  return apiRequest<ArticleMetadataDto | null>({
    path: `/admin/content/${encodeURIComponent(id)}/article`,
    token,
    signal,
  });
}

export function updateArticleMetadata(
  token: string,
  id: string,
  request: UpdateArticleMetadataRequestDto,
  signal?: AbortSignal,
): Promise<ArticleMetadataDto> {
  return apiRequest<ArticleMetadataDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/article`,
    token,
    body: request,
    signal,
  });
}

export function getNewsMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<NewsMetadataDto | null> {
  return apiRequest<NewsMetadataDto | null>({
    path: `/admin/content/${encodeURIComponent(id)}/news`,
    token,
    signal,
  });
}

export function updateNewsMetadata(
  token: string,
  id: string,
  request: UpdateNewsMetadataRequestDto,
  signal?: AbortSignal,
): Promise<NewsMetadataDto> {
  return apiRequest<NewsMetadataDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/news`,
    token,
    body: request,
    signal,
  });
}

export function listAdminTools(token: string, signal?: AbortSignal): Promise<ToolListItemDto[]> {
  return apiRequest<ToolListItemDto[]>({
    path: "/admin/tools",
    token,
    signal,
  });
}

export function getToolMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<ToolDetailDto | null> {
  return apiRequest<ToolDetailDto | null>({
    path: `/admin/content/${encodeURIComponent(id)}/tool`,
    token,
    signal,
  });
}

export function updateToolMetadata(
  token: string,
  id: string,
  request: UpdateToolRequestDto,
  signal?: AbortSignal,
): Promise<ToolDetailDto> {
  return apiRequest<ToolDetailDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/tool`,
    token,
    body: request,
    signal,
  });
}

export function addToolFeature(
  token: string,
  id: string,
  request: CreateToolFeatureRequestDto,
  signal?: AbortSignal,
): Promise<ToolFeatureDto> {
  return apiRequest<ToolFeatureDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/tool/features`,
    token,
    body: request,
    signal,
  });
}

export function removeToolFeature(
  token: string,
  id: string,
  featureId: string,
  signal?: AbortSignal,
): Promise<void> {
  return apiRequest<void>({
    method: "DELETE",
    path: `/admin/content/${encodeURIComponent(id)}/tool/features/${encodeURIComponent(featureId)}`,
    token,
    signal,
  });
}

export function suggestToolSummary(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<ToolAiSuggestionDto> {
  return apiRequest<ToolAiSuggestionDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/tool/ai/summary`,
    token,
    signal,
  });
}

export function suggestToolFeatures(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<ToolAiSuggestionDto> {
  return apiRequest<ToolAiSuggestionDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/tool/ai/features`,
    token,
    signal,
  });
}

export function getRoadmapMetadata(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<RoadmapDetailDto | null> {
  return apiRequest<RoadmapDetailDto | null>({
    path: `/admin/content/${encodeURIComponent(id)}/roadmap`,
    token,
    signal,
  });
}

export function updateRoadmapMetadata(
  token: string,
  id: string,
  request: UpdateRoadmapRequestDto,
  signal?: AbortSignal,
): Promise<RoadmapDetailDto> {
  return apiRequest<RoadmapDetailDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap`,
    token,
    body: request,
    signal,
  });
}

export function addRoadmapStep(
  token: string,
  id: string,
  request: CreateRoadmapStepRequestDto,
  signal?: AbortSignal,
): Promise<RoadmapStepDto> {
  return apiRequest<RoadmapStepDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap/steps`,
    token,
    body: request,
    signal,
  });
}

export function updateRoadmapStep(
  token: string,
  id: string,
  stepId: string,
  request: UpdateRoadmapStepRequestDto,
  signal?: AbortSignal,
): Promise<RoadmapStepDto> {
  return apiRequest<RoadmapStepDto>({
    method: "PUT",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap/steps/${encodeURIComponent(stepId)}`,
    token,
    body: request,
    signal,
  });
}

export function removeRoadmapStep(
  token: string,
  id: string,
  stepId: string,
  signal?: AbortSignal,
): Promise<void> {
  return apiRequest<void>({
    method: "DELETE",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap/steps/${encodeURIComponent(stepId)}`,
    token,
    signal,
  });
}

export function reorderRoadmapSteps(
  token: string,
  id: string,
  stepIds: string[],
  signal?: AbortSignal,
): Promise<void> {
  return apiRequest<void>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap/steps/reorder`,
    token,
    body: { stepIds },
    signal,
  });
}

export function suggestRoadmapOutline(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<RoadmapAiSuggestionDto> {
  return apiRequest<RoadmapAiSuggestionDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/roadmap/ai/outline`,
    token,
    signal,
  });
}

/**
 * Runs the SEO Analyzer Engine against the SAVED server version of the content
 * (`POST /admin/content/{id}/seo-analysis`). Returns factual findings and
 * statistics only — no score, percentage, ranking prediction or AI involved.
 */
export function analyzeContentSeo(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<SeoAuditReportDto> {
  return apiRequest<SeoAuditReportDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/seo-analysis`,
    token,
    signal,
  });
}

/** ContentAiResultDto — on-demand AI suggestion (never auto-saved). */
export type ContentAiTaskTypeDto =
  | "ContentAnalysis"
  | "TitleSuggestion"
  | "MetaDescription"
  | "OutlineGeneration"
  | "FaqGeneration";

export type ContentAiResultDto = {
  taskType: ContentAiTaskTypeDto | string;
  generatedText: string;
  createdAtUtc: string;
  model: string;
  provider: string;
};

export type ContentAiAction =
  | "analyze"
  | "title-suggestions"
  | "meta-description"
  | "outline"
  | "faq";

/**
 * POST /admin/content/{id}/ai/{action} — human-triggered suggestion only.
 * Does not persist generated text to the content entity.
 */
export function runContentAiAction(
  token: string,
  id: string,
  action: ContentAiAction,
  signal?: AbortSignal,
): Promise<ContentAiResultDto> {
  return apiRequest<ContentAiResultDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/ai/${action}`,
    token,
    signal,
  });
}

/** ContentRevisionListItemDto — revision history row (newest first). */
export type ContentRevisionListItemDto = {
  versionNumber: number;
  createdByUserId: string;
  createdAtUtc: string;
  changeReason: string | null;
};

/** ContentRevisionSnapshotDto — frozen content + SEO at a revision point. */
export type ContentRevisionSnapshotDto = {
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  coverImage: string | null;
  contentType: string;
  seoMetadata: SeoMetadataDto;
};

/** ContentRevisionDetailDto — full revision including snapshot. */
export type ContentRevisionDetailDto = {
  contentId: string;
  versionNumber: number;
  snapshot: ContentRevisionSnapshotDto;
  changeReason: string | null;
  createdByUserId: string;
  createdAtUtc: string;
};

export type RestoreContentRevisionRequestDto = {
  changeReason?: string | null;
};

/** PagedResult&lt;ContentRevisionListItemDto&gt; (camelCase JSON). */
export type ContentRevisionPagedResultDto = {
  items: ContentRevisionListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type ContentRevisionListOptions = {
  page?: number;
  pageSize?: number;
};

/** GET /admin/content/{id}/revisions — paginated revision history (newest first). */
export function getContentRevisions(
  token: string,
  id: string,
  options: ContentRevisionListOptions = {},
  signal?: AbortSignal,
): Promise<ContentRevisionPagedResultDto> {
  const query: Record<string, string | number | undefined> = {};
  if (options.page != null) query.page = options.page;
  if (options.pageSize != null) query.pageSize = options.pageSize;

  return apiRequest<ContentRevisionPagedResultDto>({
    path: `/admin/content/${encodeURIComponent(id)}/revisions`,
    token,
    query,
    signal,
  });
}

/** GET /admin/content/{id}/revisions/{version} — revision detail + snapshot. */
export function getContentRevision(
  token: string,
  id: string,
  version: number,
  signal?: AbortSignal,
): Promise<ContentRevisionDetailDto> {
  return apiRequest<ContentRevisionDetailDto>({
    path: `/admin/content/${encodeURIComponent(id)}/revisions/${version}`,
    token,
    signal,
  });
}

/** POST /admin/content/{id}/revisions/{version}/restore — restore snapshot to live content. */
export function restoreContentRevision(
  token: string,
  id: string,
  version: number,
  request?: RestoreContentRevisionRequestDto,
  signal?: AbortSignal,
): Promise<AdminContentDetailDto> {
  return apiRequest<AdminContentDetailDto>({
    method: "POST",
    path: `/admin/content/${encodeURIComponent(id)}/revisions/${version}/restore`,
    token,
    body: request ?? {},
    signal,
  });
}
