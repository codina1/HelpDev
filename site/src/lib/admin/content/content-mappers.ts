import type { AdminDashboardDto } from "@/lib/admin/dashboard/dashboard-types";
import {
  CONTENT_LIMITS,
  CONTENT_STATUSES,
  CONTENT_TYPES,
  DIFFICULTY_LEVELS,
  EMPTY_ARTICLE_FORM,
  EMPTY_NEWS_FORM,
  NEWS_PRIORITIES,
  SEO_LIMITS,
  type AdminContentDetail,
  type AdminContentDetailRawDto,
  type AdminContentListItem,
  type AdminContentListItemRawDto,
  type AdminContentPagedResult,
  type AdminContentPagedResultRawDto,
  type ArticleFormErrors,
  type ArticleFormValues,
  type ArticleMetadataRawDto,
  type ContentDetail,
  type ContentDetailRawDto,
  type ContentFilterValues,
  type ContentFormErrors,
  type ContentFormValues,
  type ContentListItem,
  type ContentListRawDto,
  type ContentStats,
  type ContentStatusValue,
  type ContentTypeValue,
  type DifficultyLevelValue,
  type NewsFormErrors,
  type NewsFormValues,
  type NewsMetadataRawDto,
  type NewsPriorityValue,
  type SeoAnalysisFinding,
  type SeoAnalysisReport,
  type SeoAnalysisSection,
  type SeoAnalysisSectionKey,
  type SeoAuditFindingRawDto,
  type SeoAuditReportRawDto,
  type SeoFindingSeverityValue,
  type SeoPlatformCategoryValue,
  SEO_PLATFORM_CATEGORIES,
  type SeoFormErrors,
  type SeoFormValues,
  type UpdateArticleMetadataPayload,
  type UpdateNewsMetadataPayload,
  type UpdateSeoMetadataPayload,
} from "@/lib/admin/content/content-types";
import { labelForWorkflowStatus } from "@/lib/admin/content/workflow/workflow-labels";

const CONTENT_TYPE_LABELS: Record<string, string> = {
  News: "خبر",
  Article: "مقاله",
  RoadmapStep: "گام نقشه راه (قدیمی)",
  Roadmap: "نقشه راه",
  Tool: "ابزار",
  Prompt: "پرامپت",
  Course: "دوره",
};

const CONTENT_STATUS_LABELS: Record<ContentStatusValue, string> = {
  Draft: labelForWorkflowStatus("Draft"),
  ReviewPending: labelForWorkflowStatus("ReviewPending"),
  Approved: labelForWorkflowStatus("Approved"),
  Published: labelForWorkflowStatus("Published"),
  Archived: labelForWorkflowStatus("Archived"),
};

export function labelForContentType(type: string): string {
  return CONTENT_TYPE_LABELS[type] ?? type;
}

export function labelForContentStatus(status: string): string {
  return CONTENT_STATUS_LABELS[status as ContentStatusValue] ?? status;
}

export function isKnownContentType(type: string): type is ContentTypeValue {
  return (CONTENT_TYPES as readonly string[]).includes(type);
}

export function normalizeContentStatus(status: string): ContentStatusValue {
  const normalized = status.trim();
  for (const value of CONTENT_STATUSES) {
    if (value.toLowerCase() === normalized.toLowerCase()) return value;
  }
  return "Draft";
}

/**
 * Produces a backend-valid slug: `^[a-z0-9]+(?:-[a-z0-9]+)*$`.
 * Non-latin input (e.g. Persian titles) yields an empty string, prompting the
 * author to enter a slug manually.
 */
export function slugify(input: string): string {
  return input
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 300);
}

export const SLUG_PATTERN = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;

export function isValidSlug(slug: string): boolean {
  return slug.length >= 2 && slug.length <= 300 && SLUG_PATTERN.test(slug);
}

/** Client-side form validation mirroring backend constraints (server is authoritative). */
export function validateContentForm(values: ContentFormValues): ContentFormErrors {
  const errors: ContentFormErrors = {};

  if (!values.title.trim()) {
    errors.title = "عنوان الزامی است.";
  } else if (values.title.trim().length > 200) {
    errors.title = "عنوان نباید بیش از ۲۰۰ نویسه باشد.";
  }

  if (!values.slug.trim()) {
    errors.slug = "اسلاگ الزامی است.";
  } else if (!isValidSlug(values.slug.trim())) {
    errors.slug = "اسلاگ فقط می‌تواند شامل حروف کوچک انگلیسی، اعداد و خط تیره باشد.";
  }

  if (!isKnownContentType(values.type)) {
    errors.type = "نوع محتوا معتبر نیست.";
  }

  if (!values.body.trim()) {
    errors.body = "متن محتوا الزامی است.";
  }

  if (values.excerpt.trim().length > CONTENT_LIMITS.excerpt) {
    errors.excerpt = `خلاصه نباید بیش از ${CONTENT_LIMITS.excerpt} نویسه باشد.`;
  }

  if (values.coverImage.trim()) {
    if (values.coverImage.trim().length > CONTENT_LIMITS.coverImage) {
      errors.coverImage = "آدرس تصویر کاور بیش از حد مجاز است.";
    } else if (!isAbsoluteUrl(values.coverImage.trim())) {
      errors.coverImage = "آدرس تصویر کاور باید یک نشانی معتبر http(s) باشد.";
    }
  }

  return errors;
}

export function hasFormErrors(errors: Record<string, unknown>): boolean {
  return Object.keys(errors).length > 0;
}

/** Validates an absolute http(s) URL (mirrors backend `Uri.TryCreate` + scheme check). */
export function isAbsoluteUrl(value: string): boolean {
  try {
    const url = new URL(value);
    return url.protocol === "http:" || url.protocol === "https:";
  } catch {
    return false;
  }
}

/** Client-side SEO validation mirroring `SeoMetadata` (server remains authoritative). */
export function validateSeoForm(values: SeoFormValues): SeoFormErrors {
  const errors: SeoFormErrors = {};

  if (values.seoTitle.trim().length > SEO_LIMITS.seoTitle) {
    errors.seoTitle = `عنوان سئو نباید بیش از ${SEO_LIMITS.seoTitle} نویسه باشد.`;
  }
  if (values.seoDescription.trim().length > SEO_LIMITS.seoDescription) {
    errors.seoDescription = `توضیحات سئو نباید بیش از ${SEO_LIMITS.seoDescription} نویسه باشد.`;
  }
  if (values.canonicalUrl.trim()) {
    if (values.canonicalUrl.trim().length > SEO_LIMITS.canonicalUrl) {
      errors.canonicalUrl = "آدرس کاننیکال بیش از حد مجاز است.";
    } else if (!isAbsoluteUrl(values.canonicalUrl.trim())) {
      errors.canonicalUrl = "آدرس کاننیکال باید یک نشانی معتبر http(s) باشد.";
    }
  }
  if (values.ogImage.trim()) {
    if (values.ogImage.trim().length > SEO_LIMITS.ogImage) {
      errors.ogImage = "آدرس تصویر OG بیش از حد مجاز است.";
    } else if (!isAbsoluteUrl(values.ogImage.trim())) {
      errors.ogImage = "آدرس تصویر OG باید یک نشانی معتبر http(s) باشد.";
    }
  }
  if (values.focusKeyword.trim().length > SEO_LIMITS.focusKeyword) {
    errors.focusKeyword = `کلمه کلیدی نباید بیش از ${SEO_LIMITS.focusKeyword} نویسه باشد.`;
  }

  return errors;
}

/** Trims values and maps blanks to null for the SEO update request. */
export function buildSeoPayload(values: SeoFormValues): UpdateSeoMetadataPayload {
  const nullIfBlank = (value: string): string | null => {
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  };
  return {
    seoTitle: nullIfBlank(values.seoTitle),
    seoDescription: nullIfBlank(values.seoDescription),
    canonicalUrl: nullIfBlank(values.canonicalUrl),
    ogImage: nullIfBlank(values.ogImage),
    focusKeyword: nullIfBlank(values.focusKeyword),
  };
}

/** Maps an admin detail DTO (from a mutation response) into SEO form values. */
export function mapSeoForm(dto: AdminContentDetailRawDto): SeoFormValues {
  return {
    seoTitle: dto.seo?.seoTitle ?? "",
    seoDescription: dto.seo?.seoDescription ?? "",
    canonicalUrl: dto.seo?.canonicalUrl ?? "",
    ogImage: dto.seo?.ogImage ?? "",
    focusKeyword: dto.seo?.focusKeyword ?? "",
  };
}

export function mapArticleForm(dto: ArticleMetadataRawDto | null | undefined): ArticleFormValues {
  if (!dto) return { ...EMPTY_ARTICLE_FORM };
  const difficulty = DIFFICULTY_LEVELS.includes(dto.difficultyLevel as DifficultyLevelValue)
    ? (dto.difficultyLevel as DifficultyLevelValue)
    : "Beginner";
  return {
    categoryId: dto.categoryId ?? "",
    difficultyLevel: difficulty,
    readingTimeMinutes: String(dto.readingTimeMinutes),
    isFeatured: dto.isFeatured,
    allowComments: dto.allowComments,
    tableOfContentsEnabled: dto.tableOfContentsEnabled,
  };
}

export function validateArticleForm(values: ArticleFormValues): ArticleFormErrors {
  const errors: ArticleFormErrors = {};
  const minutes = Number(values.readingTimeMinutes);
  if (!Number.isFinite(minutes) || minutes <= 0) {
    errors.readingTimeMinutes = "زمان مطالعه باید بزرگ‌تر از صفر باشد.";
  }
  if (values.categoryId.trim()) {
    const guid =
      /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i;
    if (!guid.test(values.categoryId.trim())) {
      errors.categoryId = "شناسه دسته معتبر نیست.";
    }
  }
  return errors;
}

export function buildArticlePayload(values: ArticleFormValues): UpdateArticleMetadataPayload {
  const category = values.categoryId.trim();
  return {
    categoryId: category.length > 0 ? category : null,
    difficultyLevel: values.difficultyLevel,
    readingTimeMinutes: Number(values.readingTimeMinutes),
    isFeatured: values.isFeatured,
    allowComments: values.allowComments,
    tableOfContentsEnabled: values.tableOfContentsEnabled,
  };
}

export function mapNewsForm(dto: NewsMetadataRawDto | null | undefined): NewsFormValues {
  if (!dto) {
    return {
      ...EMPTY_NEWS_FORM,
      newsDateUtc: new Date().toISOString().slice(0, 16),
    };
  }
  const priority = NEWS_PRIORITIES.includes(dto.priority as NewsPriorityValue)
    ? (dto.priority as NewsPriorityValue)
    : "Normal";
  return {
    sourceName: dto.sourceName,
    sourceUrl: dto.sourceUrl ?? "",
    newsDateUtc: dto.newsDateUtc.slice(0, 16),
    priority,
    externalReference: dto.externalReference ?? "",
  };
}

export function validateNewsForm(values: NewsFormValues): NewsFormErrors {
  const errors: NewsFormErrors = {};
  if (!values.sourceName.trim()) {
    errors.sourceName = "نام منبع خبر الزامی است.";
  }
  if (values.sourceUrl.trim()) {
    if (!isAbsoluteUrl(values.sourceUrl.trim())) {
      errors.sourceUrl = "آدرس منبع باید یک نشانی معتبر http(s) باشد.";
    }
  }
  if (!values.newsDateUtc.trim()) {
    errors.newsDateUtc = "تاریخ خبر الزامی است.";
  }
  return errors;
}

export function buildNewsPayload(values: NewsFormValues): UpdateNewsMetadataPayload {
  const nullIfBlank = (value: string): string | null => {
    const trimmed = value.trim();
    return trimmed.length > 0 ? trimmed : null;
  };
  const local = values.newsDateUtc.trim();
  const newsDateUtc = local ? new Date(local).toISOString() : new Date().toISOString();
  return {
    sourceName: values.sourceName.trim(),
    sourceUrl: nullIfBlank(values.sourceUrl),
    newsDateUtc,
    priority: values.priority,
    externalReference: nullIfBlank(values.externalReference),
  };
}

export function mapContentListItem(dto: ContentListRawDto): ContentListItem {
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug,
    type: dto.type,
    typeLabel: labelForContentType(dto.type),
    authorId: dto.authorId,
    views: dto.views,
    saves: dto.saves,
    createdAt: dto.createdAt,
    // GET /content returns published content only.
    status: "Published",
    statusLabel: CONTENT_STATUS_LABELS.Published,
  };
}

export function mapContentList(items: ContentListRawDto[]): ContentListItem[] {
  return items.map(mapContentListItem);
}

export function mapAdminContentListItem(dto: AdminContentListItemRawDto): AdminContentListItem {
  const status = normalizeContentStatus(dto.contentStatus);
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug,
    type: dto.contentType,
    typeLabel: labelForContentType(dto.contentType),
    status,
    statusLabel: labelForContentStatus(status),
    authorId: dto.authorId,
    createdAtUtc: dto.createdAtUtc,
    updatedAtUtc: dto.updatedAtUtc,
    publishedAtUtc: dto.publishedAtUtc,
  };
}

export function mapAdminContentPagedResult(
  dto: AdminContentPagedResultRawDto,
): AdminContentPagedResult {
  const pageSize = dto.pageSize > 0 ? dto.pageSize : 1;
  const totalPages =
    typeof dto.totalPages === "number" && dto.totalPages >= 0
      ? dto.totalPages
      : Math.ceil((dto.totalCount ?? 0) / pageSize);

  return {
    items: (dto.items ?? []).map(mapAdminContentListItem),
    page: dto.page ?? 1,
    pageSize: dto.pageSize,
    totalCount: dto.totalCount ?? 0,
    totalPages,
  };
}

export function mapContentDetail(dto: ContentDetailRawDto): ContentDetail {
  const status = normalizeContentStatus(dto.status);
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug,
    body: dto.body,
    type: dto.type,
    typeLabel: labelForContentType(dto.type),
    authorId: dto.authorId,
    status,
    statusLabel: labelForContentStatus(status),
    views: dto.views,
    saves: dto.saves,
    createdAt: dto.createdAt,
  };
}

/** Maps the Admin Read Model DTO into the Studio/details view model. */
export function mapAdminContentDetail(dto: AdminContentDetailRawDto): AdminContentDetail {
  const status = normalizeContentStatus(dto.contentStatus);
  return {
    id: dto.id,
    title: dto.title,
    slug: dto.slug,
    body: dto.body,
    excerpt: dto.excerpt ?? "",
    coverImage: dto.coverImage ?? "",
    type: dto.contentType,
    typeLabel: labelForContentType(dto.contentType),
    authorId: dto.authorId,
    status,
    statusLabel: labelForContentStatus(status),
    views: dto.views,
    saves: dto.saves,
    createdAtUtc: dto.createdAtUtc,
    updatedAtUtc: dto.updatedAtUtc,
    publishedAtUtc: dto.publishedAtUtc,
    seo: mapSeoForm(dto),
  };
}

/* --------------------------- SEO Analysis mapping ---------------------------
 * POST /admin/content/{id}/seo-analysis — factual findings/statistics only.
 * Labels are presentation-only; the underlying counts/booleans are untouched.
 * --------------------------------------------------------------------------- */

const SEO_PLATFORM_CATEGORY_LABELS: Record<SeoPlatformCategoryValue, string> = {
  Metadata: "متادیتا",
  ContentStructure: "ساختار محتوا",
  Images: "تصاویر",
  Links: "پیوندها",
  Technical: "فنی",
};

const SEO_FINDING_SEVERITY_LABELS: Record<SeoFindingSeverityValue, string> = {
  Info: "اطلاعاتی",
  Warning: "هشدار",
  Error: "خطا",
};

const SEO_ANALYSIS_SECTION_LABELS = SEO_PLATFORM_CATEGORY_LABELS;

export function labelForSeoFindingCategory(category: string): string {
  return SEO_PLATFORM_CATEGORY_LABELS[category as SeoPlatformCategoryValue] ?? category;
}

export function labelForSeoFindingSeverity(severity: string): string {
  return SEO_FINDING_SEVERITY_LABELS[severity as SeoFindingSeverityValue] ?? severity;
}

function isPassedFinding(dto: SeoAuditFindingRawDto): boolean {
  return dto.severity === "Info" && !dto.ruleId.includes("missing");
}

export function mapSeoAnalysisFinding(dto: SeoAuditFindingRawDto): SeoAnalysisFinding {
  return {
    ...dto,
    categoryLabel: labelForSeoFindingCategory(dto.category),
    severityLabel: labelForSeoFindingSeverity(dto.severity),
    passed: isPassedFinding(dto),
  };
}

/** Maps the raw SEO audit report into the Studio view model. */
export function mapSeoAnalysisReport(dto: SeoAuditReportRawDto): SeoAnalysisReport {
  return {
    contentId: dto.contentId,
    analyzedAtUtc: dto.generatedAtUtc,
    summary: dto.summary,
    findings: (dto.findings ?? []).map(mapSeoAnalysisFinding),
  };
}

/** Groups findings into platform categories (only non-empty sections). */
export function groupSeoAnalysisFindings(findings: SeoAnalysisFinding[]): SeoAnalysisSection[] {
  const buckets = new Map<SeoAnalysisSectionKey, SeoAnalysisFinding[]>();
  for (const finding of findings) {
    const section = finding.category;
    const list = buckets.get(section);
    if (list) {
      list.push(finding);
    } else {
      buckets.set(section, [finding]);
    }
  }

  const sections: SeoAnalysisSection[] = [];
  for (const key of SEO_PLATFORM_CATEGORIES) {
    const list = buckets.get(key);
    if (list && list.length > 0) {
      sections.push({ key, label: SEO_ANALYSIS_SECTION_LABELS[key], findings: list });
    }
  }
  return sections;
}

const DATE_TIME_FORMAT = new Intl.DateTimeFormat("fa-IR", {
  year: "numeric",
  month: "long",
  day: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

/** Formats the analysis timestamp (date + time) for the "saved-version" label. */
export function formatDateTimeFa(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return DATE_TIME_FORMAT.format(date);
}

export function mapContentStats(dto: AdminDashboardDto): ContentStats {
  return {
    total: dto.content.totalContent,
    published: dto.content.publishedContent,
    draft: dto.content.draftContent,
    publicationsToday: dto.content.publicationsToday,
  };
}

/** Pure client-side filtering retained for tests/legacy; Admin workspace uses server filters. */
export function filterContent(
  items: ContentListItem[],
  filters: ContentFilterValues,
): ContentListItem[] {
  const query = filters.search.trim().toLowerCase();
  return items.filter((item) => {
    if (filters.type !== "all" && item.type !== filters.type) return false;
    if (filters.status !== "all" && item.status !== filters.status) return false;
    if (query) {
      const haystack = `${item.title} ${item.slug}`.toLowerCase();
      if (!haystack.includes(query)) return false;
    }
    return true;
  });
}

const DATE_FORMAT = new Intl.DateTimeFormat("fa-IR", {
  year: "numeric",
  month: "long",
  day: "numeric",
});

export function formatDateFa(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "";
  return DATE_FORMAT.format(date);
}

/** Short, safe representation of an author identifier (no name endpoint exists). */
export function shortAuthorId(authorId: string): string {
  return authorId.length > 8 ? `${authorId.slice(0, 8)}…` : authorId;
}
