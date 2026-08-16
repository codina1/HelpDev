"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import {
  ADMIN_ROUTES,
  adminContentArticleRoute,
  adminContentItemRoute,
  adminContentNewsRoute,
} from "@/lib/admin/routes";
import {
  useArticleMetadata,
  useContentSeoAnalysis,
  useCreateContent,
  useNewsMetadata,
  useUpdateArticleMetadata,
  useUpdateContent,
  useUpdateNewsMetadata,
  useUpdateSeoMetadata,
} from "@/lib/admin/content/content-hooks";
import {
  buildArticlePayload,
  buildNewsPayload,
  buildSeoPayload,
  hasFormErrors,
  isKnownContentType,
  labelForContentType,
  mapSeoForm,
  normalizeContentStatus,
  slugify,
  validateArticleForm,
  validateContentForm,
  validateNewsForm,
  validateSeoForm,
} from "@/lib/admin/content/content-mappers";
import {
  CONTENT_LIMITS,
  CONTENT_TYPES,
  EMPTY_SEO_FORM,
  type AdminContentDetail,
  type ArticleFormErrors,
  type ContentFormErrors,
  type ContentFormValues,
  type ContentStatusValue,
  type ContentTypeValue,
  type NewsFormErrors,
  type SeoFormErrors,
  type SeoFormValues,
} from "@/lib/admin/content/content-types";
import { clearDraft, loadDraft, saveDraft } from "@/lib/admin/content/editor-draft";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import { MarkdownEditor } from "@/components/admin/content/editor/markdown-editor";
import { ContentOutline } from "@/components/admin/content/editor/content-outline";
import { ContentPreviewPanel } from "@/components/admin/content/editor/content-preview-panel";
import { ContentStatisticsCard } from "@/components/admin/content/editor/content-statistics-card";
import { ContentQualityPanel } from "@/components/admin/content/editor/content-quality-panel";
import {
  SaveStatusIndicator,
  type SaveState,
} from "@/components/admin/content/editor/save-status";
import { SeoPanel } from "@/components/admin/content/seo/seo-panel";
import { ArticleSettingsPanel } from "@/components/admin/content/workspaces/article/article-settings-panel";
import { NewsSettingsFields } from "@/components/admin/content/workspaces/news/news-settings-fields";
import { MediaPickerDialog } from "@/components/admin/media/media-picker-dialog";
import type { MediaPickerSelection } from "@/lib/admin/media/media-types";
import { WorkflowPanel } from "@/components/admin/content/workflow/workflow-panel";
import { AdminSurface } from "@/components/admin/page/admin-surface";

function initialContentValues(
  initial: AdminContentDetail | undefined,
  createType: ContentTypeValue,
): ContentFormValues {
  if (!initial) {
    return {
      title: "",
      slug: "",
      type: createType,
      body: "",
      status: "Draft",
      excerpt: "",
      coverImage: "",
    };
  }

  return {
    title: initial.title,
    slug: initial.slug,
    type: isKnownContentType(initial.type) ? initial.type : "Article",
    body: initial.body,
    status: initial.status,
    excerpt: initial.excerpt,
    coverImage: initial.coverImage,
  };
}

/**
 * Advanced Content Studio (edit mode). Three columns on desktop:
 * outline (left), editor/preview (center), SEO + analysis (right). Content and
 * SEO have separate server saves; unsaved edits are recovered locally.
 *
 * Loads exclusively from the Admin Read Model (`GET /admin/content/{id}`), so
 * body, excerpt, cover image and SEO are pre-filled for drafts and published
 * items alike (a local draft, when present, overrides the recovered text).
 */
export function ContentStudio({
  initial,
  createType = "Article",
}: {
  initial?: AdminContentDetail;
  createType?: ContentTypeValue;
}) {
  const router = useRouter();
  const create = useCreateContent();
  const update = useUpdateContent();
  const seoMutation = useUpdateSeoMetadata();
  const articleMutation = useUpdateArticleMetadata();
  const newsMutation = useUpdateNewsMetadata();
  const contentId = initial?.id ?? null;
  const contentType = initial?.type ?? createType;
  const draftKey = contentId ?? `new-${createType.toLowerCase()}`;
  const seoAnalysis = useContentSeoAnalysis(contentId);
  const articleMeta = useArticleMetadata(contentType === "Article" ? contentId : null);
  const newsMeta = useNewsMetadata(contentType === "News" ? contentId : null);
  // Set right after a successful save; consumed (and cleared) by the effect
  // below to auto-rerun the analysis exactly once — never in a loop.
  const autoRerunAfterSaveRef = useRef(false);

  const [values, setValues] = useState<ContentFormValues>(() =>
    initialContentValues(initial, createType),
  );
  const [errors, setErrors] = useState<ContentFormErrors>({});
  const [seo, setSeo] = useState<SeoFormValues>(() => initial?.seo ?? { ...EMPTY_SEO_FORM });
  const [seoErrors, setSeoErrors] = useState<SeoFormErrors>({});
  const [articleErrors, setArticleErrors] = useState<ArticleFormErrors>({});
  const [newsErrors, setNewsErrors] = useState<NewsFormErrors>({});
  const [status, setStatus] = useState<ContentStatusValue>(initial?.status ?? "Draft");

  const [contentSave, setContentSave] = useState<SaveState>("idle");
  const [seoSave, setSeoSave] = useState<SaveState>("idle");
  const [articleSave, setArticleSave] = useState<SaveState>("idle");
  const [newsSave, setNewsSave] = useState<SaveState>("idle");
  const [draftRecovered, setDraftRecovered] = useState(false);
  const [centerTab, setCenterTab] = useState<"edit" | "preview">("edit");
  // Which URL field the Media Library picker is currently targeting. Selecting
  // an asset never auto-saves — it only fills the field's value, same as
  // typing a URL manually, so it goes through the normal onChange/onSeoChange
  // paths (marks the form dirty and the SEO analysis stale).
  const [pickerTarget, setPickerTarget] = useState<"cover" | "og" | null>(null);

  const savedSnapshot = useRef<string>(
    JSON.stringify(initialContentValues(initial, createType)),
  );

  // Recover a local draft for this item on mount.
  useEffect(() => {
    const draft = loadDraft(draftKey);
    if (draft) {
      setValues((prev) => ({
        ...prev,
        title: draft.title,
        body: draft.body,
        excerpt: draft.excerpt,
      }));
      setDraftRecovered(true);
      setContentSave("unsaved");
    }
  }, [draftKey]);

  // Persist a minimal local draft as the author types (no server autosave).
  useEffect(() => {
    saveDraft({
      contentId: draftKey,
      title: values.title,
      body: values.body,
      excerpt: values.excerpt,
    });
  }, [draftKey, values.title, values.body, values.excerpt]);

  const onChange = useCallback(
    (patch: Partial<ContentFormValues>) => {
      setValues((prev) => ({ ...prev, ...patch }));
      setContentSave((prev) => (prev === "saving" ? prev : "unsaved"));
      // No-op unless a report already exists; never triggers a network call.
      seoAnalysis.markStale();
    },
    [seoAnalysis],
  );

  const onSeoChange = useCallback(
    (patch: Partial<SeoFormValues>) => {
      setSeo((prev) => ({ ...prev, ...patch }));
      setSeoSave((prev) => (prev === "saving" ? prev : "unsaved"));
      seoAnalysis.markStale();
    },
    [seoAnalysis],
  );

  // Fills the targeted URL field with the picked asset's resolved absolute
  // URL — the same shape the field already expects from manual entry — then
  // closes the picker. Never triggers a save by itself.
  const handleMediaSelect = useCallback(
    (selection: MediaPickerSelection) => {
      if (pickerTarget === "cover") {
        onChange({ coverImage: selection.absoluteUrl });
      } else if (pickerTarget === "og") {
        onSeoChange({ ogImage: selection.absoluteUrl });
      }
      setPickerTarget(null);
    },
    [pickerTarget, onChange, onSeoChange],
  );

  const isDirty = useMemo(
    () => JSON.stringify(values) !== savedSnapshot.current,
    [values],
  );

  const discardDraft = useCallback(() => {
    clearDraft();
    const restored = initialContentValues(initial, createType);
    setValues(restored);
    savedSnapshot.current = JSON.stringify(restored);
    setDraftRecovered(false);
    setContentSave("idle");
  }, [initial, createType]);

  const saveContent = useCallback(async () => {
    const validation = validateContentForm(values);
    setErrors(validation);
    if (hasFormErrors(validation)) {
      setContentSave("error");
      return;
    }
    if (!contentId && createType === "Article") {
      const articleValidation = validateArticleForm(articleMeta.values);
      setArticleErrors(articleValidation);
      if (hasFormErrors(articleValidation)) {
        setContentSave("error");
        return;
      }
    }
    if (!contentId && createType === "News") {
      const newsValidation = validateNewsForm(newsMeta.values);
      setNewsErrors(newsValidation);
      if (hasFormErrors(newsValidation)) {
        setContentSave("error");
        return;
      }
    }
    setContentSave("saving");
    try {
      if (!contentId) {
        const created = await create.create({
          title: values.title.trim(),
          slug: values.slug.trim(),
          type: createType,
          body: values.body,
          status: "Draft",
        });

        await update.run(created.id, {
          title: values.title.trim(),
          slug: values.slug.trim(),
          type: createType,
          body: values.body,
          excerpt: values.excerpt.trim() ? values.excerpt.trim() : null,
          coverImage: values.coverImage.trim() ? values.coverImage.trim() : null,
        });

        if (Object.values(seo).some((value) => value.trim())) {
          await seoMutation.run(created.id, buildSeoPayload(seo));
        }
        if (createType === "Article") {
          await articleMutation.run(created.id, buildArticlePayload(articleMeta.values));
        }
        if (createType === "News") {
          await newsMutation.run(created.id, buildNewsPayload(newsMeta.values));
        }

        clearDraft();
        setContentSave("saved");
        const createdRoute =
          createType === "Article"
            ? adminContentArticleRoute(created.id)
            : createType === "News"
              ? adminContentNewsRoute(created.id)
              : adminContentItemRoute(created.id);
        router.replace(createdRoute);
        router.refresh();
        return;
      }

      const detail = await update.run(contentId, {
        title: values.title.trim(),
        slug: values.slug.trim(),
        type: values.type,
        body: values.body,
        excerpt: values.excerpt.trim() ? values.excerpt.trim() : null,
        coverImage: values.coverImage.trim() ? values.coverImage.trim() : null,
      });
      setStatus(normalizeContentStatus(detail.contentStatus));
      savedSnapshot.current = JSON.stringify(values);
      clearDraft();
      setDraftRecovered(false);
      setContentSave("saved");
      // Only worth auto-rerunning if the author had already analyzed once.
      if (seoAnalysis.report) autoRerunAfterSaveRef.current = true;
      seoAnalysis.markStale();
    } catch {
      setContentSave("error");
    }
  }, [
    values,
    contentId,
    create,
    createType,
    update,
    seo,
    seoMutation,
    articleMutation,
    articleMeta.values,
    newsMutation,
    newsMeta.values,
    router,
    seoAnalysis,
  ]);

  const saveSeo = useCallback(async () => {
    if (!contentId) return;
    const validation = validateSeoForm(seo);
    setSeoErrors(validation);
    if (hasFormErrors(validation)) {
      setSeoSave("error");
      return;
    }
    setSeoSave("saving");
    try {
      const detail = await seoMutation.run(contentId, buildSeoPayload(seo));
      setSeo(mapSeoForm(detail));
      setStatus(normalizeContentStatus(detail.contentStatus));
      setSeoSave("saved");
      if (seoAnalysis.report) autoRerunAfterSaveRef.current = true;
      seoAnalysis.markStale();
    } catch {
      setSeoSave("error");
    }
  }, [contentId, seo, seoMutation, seoAnalysis]);

  const saveArticle = useCallback(async () => {
    if (!contentId) return;
    const validation = validateArticleForm(articleMeta.values);
    setArticleErrors(validation);
    if (hasFormErrors(validation)) {
      setArticleSave("error");
      return;
    }
    setArticleSave("saving");
    try {
      const saved = await articleMutation.run(contentId, buildArticlePayload(articleMeta.values));
      articleMeta.replaceValues({
        categoryId: saved.categoryId ?? "",
        difficultyLevel:
          saved.difficultyLevel === "Intermediate" || saved.difficultyLevel === "Advanced"
            ? saved.difficultyLevel
            : "Beginner",
        readingTimeMinutes: String(saved.readingTimeMinutes),
        isFeatured: saved.isFeatured,
        allowComments: saved.allowComments,
        tableOfContentsEnabled: saved.tableOfContentsEnabled,
      });
      setArticleSave("saved");
    } catch {
      setArticleSave("error");
    }
  }, [contentId, articleMeta, articleMutation]);

  const saveNews = useCallback(async () => {
    if (!contentId) return;
    const validation = validateNewsForm(newsMeta.values);
    setNewsErrors(validation);
    if (hasFormErrors(validation)) {
      setNewsSave("error");
      return;
    }
    setNewsSave("saving");
    try {
      const saved = await newsMutation.run(contentId, buildNewsPayload(newsMeta.values));
      newsMeta.replaceValues({
        sourceName: saved.sourceName,
        sourceUrl: saved.sourceUrl ?? "",
        newsDateUtc: saved.newsDateUtc.slice(0, 16),
        priority:
          saved.priority === "Featured" || saved.priority === "Breaking"
            ? saved.priority
            : "Normal",
        externalReference: saved.externalReference ?? "",
      });
      setNewsSave("saved");
    } catch {
      setNewsSave("error");
    }
  }, [contentId, newsMeta, newsMutation]);

  // Auto-rerun exactly once after a save, only if the author had already
  // analyzed at least once before. Guarded by the ref so this never loops:
  // the flag is cleared synchronously before calling analyze().
  useEffect(() => {
    if (!autoRerunAfterSaveRef.current) return;
    if (seoAnalysis.status !== "stale") return;
    autoRerunAfterSaveRef.current = false;
    seoAnalysis.analyze();
  }, [seoAnalysis]);

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={contentId ? "استودیوی محتوا" : `${labelForContentType(createType)} جدید`}
        description="ویرایش پیشرفته، پیش‌نمایش زنده و مدیریت سئو"
        badge={<ContentStatusBadge status={status} />}
        secondaryActions={
          <Link
            href={
              contentId
                ? `${ADMIN_ROUTES.content}/${encodeURIComponent(contentId)}`
                : ADMIN_ROUTES.contentArticles
            }
            className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="chevron" size={16} />
            بازگشت
          </Link>
        }
      />

      {contentId && initial ? (
        <WorkflowPanel
          contentId={contentId}
          authorId={initial.authorId}
          status={status}
          onStatusChange={setStatus}
          compact
        />
      ) : (
        <div className="rounded-xl border border-[var(--adm-warning-soft)] bg-[var(--adm-warning-soft)] px-3 py-2.5 text-[12px] font-semibold text-[var(--adm-warning)]">
          برای فعال‌شدن گردش کار و تحلیل سئو، ابتدا {labelForContentType(createType)} را ایجاد
          کنید. تنظیمات همین صفحه همراه اولین ذخیره ثبت می‌شوند.
        </div>
      )}

      {draftRecovered ? (
        <div className="flex flex-wrap items-center justify-between gap-2 rounded-lg border border-[var(--adm-warning-soft)] bg-[var(--adm-warning-soft)] px-3 py-2">
          <span className="text-[12px] font-semibold text-[var(--adm-warning)]">
            پیش‌نویس محلی ذخیره‌نشده بازیابی شد.
          </span>
          <button
            type="button"
            onClick={discardDraft}
            className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]"
          >
            دور انداختن پیش‌نویس
          </button>
        </div>
      ) : null}

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-[220px_minmax(0,1fr)_360px]">
        {/* LEFT — content navigation */}
        <aside className="order-3 space-y-4 xl:order-1">
          <div className="adm-surface rounded-xl p-4">
            <ContentOutline body={values.body} />
          </div>
        </aside>

        {/* CENTER — editor / preview */}
        <div className="order-1 space-y-4 xl:order-2">
          <div className="adm-surface space-y-4 rounded-xl p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <div
                role="tablist"
                aria-label="نمای مرکزی"
                className="inline-flex rounded-lg border border-[var(--adm-border)] p-0.5"
              >
                {(["edit", "preview"] as const).map((tab) => (
                  <button
                    key={tab}
                    type="button"
                    role="tab"
                    aria-selected={centerTab === tab}
                    onClick={() => setCenterTab(tab)}
                    className={`adm-focus rounded-md px-3 py-1 text-[12px] font-semibold ${
                      centerTab === tab
                        ? "bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]"
                        : "adm-muted"
                    }`}
                  >
                    {tab === "edit" ? "ویرایش" : "پیش‌نمایش"}
                  </button>
                ))}
              </div>
              <div className="flex items-center gap-3">
                <SaveStatusIndicator state={contentSave} />
                <button
                  type="button"
                  onClick={() => void saveContent()}
                  disabled={create.submitting || update.submitting || !isDirty}
                  className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
                >
                  <AdminIcon name="check" size={16} />
                  {contentId ? "ذخیره محتوا" : `ایجاد ${labelForContentType(createType)}`}
                </button>
              </div>
            </div>

            {centerTab === "edit" ? (
              <div className="space-y-4">
                <StudioField id="studio-title" label="عنوان" error={errors.title} required>
                  <input
                    id="studio-title"
                    type="text"
                    className="adm-input"
                    value={values.title}
                    maxLength={200}
                    onChange={(event) => onChange({ title: event.target.value })}
                    aria-invalid={Boolean(errors.title)}
                  />
                </StudioField>

                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <StudioField
                    id="studio-slug"
                    label="اسلاگ"
                    error={errors.slug}
                    required
                  >
                    <div className="flex items-center gap-1.5">
                      <input
                        id="studio-slug"
                        type="text"
                        dir="ltr"
                        className="adm-input min-w-0 flex-1 text-start"
                        value={values.slug}
                        maxLength={300}
                        onChange={(event) => onChange({ slug: event.target.value })}
                        aria-invalid={Boolean(errors.slug)}
                      />
                      <button
                        type="button"
                        title="تولید اسلاگ از عنوان"
                        aria-label="تولید اسلاگ از عنوان"
                        onClick={() => onChange({ slug: slugify(values.title) })}
                        className="adm-btn adm-btn-ghost adm-focus h-[38px] shrink-0 px-2 text-[11px] leading-none"
                      >
                        تولید
                      </button>
                    </div>
                  </StudioField>

                  <StudioField id="studio-type" label="نوع محتوا" error={errors.type} required>
                    <select
                      id="studio-type"
                      className="adm-input"
                      value={values.type}
                      disabled={!contentId}
                      onChange={(event) =>
                        onChange({ type: event.target.value as ContentFormValues["type"] })
                      }
                    >
                      {CONTENT_TYPES.map((type) => (
                        <option key={type} value={type}>
                          {labelForContentType(type)}
                        </option>
                      ))}
                    </select>
                  </StudioField>
                </div>

                <StudioField
                  id="studio-excerpt"
                  label="خلاصه"
                  error={errors.excerpt}
                  hint={`حداکثر ${CONTENT_LIMITS.excerpt} نویسه.`}
                >
                  <textarea
                    id="studio-excerpt"
                    className="adm-input min-h-[72px] resize-y text-[13px] leading-6"
                    value={values.excerpt}
                    onChange={(event) => onChange({ excerpt: event.target.value })}
                    aria-invalid={Boolean(errors.excerpt)}
                  />
                </StudioField>

                <StudioField
                  id="studio-cover"
                  label="آدرس تصویر کاور"
                  error={errors.coverImage}
                  hint="نشانی http(s) تصویر کاور، یا انتخاب از رسانه‌ها."
                  action={
                    <button
                      type="button"
                      onClick={() => setPickerTarget("cover")}
                      className="adm-btn adm-btn-ghost adm-focus inline-flex items-center gap-1 px-2 py-1 text-[11px]"
                    >
                      <AdminIcon name="media" size={14} />
                      انتخاب از رسانه‌ها
                    </button>
                  }
                >
                  <input
                    id="studio-cover"
                    type="text"
                    dir="ltr"
                    className="adm-input text-start"
                    value={values.coverImage}
                    placeholder="https://cdn.example.com/cover.png"
                    onChange={(event) => onChange({ coverImage: event.target.value })}
                    aria-invalid={Boolean(errors.coverImage)}
                  />
                </StudioField>

                <div className="space-y-1.5">
                  <span className="adm-text text-[12px] font-semibold">
                    متن محتوا<span className="text-[var(--adm-danger)]"> *</span>
                  </span>
                  <MarkdownEditor
                    value={values.body}
                    onChange={(body) => onChange({ body })}
                    error={errors.body}
                    ariaInvalid={Boolean(errors.body)}
                  />
                </div>
              </div>
            ) : (
              <ContentPreviewPanel values={values} bare />
            )}
          </div>
        </div>

        {/* RIGHT — SEO + type-specific settings */}
        <aside className="order-2 space-y-4 xl:order-3">
          {contentType === "Article" ? (
            <AdminSurface className="p-4">
              <ArticleSettingsPanel
                values={articleMeta.values}
                errors={articleErrors}
                onChange={articleMeta.setValues}
                onSave={() => void saveArticle()}
                saveState={articleSave}
                error={articleMutation.error ?? articleMeta.error}
                disabled={articleMutation.submitting}
                saveDisabled={!contentId}
                loading={articleMeta.loading}
              />
            </AdminSurface>
          ) : null}
          {contentType === "News" ? (
            <AdminSurface className="p-4">
              <NewsSettingsFields
                values={newsMeta.values}
                errors={newsErrors}
                onChange={newsMeta.setValues}
                onSave={() => void saveNews()}
                saveState={newsSave}
                error={newsMutation.error ?? newsMeta.error}
                disabled={newsMutation.submitting || newsMeta.loading}
                hideSave={!contentId}
              />
            </AdminSurface>
          ) : null}
          <div className="adm-surface rounded-xl p-4">
            <SeoPanel
              values={seo}
              errors={seoErrors}
              onChange={onSeoChange}
              onSave={() => void saveSeo()}
              saveState={seoSave}
              error={seoMutation.error}
              disabled={!contentId || seoMutation.submitting}
              contentTitle={values.title}
              excerpt={values.excerpt}
              coverImage={values.coverImage}
              slug={values.slug}
              analysisStatus={seoAnalysis.status}
              analysisReport={seoAnalysis.report}
              analysisError={seoAnalysis.error}
              onAnalyze={seoAnalysis.analyze}
              onPickOgImage={() => setPickerTarget("og")}
            />
          </div>
          <div className="adm-surface rounded-xl p-4">
            <ContentStatisticsCard body={values.body} />
          </div>
          <div className="adm-surface rounded-xl p-4">
            <ContentQualityPanel
              title={values.title}
              description={values.excerpt}
              body={values.body}
            />
          </div>
        </aside>
      </div>

      <MediaPickerDialog
        open={pickerTarget !== null}
        onClose={() => setPickerTarget(null)}
        onSelect={handleMediaSelect}
        title={pickerTarget === "cover" ? "انتخاب تصویر کاور" : "انتخاب تصویر OG"}
      />
    </div>
  );
}

function StudioField({
  id,
  label,
  error,
  hint,
  required,
  action,
  children,
}: {
  id: string;
  label: string;
  error?: string;
  hint?: string;
  required?: boolean;
  action?: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between gap-2">
        <label htmlFor={id} className="adm-text text-[12px] font-semibold">
          {label}
          {required ? <span className="text-[var(--adm-danger)]"> *</span> : null}
        </label>
        {action}
      </div>
      {children}
      {hint && !error ? <p className="adm-subtle text-[11px]">{hint}</p> : null}
      {error ? (
        <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{error}</p>
      ) : null}
    </div>
  );
}
