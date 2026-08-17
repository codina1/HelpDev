"use client";

import { AdminActionBar } from "@/components/admin/page/admin-action-bar";
import { WRITER_PROMPT_STATUSES, type WriterPromptListQuery } from "@/lib/admin/prompt-lab/writer-prompt-types";
import { labelForWriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-mappers";

type WriterPromptToolbarProps = {
  query: WriterPromptListQuery;
  disabled?: boolean;
  onStatusChange: (status: WriterPromptListQuery["status"]) => void;
};

/** Status filter toolbar for the writer prompt list. */
export function WriterPromptToolbar({
  query,
  disabled = false,
  onStatusChange,
}: WriterPromptToolbarProps) {
  return (
    <AdminActionBar
      filters={
        <>
          <label className="sr-only" htmlFor="writer-prompt-filter-status">
            فیلتر وضعیت
          </label>
          <select
            id="writer-prompt-filter-status"
            className="adm-input w-full max-w-xs"
            value={query.status}
            disabled={disabled}
            onChange={(event) =>
              onStatusChange(event.target.value as WriterPromptListQuery["status"])
            }
          >
            <option value="all">همه وضعیت‌ها</option>
            {WRITER_PROMPT_STATUSES.map((value) => (
              <option key={value} value={value}>
                {labelForWriterPromptStatus(value)}
              </option>
            ))}
          </select>
        </>
      }
    />
  );
}
