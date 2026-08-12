"use client";

import { AdminIcon } from "@/components/admin/shared/admin-icons";

export type SaveState = "idle" | "unsaved" | "saving" | "saved" | "error";

const LABELS: Record<Exclude<SaveState, "idle">, string> = {
  unsaved: "تغییرات ذخیره‌نشده",
  saving: "در حال ذخیره…",
  saved: "ذخیره شد",
  error: "ذخیره ناموفق بود",
};

const TONE: Record<Exclude<SaveState, "idle">, string> = {
  unsaved: "text-[var(--adm-warning)]",
  saving: "text-[var(--adm-text-muted)]",
  saved: "text-[var(--adm-success)]",
  error: "text-[var(--adm-danger)]",
};

/**
 * A quiet, single inline status pill (no toast spam). Reflects the current save
 * lifecycle for the surrounding save action.
 */
export function SaveStatusIndicator({ state }: { state: SaveState }) {
  if (state === "idle") {
    return null;
  }

  return (
    <span
      role="status"
      aria-live="polite"
      className={`inline-flex items-center gap-1.5 text-[11px] font-semibold ${TONE[state]}`}
    >
      {state === "saving" ? (
        <span
          aria-hidden
          className="h-3 w-3 animate-spin rounded-full border-2 border-current border-t-transparent"
        />
      ) : state === "saved" ? (
        <AdminIcon name="check" size={14} />
      ) : (
        <span aria-hidden className="h-1.5 w-1.5 rounded-full bg-current" />
      )}
      {LABELS[state]}
    </span>
  );
}
