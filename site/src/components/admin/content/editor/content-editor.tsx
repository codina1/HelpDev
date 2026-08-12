"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useState } from "react";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import {
  isKnownContentType,
  slugify,
  validateContentForm,
  hasFormErrors,
} from "@/lib/admin/content/content-mappers";
import { useCreateContent } from "@/lib/admin/content/content-hooks";
import type {
  ContentDetail,
  ContentFormErrors,
  ContentFormValues,
  ContentStatusValue,
} from "@/lib/admin/content/content-types";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentForm } from "@/components/admin/content/editor/content-form";
import { ContentPreviewPanel } from "@/components/admin/content/editor/content-preview-panel";
import { PublishPanel } from "@/components/admin/content/editor/publish-panel";

type ContentEditorProps = {
  mode: "create" | "edit";
  initial?: ContentDetail;
};

function initialValues(initial?: ContentDetail): ContentFormValues {
  return {
    title: initial?.title ?? "",
    slug: initial?.slug ?? "",
    type: initial && isKnownContentType(initial.type) ? initial.type : "Article",
    body: initial?.body ?? "",
    status: initial?.status ?? "Draft",
    excerpt: "",
    coverImage: "",
  };
}

/** Shared create/edit editor. Create persists via POST /content; edit is a
 * read/preview foundation because the backend has no update endpoint yet. */
export function ContentEditor({ mode, initial }: ContentEditorProps) {
  const router = useRouter();
  const create = useCreateContent();

  const [values, setValues] = useState<ContentFormValues>(() => initialValues(initial));
  const [errors, setErrors] = useState<ContentFormErrors>({});
  const [slugTouched, setSlugTouched] = useState<boolean>(Boolean(initial?.slug));

  const canMutate = mode === "create";
  const disabledReason =
    mode === "edit"
      ? "ذخیره ویرایش هنوز توسط سرور پشتیبانی نمی‌شود (به‌زودی)."
      : undefined;

  const onChange = useCallback(
    (patch: Partial<ContentFormValues>) => {
      setValues((prev) => {
        const next = { ...prev, ...patch };
        if (patch.slug !== undefined) setSlugTouched(true);
        if (patch.title !== undefined && !slugTouched && mode === "create") {
          next.slug = slugify(patch.title);
        }
        return next;
      });
    },
    [mode, slugTouched],
  );

  const onRegenerateSlug = useCallback(() => {
    setSlugTouched(true);
    setValues((prev) => ({ ...prev, slug: slugify(prev.title) }));
  }, []);

  const submit = useCallback(
    async (status: ContentStatusValue) => {
      if (mode !== "create") return; // edit persistence unsupported
      const next = { ...values, status };
      const validation = validateContentForm(next);
      setErrors(validation);
      if (hasFormErrors(validation)) return;

      try {
        const created = await create.create({
          title: next.title.trim(),
          slug: next.slug.trim(),
          body: next.body,
          type: next.type,
          status,
        });
        router.push(`${ADMIN_ROUTES.content}/${encodeURIComponent(created.id)}`);
      } catch {
        // Error surfaced via create.error in the PublishPanel.
      }
    },
    [mode, values, create, router],
  );

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={mode === "create" ? "محتوای جدید" : `ویرایش محتوا`}
        description={
          mode === "create"
            ? "ایجاد مقاله یا مطلب جدید برای HelpDev"
            : "پیش‌نمایش و ویرایش محتوای موجود"
        }
        secondaryActions={
          <div className="flex items-center gap-2">
            {mode === "create" ? (
              <Link
                href={ADMIN_ROUTES.contentWorkflows}
                className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
              >
                ایجاد با AI
              </Link>
            ) : null}
            <Link
              href={ADMIN_ROUTES.content}
              className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
            >
              <AdminIcon name="chevron" size={16} />
              بازگشت
            </Link>
          </div>
        }
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <ContentForm
          values={values}
          errors={errors}
          disabled={create.submitting}
          onChange={onChange}
          onRegenerateSlug={onRegenerateSlug}
        />
        <div className="space-y-4">
          <ContentPreviewPanel values={values} />
          <PublishPanel
            status={mode === "edit" ? (initial?.status ?? "Draft") : values.status}
            submitting={create.submitting}
            canMutate={canMutate}
            disabledReason={disabledReason}
            error={create.error}
            onSaveDraft={() => void submit("Draft")}
            onPublish={() => void submit("Published")}
          />
        </div>
      </div>
    </div>
  );
}
