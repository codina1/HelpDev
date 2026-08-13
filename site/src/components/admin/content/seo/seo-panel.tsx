"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { SeoField } from "@/components/admin/content/seo/seo-field";
import { SeoGooglePreview } from "@/components/admin/content/seo/seo-google-preview";
import { SeoSocialPreview } from "@/components/admin/content/seo/seo-social-preview";
import { SeoAnalysisPanel } from "@/components/admin/content/seo/seo-analysis-panel";
import {
  SaveStatusIndicator,
  type SaveState,
} from "@/components/admin/content/editor/save-status";
import {
  SEO_LIMITS,
  type SeoAnalysisReport,
  type SeoAnalysisStatus,
  type SeoFormErrors,
  type SeoFormValues,
} from "@/lib/admin/content/content-types";

type SeoPanelProps = {
  values: SeoFormValues;
  errors: SeoFormErrors;
  onChange: (patch: Partial<SeoFormValues>) => void;
  onSave: () => void;
  saveState: SaveState;
  error?: unknown;
  disabled?: boolean;
  // Real content context for the previews (fallbacks).
  contentTitle: string;
  excerpt: string;
  coverImage: string;
  slug: string;
  // SEO Analyzer Engine v1 — analyzes the SAVED server content only.
  analysisStatus: SeoAnalysisStatus;
  analysisReport: SeoAnalysisReport | null;
  analysisError?: unknown;
  onAnalyze: () => void;
  /** Opens the Media Library picker targeting the OG image field. */
  onPickOgImage?: () => void;
};

/**
 * SEO workspace. Owns its OWN save action (separate from the content save) that
 * calls PUT /admin/content/{id}/seo. Shows factual Google/social previews built
 * only from what the author enters — no fabricated recommendations or scores.
 */
export function SeoPanel({
  values,
  errors,
  onChange,
  onSave,
  saveState,
  error,
  disabled = false,
  contentTitle,
  excerpt,
  coverImage,
  slug,
  analysisStatus,
  analysisReport,
  analysisError,
  onAnalyze,
  onPickOgImage,
}: SeoPanelProps) {
  const hasErrors = Object.keys(errors).length > 0;

  return (
    <section className="space-y-4" aria-labelledby="seo-panel-heading">
      <div className="flex items-center justify-between gap-2">
        <h2 id="seo-panel-heading" className="adm-text inline-flex items-center gap-1.5 text-[14px] font-bold">
          <AdminIcon name="seo" size={16} />
          سئو و متادیتا
        </h2>
        <SaveStatusIndicator state={saveState} />
      </div>

      <div className="space-y-3">
        <SeoField
          id="seo-title"
          label="عنوان سئو"
          value={values.seoTitle}
          onChange={(value) => onChange({ seoTitle: value })}
          maxLength={SEO_LIMITS.seoTitle}
          hint="اگر خالی بماند، از عنوان محتوا استفاده می‌شود."
          error={errors.seoTitle}
        />
        <SeoField
          id="seo-description"
          label="توضیحات سئو"
          value={values.seoDescription}
          onChange={(value) => onChange({ seoDescription: value })}
          maxLength={SEO_LIMITS.seoDescription}
          hint="اگر خالی بماند، از خلاصه محتوا استفاده می‌شود."
          error={errors.seoDescription}
          multiline
        />
        <SeoField
          id="seo-canonical"
          label="آدرس کانونیکال"
          value={values.canonicalUrl}
          onChange={(value) => onChange({ canonicalUrl: value })}
          maxLength={SEO_LIMITS.canonicalUrl}
          placeholder="https://example.com/article"
          hint="نشانی نسخهٔ اصلی این صفحه است. اگر همین مطلب در چند آدرس دیده می‌شود، اینجا آدرس ترجیحی را بگذارید تا موتور جستجو نسخه‌های تکراری را جدا نکند."
          error={errors.canonicalUrl}
          ltr
        />
        <SeoField
          id="seo-og-image"
          label="تصویر OG"
          value={values.ogImage}
          onChange={(value) => onChange({ ogImage: value })}
          maxLength={SEO_LIMITS.ogImage}
          placeholder="https://cdn.example.com/og.png"
          hint="تصویر Open Graph همان تصویری است که هنگام اشتراک این مطلب در شبکه‌های اجتماعی مثل تلگرام، لینکدین یا ایکس نمایش داده می‌شود."
          error={errors.ogImage}
          ltr
          action={
            onPickOgImage ? (
              <button
                type="button"
                onClick={onPickOgImage}
                className="adm-btn adm-btn-ghost adm-focus inline-flex items-center gap-1 px-2 py-1 text-[11px]"
              >
                <AdminIcon name="media" size={14} />
                انتخاب از رسانه‌ها
              </button>
            ) : undefined
          }
        />
        <SeoField
          id="seo-focus-keyword"
          label="کلمه کلیدی کانونی"
          value={values.focusKeyword}
          onChange={(value) => onChange({ focusKeyword: value })}
          maxLength={SEO_LIMITS.focusKeyword}
          error={errors.focusKeyword}
        />
      </div>

      <div className="space-y-3">
        <SeoGooglePreview
          seoTitle={values.seoTitle}
          contentTitle={contentTitle}
          seoDescription={values.seoDescription}
          excerpt={excerpt}
          canonicalUrl={values.canonicalUrl}
          slug={slug}
        />
        <SeoSocialPreview
          seoTitle={values.seoTitle}
          contentTitle={contentTitle}
          seoDescription={values.seoDescription}
          excerpt={excerpt}
          ogImage={values.ogImage}
          coverImage={coverImage}
          canonicalUrl={values.canonicalUrl}
        />
      </div>

      {error ? <AdminErrorState error={error} /> : null}

      <button
        type="button"
        onClick={onSave}
        disabled={disabled || saveState === "saving" || hasErrors}
        className="adm-btn adm-btn-primary adm-focus inline-flex w-full items-center justify-center gap-1.5"
      >
        <AdminIcon name="check" size={16} />
        ذخیره سئو
      </button>

      <div className="border-t border-[var(--adm-border)] pt-4">
        <SeoAnalysisPanel
          status={analysisStatus}
          report={analysisReport}
          error={analysisError}
          onAnalyze={onAnalyze}
        />
      </div>
    </section>
  );
}
