"use client";

import { useState } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import type { RoadmapStepDto, UpsertRoadmapResourceItemDto, UpsertRoadmapTopicItemDto } from "@/lib/api/content";
import { TopicEditor } from "@/components/admin/content/workspaces/roadmap/topic-editor";
import { ResourcePicker } from "@/components/admin/content/workspaces/roadmap/resource-picker";

export type StepDraft = {
  title: string;
  description: string | null;
  order: number;
  estimatedHours: number;
  projectTitle: string | null;
  projectDescription: string | null;
  topics: UpsertRoadmapTopicItemDto[];
  resources: UpsertRoadmapResourceItemDto[];
};

type StepBuilderProps = {
  steps: RoadmapStepDto[];
  disabled?: boolean;
  onAdd: (draft: StepDraft) => Promise<void>;
  onUpdate: (stepId: string, draft: StepDraft) => Promise<void>;
  onRemove: (stepId: string) => Promise<void>;
};

const EMPTY_DRAFT: StepDraft = {
  title: "",
  description: null,
  order: 0,
  estimatedHours: 0,
  projectTitle: null,
  projectDescription: null,
  topics: [],
  resources: [],
};

export function StepBuilder({ steps, disabled, onAdd, onUpdate, onRemove }: StepBuilderProps) {
  const [draft, setDraft] = useState<StepDraft>(EMPTY_DRAFT);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const beginEdit = (step: RoadmapStepDto) => {
    setEditingId(step.id);
    setDraft({
      title: step.title,
      description: step.description,
      order: step.order,
      estimatedHours: step.estimatedHours,
      projectTitle: step.projectTitle,
      projectDescription: step.projectDescription,
      topics: step.topics.map((t) => ({
        id: t.id,
        title: t.title,
        description: t.description,
        order: t.order,
      })),
      resources: step.resources.map((r) => ({
        id: r.id,
        title: r.title,
        url: r.url,
        resourceType: r.resourceType,
        order: r.order,
      })),
    });
    setError(null);
  };

  const reset = () => {
    setEditingId(null);
    setDraft(EMPTY_DRAFT);
    setError(null);
  };

  const submit = async () => {
    if (!draft.title.trim()) {
      setError("عنوان فاز الزامی است.");
      return;
    }
    setBusy(true);
    setError(null);
    try {
      if (editingId) await onUpdate(editingId, draft);
      else await onAdd(draft);
      reset();
    } catch {
      setError("ذخیره فاز ناموفق بود.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="space-y-4">
      <AdminSurface className="space-y-3 p-4">
        <div className="flex items-center justify-between gap-2">
          <h2 className="adm-text text-[14px] font-bold">فازها (Steps)</h2>
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus text-[11px]"
            disabled={disabled || busy}
            onClick={reset}
          >
            + فاز جدید
          </button>
        </div>

        {steps.length === 0 ? (
          <p className="adm-muted text-[12px]">هنوز فازی ثبت نشده است.</p>
        ) : (
          <ul className="space-y-2">
            {[...steps]
              .sort((a, b) => a.order - b.order)
              .map((step, index) => (
                <li
                  key={step.id}
                  className="flex flex-wrap items-start justify-between gap-2 rounded-lg border border-[var(--adm-border)] px-3 py-2"
                >
                  <div>
                    <p className="adm-text text-[12px] font-semibold">
                      فاز {index + 1}: {step.title}
                    </p>
                    <p className="adm-muted text-[11px]">
                      {step.topics.length} موضوع · {step.resources.length} منبع · {step.estimatedHours}h
                    </p>
                  </div>
                  <div className="flex gap-2">
                    <button
                      type="button"
                      className="adm-btn adm-btn-outline adm-focus text-[11px]"
                      disabled={disabled || busy}
                      onClick={() => beginEdit(step)}
                    >
                      ویرایش
                    </button>
                    <button
                      type="button"
                      className="adm-btn adm-btn-outline adm-focus text-[11px]"
                      disabled={disabled || busy}
                      onClick={() => void onRemove(step.id)}
                    >
                      حذف
                    </button>
                  </div>
                </li>
              ))}
          </ul>
        )}
      </AdminSurface>

      <AdminSurface className="space-y-3 p-4">
        <h3 className="adm-text text-[13px] font-bold">
          {editingId ? "ویرایش فاز" : "افزودن فاز"}
        </h3>
        <input
          className="adm-input"
          placeholder="عنوان فاز (مثلاً JavaScript)"
          value={draft.title}
          disabled={disabled || busy}
          onChange={(e) => setDraft((prev) => ({ ...prev, title: e.target.value }))}
        />
        <textarea
          className="adm-input min-h-[64px]"
          placeholder="توضیح"
          value={draft.description ?? ""}
          disabled={disabled || busy}
          onChange={(e) =>
            setDraft((prev) => ({ ...prev, description: e.target.value.trim() || null }))
          }
        />
        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">ساعت تخمینی</span>
          <input
            className="adm-input"
            type="number"
            min={0}
            value={draft.estimatedHours}
            disabled={disabled || busy}
            onChange={(e) =>
              setDraft((prev) => ({ ...prev, estimatedHours: Number(e.target.value) || 0 }))
            }
          />
        </label>
        <input
          className="adm-input"
          placeholder="عنوان پروژه (اختیاری)"
          value={draft.projectTitle ?? ""}
          disabled={disabled || busy}
          onChange={(e) =>
            setDraft((prev) => ({ ...prev, projectTitle: e.target.value.trim() || null }))
          }
        />
        <textarea
          className="adm-input min-h-[56px]"
          placeholder="توضیح پروژه"
          value={draft.projectDescription ?? ""}
          disabled={disabled || busy}
          onChange={(e) =>
            setDraft((prev) => ({
              ...prev,
              projectDescription: e.target.value.trim() || null,
            }))
          }
        />

        <TopicEditor
          topics={draft.topics}
          disabled={disabled || busy}
          onChange={(topics) => setDraft((prev) => ({ ...prev, topics }))}
        />
        <ResourcePicker
          resources={draft.resources}
          disabled={disabled || busy}
          onChange={(resources) => setDraft((prev) => ({ ...prev, resources }))}
        />

        {error ? <p className="text-[12px] text-[var(--adm-danger)]">{error}</p> : null}
        <button
          type="button"
          className="adm-btn adm-btn-primary adm-focus"
          disabled={disabled || busy}
          onClick={() => void submit()}
        >
          {editingId ? "ذخیره فاز" : "افزودن فاز"}
        </button>
      </AdminSurface>
    </div>
  );
}
