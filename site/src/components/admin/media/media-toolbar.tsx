"use client";

import { useEffect, useState } from "react";
import { AdminActionBar } from "@/components/admin/page/admin-action-bar";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

const SEARCH_DEBOUNCE_MS = 400;

type MediaToolbarProps = {
  search: string;
  disabled?: boolean;
  onSearchCommit: (search: string) => void;
  onUploadClick: () => void;
};

/**
 * Search box + upload trigger. Search is debounced (~400ms) before committing
 * to URL/server state — never re-fetches or uploads on every keystroke.
 */
export function MediaToolbar({
  search,
  disabled = false,
  onSearchCommit,
  onUploadClick,
}: MediaToolbarProps) {
  const [draftSearch, setDraftSearch] = useState(search);

  useEffect(() => {
    setDraftSearch(search);
  }, [search]);

  useEffect(() => {
    const trimmed = draftSearch.trim();
    if (trimmed === search.trim()) return;
    const handle = window.setTimeout(() => {
      onSearchCommit(draftSearch);
    }, SEARCH_DEBOUNCE_MS);
    return () => window.clearTimeout(handle);
  }, [draftSearch, search, onSearchCommit]);

  return (
    <AdminActionBar
      filters={
        <>
          <label className="sr-only" htmlFor="media-search">
            جستجوی رسانه
          </label>
          <input
            id="media-search"
            type="search"
            value={draftSearch}
            disabled={disabled}
            onChange={(event) => setDraftSearch(event.target.value)}
            placeholder="جستجوی نام فایل..."
            className="adm-input max-w-xs"
            autoComplete="off"
          />
        </>
      }
      actions={
        <button
          type="button"
          onClick={onUploadClick}
          className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
        >
          <AdminIcon name="plus" size={16} />
          بارگذاری رسانه
        </button>
      }
    />
  );
}
