"use client";

import type { AdminNavGroup as AdminNavGroupModel } from "@/lib/admin/navigation";
import { useAdminPreferences } from "@/components/admin/admin-preferences-provider";
import { AdminNavItem } from "@/components/admin/navigation/admin-nav-item";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminNavGroupProps = {
  group: AdminNavGroupModel;
  activeItemId: string | null;
  variant: "desktop" | "mobile";
  onNavigate?: () => void;
};

/**
 * A collapsible navigation group. On the icon-collapsed desktop sidebar the
 * accordion is ignored so icons stay reachable; in expanded/mobile modes the
 * group can be collapsed and the state is persisted.
 */
export function AdminNavGroup({
  group,
  activeItemId,
  variant,
  onNavigate,
}: AdminNavGroupProps) {
  const { preferences, isGroupCollapsed, toggleGroup } = useAdminPreferences();
  const iconOnly = variant === "desktop" && preferences.sidebarCollapsed;
  const collapsed = isGroupCollapsed(group.id) && !iconOnly;
  const showLabel = variant === "mobile";

  const items = collapsed ? null : (
    <ul className="space-y-0.5">
      {group.items.map((item) => (
        <li key={item.id}>
          <AdminNavItem
            item={item}
            active={item.id === activeItemId}
            showLabel={showLabel}
            onNavigate={onNavigate}
          />
        </li>
      ))}
    </ul>
  );

  return (
    <div className="space-y-1">
      {group.title ? (
        iconOnly ? (
          <div className="adm-border-b mx-2 my-1" aria-hidden />
        ) : (
          <button
            type="button"
            onClick={() => toggleGroup(group.id)}
            aria-expanded={!collapsed}
            className="adm-focus flex w-full items-center justify-between rounded-lg px-2 py-1"
          >
            <span className="adm-group-title adm-subtle text-[10px] font-bold uppercase tracking-wide">
              {group.title}
            </span>
            <span className="adm-collapsible adm-subtle">
              <AdminIcon
                name="chevron"
                size={14}
                className={collapsed ? "-rotate-90" : ""}
              />
            </span>
          </button>
        )
      ) : null}
      {items}
    </div>
  );
}
