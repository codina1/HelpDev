/**
 * Content CMS data contracts.
 *
 * These mirror the REAL backend Content API (inspected from
 * `HelpDev.Modules.Content`), never assumed shapes. The Admin workspace uses:
 * - `GET /admin/content` (paged list; drafts + published; ownership-scoped)
 * - `GET /admin/content/{id}` (full Admin Read Model for Studio/details)
 * - update / publish / SEO mutations
 * The public `GET /content` list is NOT used by the Admin CMS.
 * See docs/admin/admin-content-cms.md.
 */

/** Backend ContentSearchFilter defaults / clamps (Application/Contents). */
export const ADMIN_CONTENT_PAGE_SIZE_DEFAULT = 20;
export const ADMIN_CONTENT_PAGE_SIZE_MAX = 100;
export const ADMIN_CONTENT_PAGE_SIZES = [10, 20, 50, 100] as const;
export type AdminContentPageSize = (typeof ADMIN_CONTENT_PAGE_SIZES)[number];

// Backend enum names (Domain/Enums/ContentType.cs).
export const CONTENT_TYPES = [
  "News",
  "Article",
  "RoadmapStep",
  "Tool",
  "Prompt",
  "Course",
  "Roadmap",
] as const;
export type ContentTypeValue = (typeof CONTENT_TYPES)[number];

// Backend enum names (Domain/Enums/ContentStatus.cs).
export const CONTENT_STATUSES = [
  "Draft",
  "ReviewPending",
  "Approved",
  "Published",
  "Archived",
] as const;
export type ContentStatusValue = (typeof CONTENT_STATUSES)[number];

/* ------------------------------- Raw API DTOs ------------------------------ */

// GET /content — ContentListItemDto (published items only; no Status/Body).
export type ContentListRawDto = {
  id: string;
  title: string;
  slug: string;
  type: string;
  authorId: string;
  views: number;
  saves: number;
  createdAt: string;
};

// GET /content/{slug} — ContentDetailDto (published only).
export type ContentDetailRawDto = {
  id: string;
  title: string;
  slug: string;
  body: string;
  type: string;
  authorId: string;
  status: string;
  views: number;
  saves: number;
  createdAt: string;
};

// POST /content — CreateContentRequest.
export type CreateContentPayload = {
  title: string;
  slug: string;
  body: string;
  type: ContentTypeValue;
  status: ContentStatusValue;
};

/* ------------------------ Admin list contracts ----------------------------- */

/** Raw AdminContentListItemDto (GET /admin/content items[]). */
export type AdminContentListItemRawDto = {
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

/** Raw PagedResult&lt;AdminContentListItemDto&gt;. */
export type AdminContentPagedResultRawDto = {
  items: AdminContentListItemRawDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

/** Normalized workspace query (URL-driven). */
export type AdminContentListQuery = {
  page: number;
  pageSize: AdminContentPageSize;
  search: string;
  status: ContentStatusValue | "all";
  type: ContentTypeValue | "all";
};

export const DEFAULT_ADMIN_CONTENT_LIST_QUERY: AdminContentListQuery = {
  page: 1,
  pageSize: ADMIN_CONTENT_PAGE_SIZE_DEFAULT,
  search: "",
  status: "all",
  type: "all",
};

/** View model for a single admin list row. */
export type AdminContentListItem = {
  id: string;
  title: string;
  slug: string;
  type: string;
  typeLabel: string;
  status: ContentStatusValue;
  statusLabel: string;
  authorId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
  publishedAtUtc: string | null;
};

export type AdminContentPagedResult = {
  items: AdminContentListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

/* --------------------- Admin detail + mutation contracts -------------------- */

// SeoMetadataDto (nested in AdminContentDetailDto). All fields nullable.
export type SeoMetadataRawDto = {
  seoTitle: string | null;
  seoDescription: string | null;
  canonicalUrl: string | null;
  ogImage: string | null;
  focusKeyword: string | null;
};

// AdminContentDetailDto — returned by GET /admin/content/{id} and by
// PUT update / POST publish / PUT seo mutations.
export type AdminContentDetailRawDto = {
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
  seo: SeoMetadataRawDto;
};

// PUT /admin/content/{id} — UpdateContentRequest.
export type UpdateContentPayload = {
  title: string;
  slug: string;
  type: ContentTypeValue;
  body: string;
  excerpt: string | null;
  coverImage: string | null;
};

// PUT /admin/content/{id}/seo — UpdateSeoMetadataRequest (nullable fields).
export type UpdateSeoMetadataPayload = {
  seoTitle: string | null;
  seoDescription: string | null;
  canonicalUrl: string | null;
  ogImage: string | null;
  focusKeyword: string | null;
};

export const DIFFICULTY_LEVELS = ["Beginner", "Intermediate", "Advanced"] as const;
export type DifficultyLevelValue = (typeof DIFFICULTY_LEVELS)[number];

export const NEWS_PRIORITIES = ["Normal", "Featured", "Breaking"] as const;
export type NewsPriorityValue = (typeof NEWS_PRIORITIES)[number];

export type ArticleMetadataRawDto = {
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

export type UpdateArticleMetadataPayload = {
  categoryId: string | null;
  difficultyLevel: string;
  readingTimeMinutes: number;
  isFeatured: boolean;
  allowComments: boolean;
  tableOfContentsEnabled: boolean;
};

export type ArticleFormValues = {
  categoryId: string;
  difficultyLevel: DifficultyLevelValue;
  readingTimeMinutes: string;
  isFeatured: boolean;
  allowComments: boolean;
  tableOfContentsEnabled: boolean;
};

export type ArticleFormErrors = Partial<Record<keyof ArticleFormValues, string>>;

export const EMPTY_ARTICLE_FORM: ArticleFormValues = {
  categoryId: "",
  difficultyLevel: "Beginner",
  readingTimeMinutes: "5",
  isFeatured: false,
  allowComments: true,
  tableOfContentsEnabled: true,
};

export type NewsMetadataRawDto = {
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

export type UpdateNewsMetadataPayload = {
  sourceName: string;
  sourceUrl: string | null;
  newsDateUtc: string;
  priority: string;
  externalReference: string | null;
};

export type NewsFormValues = {
  sourceName: string;
  sourceUrl: string;
  newsDateUtc: string;
  priority: NewsPriorityValue;
  externalReference: string;
};

export type NewsFormErrors = Partial<Record<keyof NewsFormValues, string>>;

export const EMPTY_NEWS_FORM: NewsFormValues = {
  sourceName: "",
  sourceUrl: "",
  newsDateUtc: new Date().toISOString().slice(0, 16),
  priority: "Normal",
  externalReference: "",
};

/* ------------------------ SEO Analysis contracts ---------------------------
 * POST /admin/content/{id}/seo-analysis — analyzes the SAVED server version
 * of the content only. Returns SeoAuditReportDto (findings only — no score).
 * --------------------------------------------------------------------------- */

export const SEO_PLATFORM_CATEGORIES = [
  "Metadata",
  "ContentStructure",
  "Images",
  "Links",
  "Technical",
] as const;
export type SeoPlatformCategoryValue = (typeof SEO_PLATFORM_CATEGORIES)[number];

// Backend enum names (SeoFindingSeverity).
export const SEO_FINDING_SEVERITIES = ["Info", "Warning", "Error"] as const;
export type SeoFindingSeverityValue = (typeof SEO_FINDING_SEVERITIES)[number];

export type SeoAuditFindingRawDto = {
  ruleId: string;
  category: SeoPlatformCategoryValue;
  severity: SeoFindingSeverityValue;
  message: string;
  suggestion: string | null;
  field: string | null;
};

export type SeoAuditSummaryRawDto = {
  errorCount: number;
  warningCount: number;
  infoCount: number;
};

export type SeoAuditReportRawDto = {
  contentId: string;
  generatedAtUtc: string;
  summary: SeoAuditSummaryRawDto;
  findings: SeoAuditFindingRawDto[];
};

/** View model for a single audit finding — adds Persian labels for display. */
export type SeoAnalysisFinding = SeoAuditFindingRawDto & {
  categoryLabel: string;
  severityLabel: string;
  /** True when the finding is informational only (not an open issue). */
  passed: boolean;
};

export const SEO_ANALYSIS_SECTIONS = SEO_PLATFORM_CATEGORIES;
export type SeoAnalysisSectionKey = SeoPlatformCategoryValue;

export type SeoAnalysisSection = {
  key: SeoAnalysisSectionKey;
  label: string;
  findings: SeoAnalysisFinding[];
};

/** View model for the SEO audit report returned by the analyzer API. */
export type SeoAnalysisReport = {
  contentId: string;
  analyzedAtUtc: string;
  summary: SeoAuditSummaryRawDto;
  findings: SeoAnalysisFinding[];
};

/** Explicit-action analysis lifecycle (never auto-run on keystroke). */
export const SEO_ANALYSIS_STATUSES = ["idle", "analyzing", "success", "error", "stale"] as const;
export type SeoAnalysisStatus = (typeof SEO_ANALYSIS_STATUSES)[number];

/** Controlled AI assistant tasks (v1 — no free-form prompts). */
export const CONTENT_AI_TASK_TYPES = [
  "ContentAnalysis",
  "TitleSuggestion",
  "MetaDescription",
  "OutlineGeneration",
  "FaqGeneration",
] as const;
export type ContentAiTaskTypeValue = (typeof CONTENT_AI_TASK_TYPES)[number];

export const CONTENT_AI_STATUSES = ["idle", "loading", "success", "error"] as const;
export type ContentAiStatus = (typeof CONTENT_AI_STATUSES)[number];

export type ContentAiResult = {
  taskType: string;
  generatedText: string;
  createdAtUtc: string;
  model: string;
  provider: string;
};

/* ------------------------------- SEO view model ---------------------------- */

// Backend max lengths (Domain/ValueObjects/SeoMetadata.cs).
export const SEO_LIMITS = {
  seoTitle: 70,
  seoDescription: 160,
  canonicalUrl: 2048,
  ogImage: 2048,
  focusKeyword: 100,
} as const;

// Backend content metadata limits (Domain/Entities/Content.cs).
export const CONTENT_LIMITS = {
  excerpt: 500,
  coverImage: 2048,
} as const;

export type SeoFormValues = {
  seoTitle: string;
  seoDescription: string;
  canonicalUrl: string;
  ogImage: string;
  focusKeyword: string;
};

export type SeoFormErrors = Partial<Record<keyof SeoFormValues, string>>;

export const EMPTY_SEO_FORM: SeoFormValues = {
  seoTitle: "",
  seoDescription: "",
  canonicalUrl: "",
  ogImage: "",
  focusKeyword: "",
};

/* ------------------------------- View models ------------------------------- */

export type ContentListItem = {
  id: string;
  title: string;
  slug: string;
  type: string;
  typeLabel: string;
  authorId: string;
  views: number;
  saves: number;
  createdAt: string;
  // GET /content returns published items only.
  status: ContentStatusValue;
  statusLabel: string;
};

export type ContentDetail = {
  id: string;
  title: string;
  slug: string;
  body: string;
  coverImage?: string;
  type: string;
  typeLabel: string;
  authorId: string;
  status: ContentStatusValue;
  statusLabel: string;
  views: number;
  saves: number;
  createdAt: string;
};

/** Full Admin Read Model used by the Content Studio and details workspace. */
export type AdminContentDetail = {
  id: string;
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  coverImage: string;
  type: string;
  typeLabel: string;
  authorId: string;
  status: ContentStatusValue;
  statusLabel: string;
  views: number;
  saves: number;
  createdAtUtc: string;
  updatedAtUtc: string;
  publishedAtUtc: string | null;
  seo: SeoFormValues;
};

export type ContentStats = {
  total: number;
  published: number;
  draft: number;
  publicationsToday: number | null;
};

/* --------------------------------- Forms ----------------------------------- */

export type ContentFormValues = {
  title: string;
  slug: string;
  type: ContentTypeValue;
  body: string;
  status: ContentStatusValue;
  excerpt: string;
  coverImage: string;
};

export type ContentFormErrors = Partial<Record<keyof ContentFormValues, string>>;

export type ContentFilterValues = {
  search: string;
  type: ContentTypeValue | "all";
  status: ContentStatusValue | "all";
};

export const EMPTY_CONTENT_FILTERS: ContentFilterValues = {
  search: "",
  type: "all",
  status: "all",
};
