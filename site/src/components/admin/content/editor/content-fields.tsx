"use client";

import type { ReactNode } from "react";
import { CONTENT_TYPES } from "@/lib/admin/content/content-types";
import type {
  ContentFormErrors,
  ContentFormValues,
  ContentTypeValue,
} from "@/lib/admin/content/content-types";
import { labelForContentType } from "@/lib/admin/content/content-mappers";

type ContentFieldsProps = {
  values: ContentFormValues;
  errors: ContentFormErrors;
  disabled?: boolean;
  /** When set, type is fixed for the workspace — no selector. */
  lockedType?: ContentTypeValue;
  onChange: (patch: Partial<ContentFormValues>) => void;
  onRegenerateSlug?: () => void;
  /** Extra fields below body (workspace-specific foundation UI). */
  afterFields?: ReactNode;
};

/** The content editor's field set (Title, Slug, Type, Body). */
export function ContentFields({
  values,
  errors,
  disabled = false,
  lockedType,
  onChange,
  onRegenerateSlug,
  afterFields,
}: ContentFieldsProps) {
  return (
    <div className="space-y-4">
      <Field id="content-title" label="عنوان" error={errors.title} required>
        <input
          id="content-title"
          type="text"
          className="adm-input"
          value={values.title}
          disabled={disabled}
          maxLength={200}
          onChange={(event) => onChange({ title: event.target.value })}
          aria-invalid={Boolean(errors.title)}
        />
      </Field>

      <Field
        id="content-slug"
        label="اسلاگ"
        error={errors.slug}
        required
        hint="فقط حروف کوچک انگلیسی، اعداد و خط تیره (مثال: my-first-article)."
        action={
          onRegenerateSlug ? (
            <button
              type="button"
              onClick={onRegenerateSlug}
              disabled={disabled}
              className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]"
            >
              تولید از عنوان
            </button>
          ) : undefined
        }
      >
        <input
          id="content-slug"
          type="text"
          dir="ltr"
          className="adm-input text-start"
          value={values.slug}
          disabled={disabled}
          maxLength={300}
          onChange={(event) => onChange({ slug: event.target.value })}
          aria-invalid={Boolean(errors.slug)}
        />
      </Field>

      {lockedType ? (
        <Field id="content-type" label="نوع محتوا" hint="توسط فضای کار تعیین شده است.">
          <input
            id="content-type"
            type="text"
            className="adm-input"
            value={labelForContentType(lockedType)}
            disabled
            readOnly
          />
        </Field>
      ) : (
        <Field id="content-type" label="نوع محتوا" error={errors.type} required>
          <select
            id="content-type"
            className="adm-input"
            value={values.type}
            disabled={disabled}
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
        </Field>
      )}

      <Field
        id="content-body"
        label="متن محتوا"
        error={errors.body}
        required
        hint="از Markdown پشتیبانی می‌شود: عنوان‌ها، لیست‌ها، کد، پیوند و تأکید."
      >
        <textarea
          id="content-body"
          className="adm-input min-h-[220px] font-mono text-[13px] leading-6"
          value={values.body}
          disabled={disabled}
          onChange={(event) => onChange({ body: event.target.value })}
          aria-invalid={Boolean(errors.body)}
        />
      </Field>

      {afterFields}
    </div>
  );
}

function Field({
  id,
  label,
  error,
  required,
  hint,
  action,
  children,
}: {
  id: string;
  label: string;
  error?: string;
  required?: boolean;
  hint?: string;
  action?: ReactNode;
  children: ReactNode;
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
      {hint ? <p className="adm-subtle text-[11px] leading-5">{hint}</p> : null}
      {error ? (
        <p className="text-[11px] text-[var(--adm-danger)]" role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
}
