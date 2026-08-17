"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { AuthModal, useAuth } from "@/components/auth";
import { PageHeader } from "@/components/layout";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { MarkdownEditor } from "@/components/admin/content/editor/markdown-editor";
import {
  SaveStatusIndicator,
  type SaveState,
} from "@/components/admin/content/editor/save-status";
import { MediaPickerDialog } from "@/components/admin/media/media-picker-dialog";
import { WriterPromptStatusBadge } from "@/components/admin/prompt-lab/writer-prompt-status-badge";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { useWriterPromptCatalog } from "@/lib/admin/prompt-lab/writer-prompt-hooks";
import {
  hasWriterPromptFormErrors,
  labelForWriterPromptCategory,
  labelForWriterPromptMediaType,
  slugifyWriterPromptTitle,
  validateWriterPromptForm,
} from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import {
  EMPTY_WRITER_PROMPT_FORM,
  WRITER_PROMPT_LIMITS,
  WRITER_PROMPT_MEDIA_TYPES,
  type WriterPromptFormErrors,
  type WriterPromptFormValues,
  type WriterPromptMediaType,
  type WriterPromptStatus,
} from "@/lib/admin/prompt-lab/writer-prompt-types";
import {
  createWriterPrompt,
  submitWriterPrompt,
  updateWriterPrompt,
  type WriterPromptDetailsDto,
} from "@/lib/api/promptlab-writer";
import { ApiClientError } from "@/lib/api/errors";
import type { MediaPickerSelection } from "@/lib/admin/media/media-types";

type WriterPromptEditorProps = {
  variant?: "public" | "admin";
};

function toPayload(values: WriterPromptFormValues) {
  return {
    title: values.title.trim(),
    slug: values.slug.trim().toLowerCase(),
    description: values.description.trim() || null,
    content: values.content.trim(),
    coverImage: values.coverImage.trim() || null,
    mediaType: values.mediaType,
    categoryId: values.categoryId,
    aiModelId: values.aiModelId,
  };
}

function applyDetails(details: WriterPromptDetailsDto): WriterPromptFormValues {
  return {
    title: details.title,
    slug: details.slug,
    description: details.description ?? "",
    coverImage: details.coverImage ?? "",
    content: details.content,
    aiModelId: details.aiModelId,
    categoryId: details.categoryId,
    mediaType: details.mediaType === "Image" ? "Image" : "Text",
    tags: "",
  };
}

function errorMessage(error: unknown): string {
  if (error instanceof ApiClientError) {
    if (error.isNetworkError) return "اتصال به سرور برقرار نشد.";
    if (error.isForbidden) return "برای این کار دسترسی نویسنده لازم است.";
    if (error.isConflict) return "اسلاگ تکراری است. اسلاگ دیگری انتخاب کنید.";
    return error.message || "درخواست ناموفق بود.";
  }
  return "مشکلی پیش آمد. لطفاً دوباره تلاش کنید.";
}

/** Writer prompt create/edit: save draft + submit for review. Never publishes. */
export function WriterPromptEditor({ variant = "public" }: WriterPromptEditorProps) {
  const router = useRouter();
  const { user, token, isReady } = useAuth();
  const catalog = useWriterPromptCatalog();
  const [authOpen, setAuthOpen] = useState(false);

  const [values, setValues] = useState<WriterPromptFormValues>(EMPTY_WRITER_PROMPT_FORM);
  const [errors, setErrors] = useState<WriterPromptFormErrors>({});
  const [slugTouched, setSlugTouched] = useState(false);
  const [promptId, setPromptId] = useState<string | null>(null);
  const [status, setStatus] = useState<WriterPromptStatus>("Draft");
  const [saveState, setSaveState] = useState<SaveState>("idle");
  const [submitting, setSubmitting] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [pickerOpen, setPickerOpen] = useState(false);
  const savedSnapshot = useRef(JSON.stringify(EMPTY_WRITER_PROMPT_FORM));

  const canWrite = user?.role === "Writer" || user?.role === "Admin";
  const canEdit = status === "Draft";
  const busy = saveState === "saving" || submitting;

  useEffect(() => {
    if (slugTouched || promptId) return;
    if (!values.title.trim()) {
      setValues((prev) => ({ ...prev, slug: "" }));
      return;
    }
    setValues((prev) => ({ ...prev, slug: slugifyWriterPromptTitle(prev.title) }));
  }, [values.title, slugTouched, promptId]);

  const onChange = useCallback((patch: Partial<WriterPromptFormValues>) => {
    setValues((prev) => ({ ...prev, ...patch }));
    setSaveState((prev) => (prev === "saving" ? prev : "unsaved"));
    setActionError(null);
  }, []);

  const isDirty = useMemo(
    () => JSON.stringify(values) !== savedSnapshot.current,
    [values],
  );

  const persistDraft = useCallback(async (): Promise<WriterPromptDetailsDto | null> => {
    if (!token) return null;
    const validation = validateWriterPromptForm(values);
    setErrors(validation);
    if (hasWriterPromptFormErrors(validation)) {
      setSaveState("error");
      return null;
    }

    setSaveState("saving");
    setActionError(null);
    try {
      const payload = toPayload(values);
      const saved = promptId
        ? await updateWriterPrompt(token, promptId, payload)
        : await createWriterPrompt(token, payload);
      setPromptId(saved.id);
      setStatus(saved.status);
      const next = applyDetails(saved);
      next.tags = values.tags;
      setValues(next);
      savedSnapshot.current = JSON.stringify(next);
      setSaveState("saved");
      return saved;
    } catch (error) {
      setSaveState("error");
      setActionError(errorMessage(error));
      return null;
    }
  }, [token, values, promptId]);

  const handleSaveDraft = useCallback(async () => {
    await persistDraft();
  }, [persistDraft]);

  const handleSubmitForReview = useCallback(async () => {
    if (!token || !canEdit) return;
    setSubmitting(true);
    try {
      const saved = isDirty || !promptId ? await persistDraft() : { id: promptId, status };
      if (!saved?.id) return;
      const submitted = await submitWriterPrompt(token, saved.id);
      setPromptId(submitted.id);
      setStatus(submitted.status);
      setSaveState("saved");
      setActionError(null);
      router.push(ADMIN_ROUTES.promptLab);
    } catch (error) {
      setActionError(errorMessage(error));
    } finally {
      setSubmitting(false);
    }
  }, [token, canEdit, isDirty, promptId, persistDraft, status, router]);

  if (!isReady) {
    return <AdminLoadingState cards={0} rows={6} />;
  }

  if (!user || !token) {
    return (
      <>
        <EditorChrome variant={variant} />
        <AdminSurface className="space-y-4">
          <p className="adm-muted text-[13px] leading-6">
            برای ایجاد پرامپت ابتدا وارد حساب کاربری شوید. نقش نویسنده یا ادمین لازم است.
          </p>
          <button
            type="button"
            onClick={() => setAuthOpen(true)}
            className="adm-btn adm-btn-primary adm-focus"
          >
            ورود / ثبت‌نام
          </button>
        </AdminSurface>
        <AuthModal open={authOpen} onClose={() => setAuthOpen(false)} />
      </>
    );
  }

  if (!canWrite) {
    return (
      <>
        <EditorChrome variant={variant} />
        <AdminSurface className="space-y-4">
          <p className="adm-muted text-[13px] leading-6">
            نقش فعلی شما اجازهٔ ایجاد پرامپت ندارد. یک ادمین باید نقش شما را به نویسنده ارتقا دهد.
          </p>
          <Link href="/profile" className="adm-btn adm-btn-outline adm-focus">
            رفتن به پروفایل
          </Link>
        </AdminSurface>
      </>
    );
  }

  return (
    <div className="space-y-6">
      <EditorChrome
        variant={variant}
        badge={<WriterPromptStatusBadge status={status} />}
        actions={
          <div className="flex flex-wrap items-center gap-2">
            <SaveStatusIndicator state={saveState} />
            <button
              type="button"
              onClick={() => void handleSaveDraft()}
              disabled={busy || !canEdit || (!isDirty && Boolean(promptId))}
              className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
            >
              <AdminIcon name="check" size={16} />
              ذخیره پیش‌نویس
            </button>
            <button
              type="button"
              onClick={() => void handleSubmitForReview()}
              disabled={busy || !canEdit}
              className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
            >
              <AdminIcon name="outbox" size={16} />
              {submitting ? "در حال ارسال…" : "ارسال برای بررسی"}
            </button>
          </div>
        }
      />

      <p className="adm-subtle text-[12px] leading-6">
        پرامپت به‌صورت پیش‌نویس ذخیره می‌شود و تا تأیید بازبین منتشر نمی‌شود.
      </p>

      {actionError ? (
        <p className="rounded-xl border border-[var(--adm-danger)]/25 bg-[var(--adm-danger-soft)] px-4 py-3 text-[13px] text-[var(--adm-danger)]">
          {actionError}
        </p>
      ) : null}

      {catalog.loading && catalog.categories.length === 0 ? (
        <AdminLoadingState cards={0} rows={4} />
      ) : catalog.error && catalog.categories.length === 0 ? (
        <AdminErrorState error={catalog.error} onRetry={catalog.reload} showHome={false} />
      ) : (
        <div className="grid grid-cols-1 gap-4 lg:grid-cols-[minmax(0,1fr)_280px]">
          <AdminSurface className="space-y-4">
            <Field id="writer-prompt-title" label="عنوان" error={errors.title} required>
              <input
                id="writer-prompt-title"
                type="text"
                className="adm-input"
                value={values.title}
                maxLength={WRITER_PROMPT_LIMITS.title}
                disabled={!canEdit}
                onChange={(event) => onChange({ title: event.target.value })}
                aria-invalid={Boolean(errors.title)}
              />
            </Field>

            <Field
              id="writer-prompt-slug"
              label="اسلاگ"
              error={errors.slug}
              required
              hint="از عنوان ساخته می‌شود؛ فقط انگلیسی کوچک، عدد و خط تیره."
            >
              <input
                id="writer-prompt-slug"
                type="text"
                dir="ltr"
                className="adm-input text-start"
                value={values.slug}
                maxLength={WRITER_PROMPT_LIMITS.slug}
                disabled={!canEdit}
                onChange={(event) => {
                  setSlugTouched(true);
                  onChange({ slug: event.target.value });
                }}
                aria-invalid={Boolean(errors.slug)}
              />
            </Field>

            <Field
              id="writer-prompt-description"
              label="توضیح"
              error={errors.description}
              hint={`حداکثر ${WRITER_PROMPT_LIMITS.description} نویسه.`}
            >
              <textarea
                id="writer-prompt-description"
                className="adm-input min-h-[88px] resize-y text-[13px] leading-6"
                value={values.description}
                maxLength={WRITER_PROMPT_LIMITS.description}
                disabled={!canEdit}
                onChange={(event) => onChange({ description: event.target.value })}
                aria-invalid={Boolean(errors.description)}
              />
            </Field>

            <Field
              id="writer-prompt-cover"
              label="تصویر کاور"
              error={errors.coverImage}
              hint="نشانی http(s) یا انتخاب از رسانه‌ها."
              action={
                <button
                  type="button"
                  onClick={() => setPickerOpen(true)}
                  disabled={!canEdit}
                  className="adm-btn adm-btn-ghost adm-focus inline-flex items-center gap-1 px-2 py-1 text-[11px]"
                >
                  <AdminIcon name="media" size={14} />
                  انتخاب از رسانه‌ها
                </button>
              }
            >
              <input
                id="writer-prompt-cover"
                type="text"
                dir="ltr"
                className="adm-input text-start"
                value={values.coverImage}
                placeholder="https://cdn.example.com/cover.png"
                disabled={!canEdit}
                onChange={(event) => onChange({ coverImage: event.target.value })}
                aria-invalid={Boolean(errors.coverImage)}
              />
            </Field>

            <div className="space-y-1.5">
              <span className="adm-text text-[12px] font-semibold">
                متن پرامپت<span className="text-[var(--adm-danger)]"> *</span>
              </span>
              <MarkdownEditor
                value={values.content}
                onChange={(content) => onChange({ content })}
                disabled={!canEdit}
                error={errors.content}
                ariaInvalid={Boolean(errors.content)}
              />
            </div>
          </AdminSurface>

          <aside className="space-y-4">
            <AdminSurface className="space-y-4">
              <h2 className="adm-text text-[13px] font-bold">تنظیمات انتشار</h2>
              <Field id="writer-prompt-model" label="مدل هوش مصنوعی" error={errors.aiModelId} required>
                <select
                  id="writer-prompt-model"
                  className="adm-input"
                  value={values.aiModelId}
                  disabled={!canEdit}
                  onChange={(event) => onChange({ aiModelId: event.target.value })}
                  aria-invalid={Boolean(errors.aiModelId)}
                >
                  <option value="">انتخاب کنید</option>
                  {catalog.aiModels.map((model) => (
                    <option key={model.id} value={model.id}>
                      {model.name} — {model.provider}
                    </option>
                  ))}
                </select>
              </Field>

              <Field id="writer-prompt-category" label="دسته‌بندی" error={errors.categoryId} required>
                <select
                  id="writer-prompt-category"
                  className="adm-input"
                  value={values.categoryId}
                  disabled={!canEdit}
                  onChange={(event) => onChange({ categoryId: event.target.value })}
                  aria-invalid={Boolean(errors.categoryId)}
                >
                  <option value="">انتخاب کنید</option>
                  {catalog.categories.map((category) => (
                    <option key={category.id} value={category.id}>
                      {labelForWriterPromptCategory(category.name, category.slug)}
                    </option>
                  ))}
                </select>
              </Field>

              <Field id="writer-prompt-media" label="نوع رسانه" error={errors.mediaType} required>
                <select
                  id="writer-prompt-media"
                  className="adm-input"
                  value={values.mediaType}
                  disabled={!canEdit}
                  onChange={(event) =>
                    onChange({ mediaType: event.target.value as WriterPromptMediaType })
                  }
                >
                  {WRITER_PROMPT_MEDIA_TYPES.map((value) => (
                    <option key={value} value={value}>
                      {labelForWriterPromptMediaType(value)}
                    </option>
                  ))}
                </select>
              </Field>

              <Field
                id="writer-prompt-tags"
                label="برچسب‌ها"
                hint="با کاما جدا کنید. در API فعلی نویسنده ذخیره نمی‌شود."
              >
                <input
                  id="writer-prompt-tags"
                  type="text"
                  className="adm-input"
                  value={values.tags}
                  disabled={!canEdit}
                  placeholder="کدنویسی، بازبینی، معماری"
                  onChange={(event) => onChange({ tags: event.target.value })}
                />
              </Field>
            </AdminSurface>

            <AdminSurface className="space-y-2">
              <h2 className="adm-text text-[13px] font-bold">گردش کار</h2>
              <p className="adm-muted text-[12px] leading-6">
                پیش‌نویس ← ارسال برای بررسی ← تأیید. انتشار فقط پس از تأیید انجام می‌شود و از این
                صفحه در دسترس نیست.
              </p>
              <Link
                href={ADMIN_ROUTES.promptLab}
                className="adm-btn adm-btn-ghost adm-focus text-[12px]"
              >
                بازگشت به داشبورد
              </Link>
            </AdminSurface>
          </aside>
        </div>
      )}

      <MediaPickerDialog
        open={pickerOpen}
        onClose={() => setPickerOpen(false)}
        onSelect={(selection: MediaPickerSelection) => {
          onChange({ coverImage: selection.absoluteUrl });
          setPickerOpen(false);
        }}
        title="انتخاب تصویر کاور"
      />
    </div>
  );
}

function EditorChrome({
  variant,
  badge,
  actions,
}: {
  variant: "public" | "admin";
  badge?: React.ReactNode;
  actions?: React.ReactNode;
}) {
  if (variant === "admin") {
    return (
      <AdminPageHeader
        title="پرامپت جدید"
        description="ایجاد پیش‌نویس و ارسال برای بررسی — بدون انتشار خودکار"
        badge={badge}
        primaryAction={actions}
        breadcrumbs={[
          { title: "محتوا", href: ADMIN_ROUTES.content, current: false },
          { title: "Prompt Lab", href: ADMIN_ROUTES.contentPrompts, current: false },
          { title: "پرامپت جدید", current: true },
        ]}
      />
    );
  }

  return (
    <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <PageHeader
          title="پرامپت جدید"
          description="عنوان، متن و تنظیمات را وارد کنید، پیش‌نویس ذخیره کنید یا برای بررسی بفرستید."
        />
        {badge}
      </div>
      {actions}
    </div>
  );
}

function Field({
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

