"use client";

import { CONTENT_STATUSES, CONTENT_TYPES } from "@/lib/admin/content/content-types";
import type { AdminContentListQuery } from "@/lib/admin/content/content-types";
import {
  labelForContentStatus,
  labelForContentType,
} from "@/lib/admin/content/content-mappers";

type ContentFiltersProps = {
  status: AdminContentListQuery["status"];
  type: AdminContentListQuery["type"];
  disabled?: boolean;
  /** Hide type filter when the workspace locks content type. */
  hideTypeFilter?: boolean;
  onStatusChange: (status: AdminContentListQuery["status"]) => void;
  onTypeChange: (type: AdminContentListQuery["type"]) => void;
};

/** Type and status filters — values are sent to the backend list endpoint. */
export function ContentFilters({
  status,
  type,
  disabled = false,
  hideTypeFilter = false,
  onStatusChange,
  onTypeChange,
}: ContentFiltersProps) {
  return (
    <>
      {!hideTypeFilter ? (
        <>
          <label className="sr-only" htmlFor="content-filter-type">
            فیلتر نوع محتوا
          </label>
          <select
            id="content-filter-type"
            className="adm-input"
            value={type}
            disabled={disabled}
            onChange={(event) =>
              onTypeChange(event.target.value as AdminContentListQuery["type"])
            }
          >
            <option value="all">همه نوع‌ها</option>
            {CONTENT_TYPES.map((value) => (
              <option key={value} value={value}>
                {labelForContentType(value)}
              </option>
            ))}
          </select>
        </>
      ) : null}

      <label className="sr-only" htmlFor="content-filter-status">
        فیلتر وضعیت
      </label>
      <select
        id="content-filter-status"
        className="adm-input"
        value={status}
        disabled={disabled}
        onChange={(event) =>
          onStatusChange(event.target.value as AdminContentListQuery["status"])
        }
      >
        <option value="all">همه وضعیت‌ها</option>
        {CONTENT_STATUSES.map((value) => (
          <option key={value} value={value}>
            {labelForContentStatus(value)}
          </option>
        ))}
      </select>
    </>
  );
}
