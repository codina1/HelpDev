"use client";

import { ADMIN_CONTENT_PAGE_SIZES } from "@/lib/admin/content/content-types";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export type AdminPaginationProps = {
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  disabled?: boolean;
  /** Selectable page sizes. Defaults to the Content module's set for backward compatibility. */
  pageSizeOptions?: readonly number[];
  /** Accessible label for the nav element. Defaults to the Content module's label. */
  ariaLabel?: string;
  /** Unique id prefix for the page-size `<select>` (avoids duplicate ids when reused on the same page). */
  idPrefix?: string;
  onPageChange: (page: number) => void;
  onPageSizeChange: (pageSize: number) => void;
};

/**
 * Reusable Admin pagination: previous/next, page indicator, page-size selector.
 * RTL-correct (logical chevrons), keyboard accessible, compact on mobile.
 * Generic over `pageSizeOptions` so other modules (e.g. Media) can reuse it.
 */
export function AdminPagination({
  page,
  pageSize,
  totalCount,
  totalPages,
  disabled = false,
  pageSizeOptions = ADMIN_CONTENT_PAGE_SIZES,
  ariaLabel = "صفحه‌بندی فهرست محتوا",
  idPrefix = "admin-content",
  onPageChange,
  onPageSizeChange,
}: AdminPaginationProps) {
  const safeTotalPages = Math.max(totalPages, 1);
  const atStart = page <= 1;
  const atEnd = page >= safeTotalPages || totalCount === 0;
  const pageSizeId = `${idPrefix}-page-size`;

  return (
    <nav
      aria-label={ariaLabel}
      className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between"
    >
      <p className="adm-muted text-[12px]" aria-live="polite">
        {totalCount === 0
          ? "نتیجه‌ای نیست"
          : `${formatNumberFa(totalCount)} نتیجه · صفحه ${formatNumberFa(page)} از ${formatNumberFa(safeTotalPages)}`}
      </p>

      <div className="flex flex-wrap items-center gap-2">
        <label className="sr-only" htmlFor={pageSizeId}>
          تعداد در هر صفحه
        </label>
        <select
          id={pageSizeId}
          className="adm-input w-auto text-[12px]"
          value={pageSize}
          disabled={disabled}
          onChange={(event) => onPageSizeChange(Number(event.target.value))}
        >
          {pageSizeOptions.map((size) => (
            <option key={size} value={size}>
              {formatNumberFa(size)} در صفحه
            </option>
          ))}
        </select>

        <div className="flex items-center gap-1">
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus px-2.5 py-1.5 text-[12px]"
            disabled={disabled || atStart}
            aria-label="صفحه قبل"
            onClick={() => onPageChange(page - 1)}
          >
            <AdminIcon name="collapse" size={14} className="rtl:hidden" />
            <AdminIcon name="expand" size={14} className="hidden rtl:inline" />
            <span className="ms-1 hidden sm:inline">قبلی</span>
          </button>
          <span
            className="adm-text min-w-[2.5rem] text-center text-[12px] font-semibold"
            aria-current="page"
          >
            {formatNumberFa(page)}
          </span>
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus px-2.5 py-1.5 text-[12px]"
            disabled={disabled || atEnd}
            aria-label="صفحه بعد"
            onClick={() => onPageChange(page + 1)}
          >
            <span className="me-1 hidden sm:inline">بعدی</span>
            <AdminIcon name="expand" size={14} className="rtl:hidden" />
            <AdminIcon name="collapse" size={14} className="hidden rtl:inline" />
          </button>
        </div>
      </div>
    </nav>
  );
}
