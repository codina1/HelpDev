"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import {
  SaveStatusIndicator,
  type SaveState,
} from "@/components/admin/content/editor/save-status";
import {
  DIFFICULTY_LEVELS,
  type ArticleFormErrors,
  type ArticleFormValues,
} from "@/lib/admin/content/content-types";

type ArticleSettingsPanelProps = {
  values: ArticleFormValues;
  errors: ArticleFormErrors;
  onChange: (patch: Partial<ArticleFormValues>) => void;
  onSave: () => void;
  saveState: SaveState;
  error?: unknown;
  disabled?: boolean;
  loading?: boolean;
};

/** Article-specific settings — own save via PUT /admin/content/{id}/article. */
export function ArticleSettingsPanel({
  values,
  errors,
  onChange,
  onSave,
  saveState,
  error,
  disabled = false,
  loading = false,
}: ArticleSettingsPanelProps) {
  const hasErrors = Object.keys(errors).length > 0;

  return (
    <section className="space-y-4" aria-labelledby="article-settings-heading">
      <div className="flex items-center justify-between gap-2">
        <h2
          id="article-settings-heading"
          className="adm-text inline-flex items-center gap-1.5 text-[14px] font-bold"
        >
          <AdminIcon name="content" size={16} />
          تنظیمات مقاله
        </h2>
        <SaveStatusIndicator state={saveState} />
      </div>

      {loading ? (
        <p className="adm-subtle text-[12px]">در حال بارگذاری…</p>
      ) : (
        <div className="space-y-3">
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">دسته (CategoryId)</span>
            <input
              className="adm-input"
              dir="ltr"
              disabled={disabled}
              placeholder="GUID اختیاری — taxonomy در نسخه‌های بعدی"
              value={values.categoryId}
              onChange={(e) => onChange({ categoryId: e.target.value })}
            />
            {errors.categoryId ? (
              <p className="text-[11px] text-[var(--adm-danger)]">{errors.categoryId}</p>
            ) : null}
          </label>

          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">سطح دشواری</span>
            <select
              className="adm-input"
              disabled={disabled}
              value={values.difficultyLevel}
              onChange={(e) =>
                onChange({
                  difficultyLevel: e.target.value as ArticleFormValues["difficultyLevel"],
                })
              }
            >
              {DIFFICULTY_LEVELS.map((level) => (
                <option key={level} value={level}>
                  {level}
                </option>
              ))}
            </select>
          </label>

          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">زمان مطالعه (دقیقه)</span>
            <input
              className="adm-input"
              type="number"
              min={1}
              disabled={disabled}
              value={values.readingTimeMinutes}
              onChange={(e) => onChange({ readingTimeMinutes: e.target.value })}
            />
            {errors.readingTimeMinutes ? (
              <p className="text-[11px] text-[var(--adm-danger)]">{errors.readingTimeMinutes}</p>
            ) : null}
          </label>

          <Toggle
            label="ویژه (Featured)"
            checked={values.isFeatured}
            disabled={disabled}
            onChange={(checked) => onChange({ isFeatured: checked })}
          />
          <Toggle
            label="اجازه دیدگاه"
            checked={values.allowComments}
            disabled={disabled}
            onChange={(checked) => onChange({ allowComments: checked })}
          />
          <Toggle
            label="فهرست مطالب (TOC)"
            checked={values.tableOfContentsEnabled}
            disabled={disabled}
            onChange={(checked) => onChange({ tableOfContentsEnabled: checked })}
          />
        </div>
      )}

      {error ? <AdminErrorState error={error} /> : null}

      <button
        type="button"
        className="adm-btn adm-btn-primary adm-focus w-full"
        disabled={disabled || loading || hasErrors || saveState === "saving"}
        onClick={onSave}
      >
        ذخیره تنظیمات مقاله
      </button>
    </section>
  );
}

function Toggle({
  label,
  checked,
  disabled,
  onChange,
}: {
  label: string;
  checked: boolean;
  disabled?: boolean;
  onChange: (checked: boolean) => void;
}) {
  return (
    <label className="flex items-center justify-between gap-3 rounded-lg border border-[var(--adm-border)] px-3 py-2">
      <span className="adm-text text-[12px] font-semibold">{label}</span>
      <input
        type="checkbox"
        className="h-4 w-4"
        checked={checked}
        disabled={disabled}
        onChange={(e) => onChange(e.target.checked)}
      />
    </label>
  );
}
