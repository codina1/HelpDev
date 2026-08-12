"use client";

import { AdminSurface } from "@/components/admin/page/admin-surface";
import type { RoadmapStepDto } from "@/lib/api/content";

type DragDropOrderingProps = {
  steps: RoadmapStepDto[];
  disabled?: boolean;
  onReorder: (orderedIds: string[]) => Promise<void>;
};

/** Lightweight reorder controls (move up/down) — avoids heavy DnD libs. */
export function DragDropOrdering({ steps, disabled, onReorder }: DragDropOrderingProps) {
  const ordered = [...steps].sort((a, b) => a.order - b.order);

  const move = async (index: number, direction: -1 | 1) => {
    const target = index + direction;
    if (target < 0 || target >= ordered.length) return;
    const next = [...ordered];
    const [item] = next.splice(index, 1);
    next.splice(target, 0, item);
    await onReorder(next.map((s) => s.id));
  };

  if (ordered.length < 2) return null;

  return (
    <AdminSurface className="space-y-3 p-4">
      <h3 className="adm-text text-[13px] font-bold">ترتیب فازها</h3>
      <ul className="space-y-2">
        {ordered.map((step, index) => (
          <li
            key={step.id}
            className="flex items-center justify-between gap-2 rounded-lg border border-[var(--adm-border)] px-3 py-2"
          >
            <span className="adm-text text-[12px]">
              {index + 1}. {step.title}
            </span>
            <div className="flex gap-1">
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus text-[11px]"
                disabled={disabled || index === 0}
                onClick={() => void move(index, -1)}
              >
                ↑
              </button>
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus text-[11px]"
                disabled={disabled || index === ordered.length - 1}
                onClick={() => void move(index, 1)}
              >
                ↓
              </button>
            </div>
          </li>
        ))}
      </ul>
    </AdminSurface>
  );
}
