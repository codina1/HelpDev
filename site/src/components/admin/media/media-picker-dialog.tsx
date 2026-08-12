"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { MediaToolbar } from "@/components/admin/media/media-toolbar";
import { MediaGrid } from "@/components/admin/media/media-grid";
import { MediaSkeleton } from "@/components/admin/media/media-skeleton";
import { MediaEmptyState } from "@/components/admin/media/media-empty-state";
import { MediaPagination } from "@/components/admin/media/media-pagination";
import { MediaUploadDialog } from "@/components/admin/media/media-upload-dialog";
import { useAdminMediaList } from "@/lib/admin/media/media-hooks";
import { toMediaPickerSelection } from "@/lib/admin/media/media-mappers";
import { mergeAdminMediaListQuery, isAdminMediaListFiltered } from "@/lib/admin/media/media-url-state";
import {
  DEFAULT_ADMIN_MEDIA_LIST_QUERY,
  type AdminMediaDetail,
  type AdminMediaListItem,
  type AdminMediaListQuery,
  type MediaPickerSelection,
} from "@/lib/admin/media/media-types";

type MediaPickerDialogProps = {
  open: boolean;
  onClose: () => void;
  onSelect: (selection: MediaPickerSelection) => void;
  title?: string;
};

/**
 * Browse-and-select image picker for text fields that store an image URL
 * (Cover image, SEO OG image). Selection never auto-saves the host form — the
 * caller decides what to do with the returned `{ id, publicUrl, altText,
 * width, height }` (plus a resolved `absoluteUrl`).
 *
 * Uses its own local (non-URL-driven) query state since it is a modal, not a
 * page. Upload is available inline: a freshly uploaded asset is selected and
 * the picker closes immediately.
 */
export function MediaPickerDialog({
  open,
  onClose,
  onSelect,
  title = "انتخاب رسانه",
}: MediaPickerDialogProps) {
  const [query, setQuery] = useState<AdminMediaListQuery>(DEFAULT_ADMIN_MEDIA_LIST_QUERY);
  const [uploadOpen, setUploadOpen] = useState(false);
  const list = useAdminMediaList(query);
  const dialogRef = useRef<HTMLDivElement>(null);

  // Reset the local browse state each time the picker is (re-)opened.
  useEffect(() => {
    if (open) {
      setQuery(DEFAULT_ADMIN_MEDIA_LIST_QUERY);
      setUploadOpen(false);
    }
  }, [open]);

  useEffect(() => {
    if (!open) return;
    dialogRef.current?.querySelector<HTMLElement>("input,button")?.focus();
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") onClose();
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open, onClose]);

  const patchQuery = useCallback((patch: Partial<AdminMediaListQuery>) => {
    setQuery((current) => mergeAdminMediaListQuery(current, patch));
  }, []);

  const handleItemClick = useCallback(
    (item: AdminMediaListItem) => {
      onSelect(toMediaPickerSelection(item));
      onClose();
    },
    [onSelect, onClose],
  );

  const handleUploaded = useCallback(
    (detail: AdminMediaDetail) => {
      onSelect(toMediaPickerSelection(detail));
      onClose();
    },
    [onSelect, onClose],
  );

  if (!open) return null;

  const filtered = isAdminMediaListFiltered(query);

  return (
    <>
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4" onMouseDown={onClose}>
      <div
        ref={dialogRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby="media-picker-title"
        className="adm-surface flex h-[85vh] w-full max-w-3xl flex-col gap-4 rounded-xl p-5 shadow-[var(--adm-shadow)]"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <div className="flex items-center justify-between gap-2">
          <h3 id="media-picker-title" className="adm-text text-[15px] font-bold">
            {title}
          </h3>
          <button
            type="button"
            onClick={onClose}
            aria-label="بستن"
            className="adm-btn adm-btn-ghost adm-focus p-1.5"
          >
            <AdminIcon name="close" size={16} />
          </button>
        </div>

        <MediaToolbar
          search={query.search}
          disabled={list.loading && !list.data}
          onSearchCommit={(search) => patchQuery({ search })}
          onUploadClick={() => setUploadOpen(true)}
        />

        <div className="min-h-0 flex-1 overflow-y-auto">
          {list.loading && !list.data ? (
            <MediaSkeleton count={9} />
          ) : list.error && !list.data ? (
            <AdminErrorState error={list.error} onRetry={list.reload} showHome={false} />
          ) : list.data && list.data.items.length === 0 ? (
            <MediaEmptyState
              filtered={filtered}
              onClearFilters={filtered ? () => setQuery(DEFAULT_ADMIN_MEDIA_LIST_QUERY) : undefined}
              onUpload={() => setUploadOpen(true)}
            />
          ) : list.data ? (
            <MediaGrid items={list.data.items} onItemClick={handleItemClick} actionLabel="انتخاب" />
          ) : null}
        </div>

        {list.data && list.data.items.length > 0 ? (
          <MediaPagination
            page={list.data.page}
            pageSize={list.data.pageSize}
            totalCount={list.data.totalCount}
            totalPages={list.data.totalPages}
            disabled={list.refreshing}
            idPrefix="admin-media-picker"
            onPageChange={(page) => patchQuery({ page })}
            onPageSizeChange={(pageSize) => patchQuery({ pageSize })}
          />
        ) : null}
      </div>
    </div>

    <MediaUploadDialog
      open={uploadOpen}
      onClose={() => setUploadOpen(false)}
      onUploaded={handleUploaded}
    />
    </>
  );
}
