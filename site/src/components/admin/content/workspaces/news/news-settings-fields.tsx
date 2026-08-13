"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { JalaliDateTimePicker } from "@/components/admin/shared/jalali-date-picker";
import {
  SaveStatusIndicator,
  type SaveState,
} from "@/components/admin/content/editor/save-status";
import { labelForNewsPriority } from "@/lib/admin/content/content-mappers";
import {
  NEWS_PRIORITIES,
  type NewsFormErrors,
  type NewsFormValues,
} from "@/lib/admin/content/content-types";

type NewsSettingsPanelProps = {
  values: NewsFormValues;
  errors: NewsFormErrors;
  onChange: (patch: Partial<NewsFormValues>) => void;
  onSave?: () => void;
  saveState?: SaveState;
  error?: unknown;
  disabled?: boolean;
  /** When true, hide the save button (create form embeds fields only). */
  hideSave?: boolean;
  title?: string;
};

/** News-specific fields — saved via PUT /admin/content/{id}/news. */
export function NewsSettingsFields({
  values,
  errors,
  onChange,
  onSave,
  saveState = "idle",
  error,
  disabled = false,
  hideSave = false,
  title = "تنظیمات خبر",
}: NewsSettingsPanelProps) {
  const hasErrors = Object.keys(errors).length > 0;

  return (
    <section className="space-y-4" aria-labelledby="news-settings-heading">
      <div className="flex items-center justify-between gap-2">
        <h2
          id="news-settings-heading"
          className="adm-text inline-flex items-center gap-1.5 text-[14px] font-bold"
        >
          <AdminIcon name="news" size={16} />
          {title}
        </h2>
        {!hideSave ? <SaveStatusIndicator state={saveState} /> : null}
      </div>

      <div className="space-y-3">
        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">منبع</span>
          <input
            className="adm-input"
            disabled={disabled}
            value={values.sourceName}
            onChange={(e) => onChange({ sourceName: e.target.value })}
          />
          {errors.sourceName ? (
            <p className="text-[11px] text-[var(--adm-danger)]">{errors.sourceName}</p>
          ) : null}
        </label>

        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">آدرس منبع</span>
          <input
            className="adm-input"
            dir="ltr"
            disabled={disabled}
            placeholder="https://"
            value={values.sourceUrl}
            onChange={(e) => onChange({ sourceUrl: e.target.value })}
          />
          {errors.sourceUrl ? (
            <p className="text-[11px] text-[var(--adm-danger)]">{errors.sourceUrl}</p>
          ) : null}
        </label>

        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">اولویت</span>
          <select
            className="adm-input"
            disabled={disabled}
            value={values.priority}
            onChange={(e) =>
              onChange({ priority: e.target.value as NewsFormValues["priority"] })
            }
          >
            {NEWS_PRIORITIES.map((priority) => (
              <option key={priority} value={priority}>
                {labelForNewsPriority(priority)}
              </option>
            ))}
          </select>
        </label>

        <div className="space-y-1.5">
          <label htmlFor="news-date" className="adm-text text-[12px] font-semibold">
            تاریخ خبر
          </label>
          <JalaliDateTimePicker
            id="news-date"
            value={values.newsDateUtc}
            disabled={disabled}
            invalid={Boolean(errors.newsDateUtc)}
            onChange={(newsDateUtc) => onChange({ newsDateUtc })}
          />
          {errors.newsDateUtc ? (
            <p className="text-[11px] text-[var(--adm-danger)]">{errors.newsDateUtc}</p>
          ) : null}
        </div>

        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">ارجاع خارجی (اختیاری)</span>
          <input
            className="adm-input"
            dir="ltr"
            disabled={disabled}
            value={values.externalReference}
            onChange={(e) => onChange({ externalReference: e.target.value })}
          />
        </label>
      </div>

      {error ? <AdminErrorState error={error} /> : null}

      {!hideSave && onSave ? (
        <button
          type="button"
          className="adm-btn adm-btn-primary adm-focus w-full"
          disabled={disabled || hasErrors || saveState === "saving"}
          onClick={onSave}
        >
          ذخیره تنظیمات خبر
        </button>
      ) : null}
    </section>
  );
}
