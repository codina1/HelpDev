"use client";

import { useEffect, useState } from "react";
import { AdminActionBar } from "@/components/admin/page/admin-action-bar";
import { ContentFilters } from "@/components/admin/content/list/content-filters";
import type { AdminContentListQuery } from "@/lib/admin/content/content-types";

const SEARCH_DEBOUNCE_MS = 400;

type ContentToolbarProps = {
  query: AdminContentListQuery;
  disabled?: boolean;
  hideTypeFilter?: boolean;
  onSearchCommit: (search: string) => void;
  onStatusChange: (status: AdminContentListQuery["status"]) => void;
  onTypeChange: (type: AdminContentListQuery["type"]) => void;
};

/**
 * Search + filters toolbar. Search is debounced (~400ms) before committing to
 * URL/server state; type/status commit immediately and reset page upstream.
 */
export function ContentToolbar({
  query,
  disabled = false,
  hideTypeFilter = false,
  onSearchCommit,
  onStatusChange,
  onTypeChange,
}: ContentToolbarProps) {
  const [draftSearch, setDraftSearch] = useState(query.search);

  // Sync local draft when URL/back-forward changes the committed search.
  useEffect(() => {
    setDraftSearch(query.search);
  }, [query.search]);

  useEffect(() => {
    const trimmed = draftSearch.trim();
    if (trimmed === query.search.trim()) return;
    const handle = window.setTimeout(() => {
      onSearchCommit(draftSearch);
    }, SEARCH_DEBOUNCE_MS);
    return () => window.clearTimeout(handle);
  }, [draftSearch, query.search, onSearchCommit]);

  return (
    <AdminActionBar
      filters={
        <div className="flex w-full max-w-xs flex-col gap-2">
          <label className="sr-only" htmlFor="content-search">
            جستجوی محتوا
          </label>
          <input
            id="content-search"
            type="search"
            value={draftSearch}
            disabled={disabled}
            onChange={(event) => setDraftSearch(event.target.value)}
            placeholder="جستجوی عنوان یا اسلاگ..."
            className="adm-input"
            autoComplete="off"
          />
          <ContentFilters
            status={query.status}
            type={query.type}
            disabled={disabled}
            hideTypeFilter={hideTypeFilter}
            onStatusChange={onStatusChange}
            onTypeChange={onTypeChange}
          />
        </div>
      }
    />
  );
}
