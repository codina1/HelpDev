"use client";

import { AdminPagination } from "@/components/admin/shared/admin-pagination";
import { ADMIN_MEDIA_PAGE_SIZES, type AdminMediaPageSize } from "@/lib/admin/media/media-types";

type MediaPaginationProps = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  disabled?: boolean;
  /** Distinguishes the full workspace pagination from the picker dialog's (avoids duplicate ids). */
  idPrefix?: string;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: AdminMediaPageSize) => void;
};

/** Thin wrapper reusing the shared `AdminPagination` with Media's page-size set. */
export function MediaPagination({
  onPageSizeChange,
  idPrefix = "admin-media",
  ...rest
}: MediaPaginationProps) {
  return (
    <AdminPagination
      {...rest}
      pageSizeOptions={ADMIN_MEDIA_PAGE_SIZES}
      ariaLabel="صفحه‌بندی رسانه‌ها"
      idPrefix={idPrefix}
      compact
      onPageSizeChange={(pageSize) => onPageSizeChange(pageSize as AdminMediaPageSize)}
    />
  );
}
