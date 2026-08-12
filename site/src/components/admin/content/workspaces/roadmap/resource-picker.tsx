"use client";

import type { UpsertRoadmapResourceItemDto } from "@/lib/api/content";

const RESOURCE_TYPES = ["Article", "Tool", "Course", "Video", "External"] as const;

type ResourcePickerProps = {
  resources: UpsertRoadmapResourceItemDto[];
  onChange: (resources: UpsertRoadmapResourceItemDto[]) => void;
  disabled?: boolean;
};

/** Resource links by identifier only — content:/tool:/course: or https URL. No cross-module FK. */
export function ResourcePicker({ resources, onChange, disabled }: ResourcePickerProps) {
  const add = () => {
    onChange([
      ...resources,
      { title: "", url: "", resourceType: "External", order: resources.length },
    ]);
  };

  const update = (index: number, patch: Partial<UpsertRoadmapResourceItemDto>) => {
    onChange(resources.map((r, i) => (i === index ? { ...r, ...patch } : r)));
  };

  const remove = (index: number) => {
    onChange(resources.filter((_, i) => i !== index).map((r, order) => ({ ...r, order })));
  };

  return (
    <div className="space-y-2 rounded-lg border border-[var(--adm-border)] p-3">
      <div className="flex items-center justify-between">
        <h4 className="adm-text text-[12px] font-bold">منابع</h4>
        <button
          type="button"
          className="adm-btn adm-btn-ghost adm-focus text-[11px]"
          disabled={disabled}
          onClick={add}
        >
          افزودن منبع
        </button>
      </div>
      <p className="adm-muted text-[11px]">
        URL یا شناسه: <code dir="ltr">content:</code> / <code dir="ltr">tool:</code> /{" "}
        <code dir="ltr">course:</code>
      </p>
      {resources.length === 0 ? (
        <p className="adm-muted text-[11px]">منبعی ثبت نشده.</p>
      ) : (
        <ul className="space-y-2">
          {resources.map((resource, index) => (
            <li key={`${resource.id ?? "new"}-${index}`} className="space-y-2 rounded-md bg-[var(--adm-surface-2)] p-2">
              <input
                className="adm-input"
                placeholder="عنوان منبع"
                value={resource.title}
                disabled={disabled}
                onChange={(e) => update(index, { title: e.target.value })}
              />
              <input
                className="adm-input font-mono text-[12px]"
                dir="ltr"
                placeholder="https://… or content:guid"
                value={resource.url}
                disabled={disabled}
                onChange={(e) => update(index, { url: e.target.value })}
              />
              <div className="flex gap-2">
                <select
                  className="adm-input flex-1"
                  value={resource.resourceType}
                  disabled={disabled}
                  onChange={(e) => update(index, { resourceType: e.target.value })}
                >
                  {RESOURCE_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
                <button
                  type="button"
                  className="adm-btn adm-btn-outline adm-focus text-[11px]"
                  disabled={disabled}
                  onClick={() => remove(index)}
                >
                  حذف
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
