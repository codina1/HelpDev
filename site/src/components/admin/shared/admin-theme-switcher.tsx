"use client";

import { useAdminPreferences } from "@/components/admin/admin-preferences-provider";
import { AdminMenu } from "@/components/admin/shared/admin-menu";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import type { AdminIconName } from "@/lib/admin/navigation";
import type { AdminTheme } from "@/lib/admin/preferences";

const OPTIONS: Array<{ value: AdminTheme; label: string; icon: AdminIconName }> = [
  { value: "light", label: "روشن", icon: "sun" },
  { value: "dark", label: "تیره", icon: "moon" },
  { value: "system", label: "سیستم", icon: "settings" },
];

export function AdminThemeSwitcher() {
  const { preferences, effectiveTheme, setTheme } = useAdminPreferences();
  const activeIcon: AdminIconName = effectiveTheme === "dark" ? "moon" : "sun";

  return (
    <AdminMenu
      label="انتخاب پوسته"
      trigger={({ toggle, triggerProps }) => (
        <button
          type="button"
          onClick={toggle}
          className="adm-icon-btn adm-focus"
          aria-label="تغییر پوسته"
          {...triggerProps}
        >
          <AdminIcon name={activeIcon} size={18} />
        </button>
      )}
    >
      {({ close }) => (
        <div role="none" className="min-w-[180px]">
          {OPTIONS.map((option) => {
            const selected = preferences.theme === option.value;
            return (
              <button
                key={option.value}
                type="button"
                role="menuitemradio"
                aria-checked={selected}
                onClick={() => {
                  setTheme(option.value);
                  close();
                }}
                className={`adm-focus flex w-full items-center justify-between gap-2 rounded-lg px-3 py-2 text-[13px] ${
                  selected
                    ? "bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]"
                    : "adm-muted adm-hover"
                }`}
              >
                <span className="flex items-center gap-2">
                  <AdminIcon name={option.icon} size={16} />
                  {option.label}
                </span>
                {selected ? <AdminIcon name="check" size={14} /> : null}
              </button>
            );
          })}
        </div>
      )}
    </AdminMenu>
  );
}
