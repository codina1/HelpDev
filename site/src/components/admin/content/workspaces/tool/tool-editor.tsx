"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { adminContentItemRoute, adminContentToolRoute } from "@/lib/admin/routes";
import {
  addToolFeature,
  getToolMetadata,
  removeToolFeature,
  suggestToolFeatures,
  suggestToolSummary,
  updateToolMetadata,
  type ToolAiSuggestionDto,
  type ToolDetailDto,
  type ToolFeatureDto,
} from "@/lib/api/content";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { ContentStudio } from "@/components/admin/content/editor/content-studio";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { ToolFeaturesEditor } from "@/components/admin/content/workspaces/tool/tool-features-editor";
import { ToolAlternativesEditor } from "@/components/admin/content/workspaces/tool/tool-alternatives-editor";
import { ToolPreview } from "@/components/admin/content/workspaces/tool/tool-preview";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";
import {
  EMPTY_TOOL_FORM,
  TOOL_LICENSES,
  TOOL_PLATFORMS,
  TOOL_PRICING,
  type ToolFormState,
} from "@/components/admin/content/workspaces/tool/tool-form-types";

const workspace = getWorkspaceByKey("tool");
const TOOL_PRICING_LABELS: Record<ToolFormState["pricingModel"], string> = {
  Free: "رایگان",
  Freemium: "رایگان با امکانات پولی",
  Paid: "پولی",
  OpenSource: "متن‌باز",
};
const TOOL_LICENSE_LABELS: Record<ToolFormState["licenseType"], string> = {
  Commercial: "تجاری",
  OpenSource: "متن‌باز",
  Community: "جامعه‌محور",
};
const TOOL_PLATFORM_LABELS: Record<(typeof TOOL_PLATFORMS)[number], string> = {
  Windows: "ویندوز",
  Linux: "لینوکس",
  MacOS: "مک",
  Web: "وب",
};

export type { ToolFormState };

export function ToolList() {
  return <ContentWorkspaceList workspace={workspace} />;
}

/** Create tool: Content (type=Tool) + Tool Metadata upsert. */
export function ToolEditor() {
  const { token } = useAuth();
  const [tool, setTool] = useState<ToolFormState>(EMPTY_TOOL_FORM);
  const [toolError, setToolError] = useState<string | null>(null);

  const createExtension = useMemo(
    () => ({
      panel: (
        <AdminSurface className="space-y-4 p-4">
          <ToolCatalogFields tool={tool} onChange={setTool} error={toolError} />
        </AdminSurface>
      ),
      validate: () => {
        if (
          !tool.toolName.trim() ||
          !tool.officialWebsiteUrl.trim() ||
          !tool.toolCategory.trim()
        ) {
          setToolError("نام، وب‌سایت و دسته ابزار الزامی است.");
          return false;
        }
        setToolError(null);
        return true;
      },
      persist: async (contentId: string) => {
        if (!token) {
          throw new Error("برای ذخیره تنظیمات ابزار باید وارد شوید.");
        }
        await updateToolMetadata(token, contentId, {
          toolName: tool.toolName.trim(),
          officialWebsiteUrl: tool.officialWebsiteUrl.trim(),
          githubUrl: tool.githubUrl.trim() || null,
          logoMediaId: null,
          companyName: tool.companyName.trim() || null,
          pricingModel: tool.pricingModel,
          toolCategory: tool.toolCategory.trim(),
          platforms: tool.platforms,
          licenseType: tool.licenseType,
          alternatives: tool.alternatives,
        });
      },
      successPath: adminContentToolRoute,
    }),
    [tool, toolError, token],
  );

  return <ContentStudio createType="Tool" createExtension={createExtension} />;
}

/** Edit existing tool metadata + features on /admin/content/tools/[id]. */
export function ToolWorkspaceDetail({ contentId }: { contentId: string }) {
  const { token } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [detail, setDetail] = useState<ToolDetailDto | null>(null);
  const [tool, setTool] = useState<ToolFormState>(EMPTY_TOOL_FORM);
  const [features, setFeatures] = useState<ToolFeatureDto[]>([]);
  const [saving, setSaving] = useState(false);
  const [ai, setAi] = useState<ToolAiSuggestionDto | null>(null);

  const reload = useCallback(() => {
    if (!token) return;
    setLoading(true);
    getToolMetadata(token, contentId)
      .then((dto) => {
        setDetail(dto);
        if (dto) {
          setTool({
            toolName: dto.toolName,
            officialWebsiteUrl: dto.officialWebsiteUrl,
            githubUrl: dto.githubUrl ?? "",
            companyName: dto.companyName ?? "",
            pricingModel: (TOOL_PRICING.includes(dto.pricingModel as (typeof TOOL_PRICING)[number])
              ? dto.pricingModel
              : "Freemium") as ToolFormState["pricingModel"],
            toolCategory: dto.toolCategory,
            platforms: dto.platforms,
            licenseType: (TOOL_LICENSES.includes(dto.licenseType as (typeof TOOL_LICENSES)[number])
              ? dto.licenseType
              : "Commercial") as ToolFormState["licenseType"],
            alternatives: dto.alternatives.map((a) => ({
              alternativeToolContentId: a.alternativeToolContentId,
              order: a.order,
            })),
          });
          setFeatures(dto.features);
        }
        setLoading(false);
      })
      .catch((err) => {
        setError(err);
        setLoading(false);
      });
  }, [token, contentId]);

  useEffect(() => {
    reload();
  }, [reload]);

  const save = useCallback(async () => {
    if (!token) return;
    setSaving(true);
    try {
      const saved = await updateToolMetadata(token, contentId, {
        toolName: tool.toolName.trim(),
        officialWebsiteUrl: tool.officialWebsiteUrl.trim(),
        githubUrl: tool.githubUrl.trim() || null,
        logoMediaId: null,
        companyName: tool.companyName.trim() || null,
        pricingModel: tool.pricingModel,
        toolCategory: tool.toolCategory.trim(),
        platforms: tool.platforms,
        licenseType: tool.licenseType,
        alternatives: tool.alternatives,
      });
      setDetail(saved);
      setFeatures(saved.features);
    } catch (err) {
      setError(err);
    } finally {
      setSaving(false);
    }
  }, [token, contentId, tool]);

  const preview = useMemo(
    () => ({ ...tool, features }),
    [tool, features],
  );

  if (loading) return <AdminLoadingState cards={0} rows={6} />;
  if (error) return <AdminErrorState error={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <WorkspaceHeader
        workspace={workspace}
        showCreate={false}
        secondaryActions={
          <div className="flex flex-wrap gap-2">
            <Link href={adminContentItemRoute(contentId) + "/edit"} className="adm-btn adm-btn-outline adm-focus">
              استودیوی محتوا / SEO
            </Link>
            <Link href={workspace.listHref} className="adm-btn adm-btn-outline adm-focus">
              فهرست
            </Link>
          </div>
        }
        primaryAction={
          <button type="button" className="adm-btn adm-btn-primary adm-focus" disabled={saving} onClick={() => void save()}>
            ذخیره متادیتای ابزار
          </button>
        }
      />

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <div className="space-y-4">
          <AdminSurface className="space-y-4 p-4">
            <ToolCatalogFields tool={tool} onChange={setTool} />
          </AdminSurface>
          <ToolFeaturesEditor
            features={features}
            disabled={saving || !token}
            onAdd={async (title, description) => {
              if (!token) return;
              const created = await addToolFeature(token, contentId, {
                title,
                description,
                order: null,
              });
              setFeatures((prev) => [...prev, created]);
            }}
            onRemove={async (featureId) => {
              if (!token) return;
              await removeToolFeature(token, contentId, featureId);
              setFeatures((prev) => prev.filter((f) => f.id !== featureId));
            }}
          />
          <ToolAlternativesEditor
            items={tool.alternatives}
            onChange={(alternatives) => setTool((prev) => ({ ...prev, alternatives }))}
          />
          <AdminSurface className="space-y-3 p-4">
            <h3 className="adm-text text-[13px] font-bold">پیشنهاد AI (فقط انسانی)</h3>
            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus"
                onClick={() =>
                  token &&
                  void suggestToolSummary(token, contentId).then(setAi).catch(setError)
                }
              >
                پیشنهاد خلاصه
              </button>
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus"
                onClick={() =>
                  token &&
                  void suggestToolFeatures(token, contentId).then(setAi).catch(setError)
                }
              >
                پیشنهاد ویژگی‌ها
              </button>
            </div>
            {ai ? (
              <div className="space-y-2 rounded-lg border border-[var(--adm-border)] p-3 text-[12px]">
                <p className="font-bold">{ai.title}</p>
                <p className="adm-muted whitespace-pre-wrap">{ai.body}</p>
                <ul className="list-disc pr-5">
                  {ai.bulletSuggestions.map((b) => (
                    <li key={b}>{b}</li>
                  ))}
                </ul>
                <p className="adm-subtle">اعمال خودکار غیرفعال است — کپی دستی.</p>
              </div>
            ) : null}
          </AdminSurface>
        </div>
        <ToolPreview
          tool={preview}
          title={detail?.toolName ?? tool.toolName}
          body=""
          features={features}
        />
      </div>
    </div>
  );
}

function ToolCatalogFields({
  tool,
  onChange,
  error,
}: {
  tool: ToolFormState;
  onChange: (next: ToolFormState) => void;
  error?: string | null;
}) {
  const patch = (partial: Partial<ToolFormState>) => onChange({ ...tool, ...partial });
  return (
    <div className="space-y-3">
      <h2 className="adm-text text-[14px] font-bold">کاتالوگ ابزار</h2>
      {error ? <p className="text-[12px] text-[var(--adm-danger)]">{error}</p> : null}
      <Field label="نام ابزار" value={tool.toolName} onChange={(v) => patch({ toolName: v })} />
      <Field label="وب‌سایت رسمی" value={tool.officialWebsiteUrl} onChange={(v) => patch({ officialWebsiteUrl: v })} dir="ltr" />
      <Field label="GitHub" value={tool.githubUrl} onChange={(v) => patch({ githubUrl: v })} dir="ltr" />
      <Field label="شرکت" value={tool.companyName} onChange={(v) => patch({ companyName: v })} />
      <Field label="دسته" value={tool.toolCategory} onChange={(v) => patch({ toolCategory: v })} />
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">قیمت‌گذاری</span>
        <select
          className="adm-input"
          value={tool.pricingModel}
          onChange={(e) => patch({ pricingModel: e.target.value as ToolFormState["pricingModel"] })}
        >
          {TOOL_PRICING.map((p) => (
            <option key={p} value={p}>
              {TOOL_PRICING_LABELS[p]}
            </option>
          ))}
        </select>
      </label>
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">لایسنس</span>
        <select
          className="adm-input"
          value={tool.licenseType}
          onChange={(e) => patch({ licenseType: e.target.value as ToolFormState["licenseType"] })}
        >
          {TOOL_LICENSES.map((p) => (
            <option key={p} value={p}>
              {TOOL_LICENSE_LABELS[p]}
            </option>
          ))}
        </select>
      </label>
      <fieldset className="space-y-2">
        <legend className="adm-text text-[12px] font-semibold">پلتفرم‌ها</legend>
        <div className="flex flex-wrap gap-3">
          {TOOL_PLATFORMS.map((platform) => {
            const checked = tool.platforms.includes(platform);
            return (
              <label key={platform} className="inline-flex items-center gap-2 text-[12px]">
                <input
                  type="checkbox"
                  checked={checked}
                  onChange={() =>
                    patch({
                      platforms: checked
                        ? tool.platforms.filter((p) => p !== platform)
                        : [...tool.platforms, platform],
                    })
                  }
                />
                {TOOL_PLATFORM_LABELS[platform]}
              </label>
            );
          })}
        </div>
      </fieldset>
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
  dir,
}: {
  label: string;
  value: string;
  onChange: (v: string) => void;
  dir?: "ltr" | "rtl";
}) {
  return (
    <label className="block space-y-1.5">
      <span className="adm-text text-[12px] font-semibold">{label}</span>
      <input className="adm-input" dir={dir} value={value} onChange={(e) => onChange(e.target.value)} />
    </label>
  );
}
