"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { adminContentItemRoute, adminContentRoadmapRoute } from "@/lib/admin/routes";
import {
  addRoadmapStep,
  getRoadmapMetadata,
  removeRoadmapStep,
  reorderRoadmapSteps,
  suggestRoadmapOutline,
  updateRoadmapMetadata,
  updateRoadmapStep,
  type RoadmapAiSuggestionDto,
  type RoadmapDetailDto,
  type RoadmapStepDto,
} from "@/lib/api/content";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";
import { ContentStudio } from "@/components/admin/content/editor/content-studio";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";
import { StepBuilder } from "@/components/admin/content/workspaces/roadmap/step-builder";
import { DragDropOrdering } from "@/components/admin/content/workspaces/roadmap/drag-drop-ordering";

const workspace = getWorkspaceByKey("roadmap");
const LEVELS = ["Beginner", "Intermediate", "Advanced"] as const;
const LEVEL_LABELS: Record<(typeof LEVELS)[number], string> = {
  Beginner: "مبتدی",
  Intermediate: "متوسط",
  Advanced: "پیشرفته",
};

export type RoadmapMetaForm = {
  level: (typeof LEVELS)[number];
  estimatedDuration: string;
  goal: string;
  prerequisites: string;
};

const EMPTY_META: RoadmapMetaForm = {
  level: "Beginner",
  estimatedDuration: "",
  goal: "",
  prerequisites: "",
};

export function RoadmapList() {
  return <ContentWorkspaceList workspace={workspace} />;
}

/** Create roadmap: Content (type=Roadmap) + metadata upsert. */
export function RoadmapEditor() {
  const { token } = useAuth();
  const [meta, setMeta] = useState<RoadmapMetaForm>(EMPTY_META);
  const [metaError, setMetaError] = useState<string | null>(null);

  const createExtension = useMemo(
    () => ({
      panel: (
        <AdminSurface className="space-y-4 p-4">
          <RoadmapMetaFields meta={meta} onChange={setMeta} error={metaError} />
        </AdminSurface>
      ),
      validate: () => {
        if (!meta.goal.trim() || !meta.estimatedDuration.trim()) {
          setMetaError("هدف و مدت تخمینی الزامی است.");
          return false;
        }
        setMetaError(null);
        return true;
      },
      persist: async (contentId: string) => {
        if (!token) {
          throw new Error("برای ذخیره مشخصات نقشه راه باید وارد شوید.");
        }
        await updateRoadmapMetadata(token, contentId, {
          level: meta.level,
          estimatedDuration: meta.estimatedDuration.trim(),
          goal: meta.goal.trim(),
          prerequisites: meta.prerequisites.trim() || null,
        });
      },
      successPath: adminContentRoadmapRoute,
    }),
    [meta, metaError, token],
  );

  return <ContentStudio createType="Roadmap" createExtension={createExtension} />;
}

/** Edit roadmap builder on /admin/content/roadmaps/[id]. */
export function RoadmapBuilderDetail({ contentId }: { contentId: string }) {
  const { token } = useAuth();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [detail, setDetail] = useState<RoadmapDetailDto | null>(null);
  const [meta, setMeta] = useState<RoadmapMetaForm>(EMPTY_META);
  const [steps, setSteps] = useState<RoadmapStepDto[]>([]);
  const [saving, setSaving] = useState(false);
  const [ai, setAi] = useState<RoadmapAiSuggestionDto | null>(null);

  const reload = useCallback(() => {
    if (!token) return;
    setLoading(true);
    getRoadmapMetadata(token, contentId)
      .then((dto) => {
        setDetail(dto);
        if (dto) {
          setMeta({
            level: (LEVELS.includes(dto.level as (typeof LEVELS)[number])
              ? dto.level
              : "Beginner") as RoadmapMetaForm["level"],
            estimatedDuration: dto.estimatedDuration,
            goal: dto.goal,
            prerequisites: dto.prerequisites ?? "",
          });
          setSteps([...dto.steps].sort((a, b) => a.order - b.order));
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

  const saveMeta = useCallback(async () => {
    if (!token) return;
    setSaving(true);
    try {
      const saved = await updateRoadmapMetadata(token, contentId, {
        level: meta.level,
        estimatedDuration: meta.estimatedDuration.trim(),
        goal: meta.goal.trim(),
        prerequisites: meta.prerequisites.trim() || null,
      });
      setDetail(saved);
      setSteps([...saved.steps].sort((a, b) => a.order - b.order));
    } catch (err) {
      setError(err);
    } finally {
      setSaving(false);
    }
  }, [token, contentId, meta]);

  if (loading) return <AdminLoadingState cards={0} rows={6} />;
  if (error && !detail) return <AdminErrorState error={error} onRetry={reload} />;

  return (
    <div className="space-y-6">
      <WorkspaceHeader
        workspace={workspace}
        showCreate={false}
        secondaryActions={
          <div className="flex flex-wrap gap-2">
            <Link href={`${adminContentItemRoute(contentId)}/edit`} className="adm-btn adm-btn-outline adm-focus">
              استودیوی محتوا / SEO
            </Link>
            <Link href={workspace.listHref} className="adm-btn adm-btn-outline adm-focus">
              فهرست
            </Link>
          </div>
        }
        primaryAction={
          <button
            type="button"
            className="adm-btn adm-btn-primary adm-focus"
            disabled={saving}
            onClick={() => void saveMeta()}
          >
            ذخیره مشخصات
          </button>
        }
      />

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <AdminSurface className="space-y-4 p-4">
          <RoadmapMetaFields meta={meta} onChange={setMeta} />
        </AdminSurface>

        <AdminSurface className="space-y-3 p-4">
          <h3 className="adm-text text-[13px] font-bold">پیشنهاد AI (فقط انسانی)</h3>
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus"
            onClick={() =>
              token && void suggestRoadmapOutline(token, contentId).then(setAi).catch(setError)
            }
          >
            پیشنهاد ساختار کلی
          </button>
          {ai ? (
            <div className="space-y-2 rounded-lg border border-[var(--adm-border)] p-3 text-[12px]">
              <p className="font-bold">{ai.title}</p>
              <p className="adm-muted whitespace-pre-wrap">{ai.body}</p>
              <ul className="list-disc pr-5">
                {ai.bulletSuggestions.map((b) => (
                  <li key={b}>{b}</li>
                ))}
              </ul>
              <p className="adm-subtle">اعمال خودکار غیرفعال است.</p>
            </div>
          ) : null}
        </AdminSurface>
      </div>

      <DragDropOrdering
        steps={steps}
        disabled={saving || !token}
        onReorder={async (orderedIds) => {
          if (!token) return;
          await reorderRoadmapSteps(token, contentId, orderedIds);
          setSteps((prev) =>
            orderedIds
              .map((id, order) => {
                const step = prev.find((s) => s.id === id);
                return step ? { ...step, order } : null;
              })
              .filter((s): s is RoadmapStepDto => s != null),
          );
        }}
      />

      <StepBuilder
        steps={steps}
        disabled={saving || !token}
        onAdd={async (draft) => {
          if (!token) return;
          const created = await addRoadmapStep(token, contentId, {
            title: draft.title,
            description: draft.description,
            order: null,
            estimatedHours: draft.estimatedHours,
            projectTitle: draft.projectTitle,
            projectDescription: draft.projectDescription,
            topics: draft.topics,
            resources: draft.resources,
          });
          setSteps((prev) => [...prev, created].sort((a, b) => a.order - b.order));
        }}
        onUpdate={async (stepId, draft) => {
          if (!token) return;
          const updated = await updateRoadmapStep(token, contentId, stepId, {
            title: draft.title,
            description: draft.description,
            order: draft.order,
            estimatedHours: draft.estimatedHours,
            projectTitle: draft.projectTitle,
            projectDescription: draft.projectDescription,
            topics: draft.topics,
            resources: draft.resources,
          });
          setSteps((prev) => prev.map((s) => (s.id === stepId ? updated : s)));
        }}
        onRemove={async (stepId) => {
          if (!token) return;
          await removeRoadmapStep(token, contentId, stepId);
          setSteps((prev) => prev.filter((s) => s.id !== stepId));
        }}
      />
    </div>
  );
}

function RoadmapMetaFields({
  meta,
  onChange,
  error,
}: {
  meta: RoadmapMetaForm;
  onChange: (next: RoadmapMetaForm) => void;
  error?: string | null;
}) {
  const patch = (partial: Partial<RoadmapMetaForm>) => onChange({ ...meta, ...partial });
  return (
    <div className="space-y-3">
      <h2 className="adm-text text-[14px] font-bold">مشخصات نقشه راه</h2>
      {error ? <p className="text-[12px] text-[var(--adm-danger)]">{error}</p> : null}
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">سطح</span>
        <select
          className="adm-input"
          value={meta.level}
          onChange={(e) => patch({ level: e.target.value as RoadmapMetaForm["level"] })}
        >
          {LEVELS.map((level) => (
            <option key={level} value={level}>
              {LEVEL_LABELS[level]}
            </option>
          ))}
        </select>
      </label>
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">مدت تخمینی</span>
        <input
          className="adm-input"
          value={meta.estimatedDuration}
          onChange={(e) => patch({ estimatedDuration: e.target.value })}
          placeholder="مثلاً 12 weeks"
        />
      </label>
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">هدف</span>
        <textarea
          className="adm-input min-h-[72px]"
          value={meta.goal}
          onChange={(e) => patch({ goal: e.target.value })}
        />
      </label>
      <label className="block space-y-1.5">
        <span className="adm-text text-[12px] font-semibold">پیش‌نیازها</span>
        <textarea
          className="adm-input min-h-[64px]"
          value={meta.prerequisites}
          onChange={(e) => patch({ prerequisites: e.target.value })}
        />
      </label>
    </div>
  );
}
