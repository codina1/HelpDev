import { formatDateFa, formatFileSize, labelForMediaContentType } from "@/lib/admin/media/media-mappers";
import type { AdminMediaListItem } from "@/lib/admin/media/media-types";

type MediaCardProps = {
  item: AdminMediaListItem;
  onClick: (item: AdminMediaListItem) => void;
  /** Shown in the hover/focus overlay. Defaults to "مشاهده"; pickers pass "انتخاب". */
  actionLabel?: string;
};

/**
 * Single media thumbnail card. Never displays a storage key or filesystem path —
 * only the original filename, dimensions, size and upload date.
 */
export function MediaCard({ item, onClick, actionLabel = "مشاهده" }: MediaCardProps) {
  const dimensions =
    item.width && item.height ? `${item.width}×${item.height}` : null;

  return (
    <button
      type="button"
      onClick={() => onClick(item)}
      className="adm-focus adm-surface group flex flex-col overflow-hidden rounded-xl p-2 text-start transition-shadow hover:shadow-[var(--adm-shadow)]"
    >
      <span className="relative block aspect-square w-full overflow-hidden rounded-lg bg-[var(--adm-surface-2)]">
        {item.absoluteUrl ? (
          // eslint-disable-next-line @next/next/no-img-element
          <img
            src={item.absoluteUrl}
            alt={item.altText || item.originalFileName}
            loading="lazy"
            className="h-full w-full object-cover transition-transform group-hover:scale-105"
          />
        ) : null}
        <span className="pointer-events-none absolute inset-0 flex items-center justify-center bg-black/0 text-[12px] font-semibold text-white opacity-0 transition-opacity group-hover:bg-black/40 group-hover:opacity-100 group-focus-visible:bg-black/40 group-focus-visible:opacity-100">
          {actionLabel}
        </span>
      </span>

      <span className="adm-text mt-2 block truncate text-[12px] font-semibold" title={item.originalFileName}>
        {item.originalFileName}
      </span>
      <span className="adm-subtle flex items-center justify-between text-[11px]">
        <span>{labelForMediaContentType(item.contentType)}</span>
        <span dir="ltr">{formatFileSize(item.sizeBytes)}</span>
      </span>
      <span className="adm-subtle flex items-center justify-between text-[11px]">
        <span>{dimensions ?? "—"}</span>
        <span>{formatDateFa(item.createdAtUtc)}</span>
      </span>
    </button>
  );
}
