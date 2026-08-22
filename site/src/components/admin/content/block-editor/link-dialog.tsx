"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import { fetchAdminContentList } from "@/lib/admin/content/content-api";
import { mapAdminContentListItem } from "@/lib/admin/content/content-mappers";
import type { AdminContentListItem } from "@/lib/admin/content/content-types";
import styles from "./article-rich-text-editor.module.css";

export type LinkDialogValue = {
  href: string;
  text: string;
  title: string;
  newTab: boolean;
};

type LinkDialogProps = {
  open: boolean;
  initial: LinkDialogValue;
  onClose: () => void;
  onApply: (value: LinkDialogValue) => void;
  onRemove: () => void;
};

export function LinkDialog({ open, initial, onClose, onApply, onRemove }: LinkDialogProps) {
  const { token } = useAuth();
  const [value, setValue] = useState(initial);
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<AdminContentListItem[]>([]);

  useEffect(() => {
    if (open) setValue(initial);
  }, [initial, open]);

  useEffect(() => {
    if (!open || !token || query.trim().length < 2) {
      setResults([]);
      return;
    }
    const controller = new AbortController();
    const timer = window.setTimeout(() => {
      void fetchAdminContentList(
        token,
        { page: 1, pageSize: 10, search: query.trim(), status: "all", type: "Article" },
        controller.signal,
      )
        .then((raw) => setResults((raw.items ?? []).map(mapAdminContentListItem)))
        .catch(() => setResults([]));
    }, 250);
    return () => {
      window.clearTimeout(timer);
      controller.abort();
    };
  }, [open, query, token]);

  if (!open) return null;

  return (
    <div className={styles.dialogBackdrop} onMouseDown={onClose}>
      <form
        className={styles.dialog}
        dir="rtl"
        onMouseDown={(event) => event.stopPropagation()}
        onSubmit={(event) => {
          event.preventDefault();
          onApply(value);
        }}
      >
        <h2 className="adm-text mb-3 text-[15px] font-bold">درج پیوند</h2>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">نشانی</span>
          <input
            className="adm-input text-start"
            dir="ltr"
            value={value.href}
            onChange={(event) => setValue((prev) => ({ ...prev, href: event.target.value }))}
          />
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">متن پیوند</span>
          <input
            className="adm-input"
            value={value.text}
            onChange={(event) => setValue((prev) => ({ ...prev, text: event.target.value }))}
          />
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">عنوان</span>
          <input
            className="adm-input"
            value={value.title}
            onChange={(event) => setValue((prev) => ({ ...prev, title: event.target.value }))}
          />
        </label>
        <label className="mb-3 flex items-center gap-2 text-[12px]">
          <input
            type="checkbox"
            checked={value.newTab}
            onChange={(event) => setValue((prev) => ({ ...prev, newTab: event.target.checked }))}
          />
          بازشدن در تب جدید
        </label>
        <label className="mb-2 block space-y-1">
          <span className="adm-text text-[12px] font-semibold">جست‌وجوی مقالات داخلی</span>
          <input
            className="adm-input"
            value={query}
            placeholder="حداقل دو حرف"
            onChange={(event) => setQuery(event.target.value)}
          />
        </label>
        {results.length > 0 ? (
          <ul className="mb-3 max-h-36 space-y-1 overflow-auto">
            {results.map((item) => (
              <li key={item.id}>
                <button
                  type="button"
                  className="adm-btn adm-btn-ghost adm-focus w-full justify-start text-start text-[12px]"
                  onClick={() =>
                    setValue((prev) => ({
                      ...prev,
                      href: `/articles/${item.slug}`,
                      text: prev.text || item.title,
                      title: item.title,
                    }))
                  }
                >
                  {item.title}
                </button>
              </li>
            ))}
          </ul>
        ) : null}
        <div className="flex flex-wrap justify-end gap-2">
          <button type="button" className="adm-btn adm-btn-ghost adm-focus" onClick={onRemove}>
            حذف پیوند
          </button>
          <button type="button" className="adm-btn adm-btn-outline adm-focus" onClick={onClose}>
            انصراف
          </button>
          <button type="submit" className="adm-btn adm-btn-primary adm-focus">
            اعمال
          </button>
        </div>
      </form>
    </div>
  );
}
