"use client";

import { useEffect, useId, useRef, useState } from "react";
import { emptyNotificationFeed, type NotificationItem } from "@/lib/notifications";

type NotificationCenterProps = {
  /** Optional feed. Defaults to empty foundation (no fake items). */
  feed?: { items: NotificationItem[]; unreadCount: number };
  className?: string;
};

/**
 * User notification center foundation.
 * Renders empty / read-unread UI only. Does not invent notifications.
 */
export function NotificationCenter({ feed, className = "" }: NotificationCenterProps) {
  const [open, setOpen] = useState(false);
  const panelId = useId();
  const rootRef = useRef<HTMLDivElement>(null);
  const data = feed ?? emptyNotificationFeed();
  const unread = data.unreadCount;

  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") setOpen(false);
    }
    function onClick(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    }
    window.addEventListener("keydown", onKey);
    window.addEventListener("mousedown", onClick);
    return () => {
      window.removeEventListener("keydown", onKey);
      window.removeEventListener("mousedown", onClick);
    };
  }, [open]);

  return (
    <div ref={rootRef} className={`relative ${className}`.trim()} dir="rtl">
      <button
        type="button"
        className="focus-ring relative flex h-10 w-10 items-center justify-center rounded-xl border border-white/10 bg-white/5 text-slate-300 hover:bg-white/10"
        aria-label={unread > 0 ? `اعلان‌ها، ${unread} خوانده‌نشده` : "اعلان‌ها"}
        aria-haspopup="dialog"
        aria-expanded={open}
        aria-controls={panelId}
        onClick={() => setOpen((value) => !value)}
      >
        <BellIcon />
        {unread > 0 ? (
          <span className="absolute -top-1 -start-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-rose-500 px-1 text-[10px] font-bold text-white">
            {unread}
            <span className="sr-only">خوانده‌نشده</span>
          </span>
        ) : null}
      </button>

      {open ? (
        <div
          id={panelId}
          role="dialog"
          aria-label="مرکز اعلان‌ها"
          className="absolute end-0 z-50 mt-2 w-[min(100vw-2rem,20rem)] rounded-2xl border border-white/10 bg-[#121826] p-3 shadow-xl"
        >
          <div className="mb-2 flex items-center justify-between border-b border-white/10 pb-2">
            <h2 className="text-[13px] font-bold text-white">اعلان‌ها</h2>
            <span className="text-[11px] text-slate-500">
              {unread > 0 ? `${unread} خوانده‌نشده` : "همه خوانده شده"}
            </span>
          </div>

          {data.items.length === 0 ? (
            <div className="px-2 py-8 text-center" role="status">
              <p className="text-[13px] font-semibold text-white">اعلان جدیدی نیست</p>
              <p className="mt-1 text-[12px] leading-6 text-slate-400">
                وقتی سرویس اعلان فعال شود، رویدادها اینجا نمایش داده می‌شوند. هیچ مورد ساختگی نشان داده نمی‌شود.
              </p>
            </div>
          ) : (
            <ul className="max-h-72 space-y-2 overflow-y-auto">
              {data.items.map((item) => (
                <li
                  key={item.id}
                  className={[
                    "rounded-xl border px-3 py-2",
                    item.read
                      ? "border-white/5 bg-white/[0.02] text-slate-400"
                      : "border-violet-500/20 bg-violet-500/10 text-slate-200",
                  ].join(" ")}
                >
                  <div className="flex items-center justify-between gap-2">
                    <p className="text-[13px] font-semibold">{item.title}</p>
                    <span className="text-[10px] font-bold">
                      {item.read ? "خوانده‌شده" : "جدید"}
                    </span>
                  </div>
                  <p className="mt-1 text-[12px] leading-5 text-slate-400">{item.body}</p>
                </li>
              ))}
            </ul>
          )}
        </div>
      ) : null}
    </div>
  );
}

function BellIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
      <path
        d="M6 9a6 6 0 1 1 12 0c0 7 3 7 3 7H3s3 0 3-7Z"
        stroke="currentColor"
        strokeWidth="1.8"
        strokeLinejoin="round"
      />
      <path d="M10 19a2 2 0 0 0 4 0" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}
