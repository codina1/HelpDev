import { labelForContentType } from "@/lib/admin/content/content-mappers";

/** Neutral chip showing a content type's Persian label. */
export function ContentTypeBadge({ type }: { type: string }) {
  return (
    <span className="inline-flex items-center rounded-md bg-[var(--adm-surface-3)] px-2 py-0.5 text-[11px] font-semibold text-[var(--adm-text-muted)]">
      {labelForContentType(type)}
    </span>
  );
}
