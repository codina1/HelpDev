"use client";

import { AdminMenu } from "@/components/admin/shared/admin-menu";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";

/**
 * Notifications entry point (UI foundation only).
 *
 * Phase 1 shows an empty state; there is no notifications endpoint yet, so no
 * unread badge or fake items are shown. Future integration point: wire this to
 * a real notifications/announcements feed and derive the unread badge from that
 * response only.
 */
export function AdminNotificationsButton() {
  return (
    <AdminMenu
      label="اعلان‌ها"
      panelClassName="w-[300px]"
      trigger={({ toggle, triggerProps }) => (
        <button
          type="button"
          onClick={toggle}
          className="adm-icon-btn adm-focus"
          aria-label="اعلان‌ها"
          {...triggerProps}
        >
          <AdminIcon name="bell" size={18} />
        </button>
      )}
    >
      {() => (
        <div className="p-1">
          <div className="adm-border-b flex items-center justify-between px-2 pb-2">
            <span className="adm-text text-[13px] font-bold">اعلان‌ها</span>
          </div>
          <div className="py-2">
            <AdminEmptyState
              icon="bell"
              title="اعلان جدیدی وجود ندارد"
              description="زمانی که رویداد تازه‌ای ثبت شود اینجا نمایش داده می‌شود."
            />
          </div>
        </div>
      )}
    </AdminMenu>
  );
}
