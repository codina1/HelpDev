import type { ReactNode } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";

type FutureCapabilityListProps = {
  title?: string;
  items: readonly string[];
};

/** Shows unsupported future fields as disabled “coming soon” — never fake-saved. */
export function FutureCapabilityList({
  title = "در نسخه آینده",
  items,
}: FutureCapabilityListProps) {
  if (items.length === 0) return null;

  return (
    <AdminSurface className="space-y-3 border-dashed opacity-90">
      <h3 className="adm-text text-[13px] font-bold">{title}</h3>
      <ul className="space-y-2">
        {items.map((item) => (
          <li key={item}>
            <label className="flex cursor-not-allowed items-center gap-2 opacity-60">
              <input type="checkbox" disabled className="adm-focus" />
              <span className="adm-muted text-[12px]">{item}</span>
            </label>
          </li>
        ))}
      </ul>
      <p className="adm-subtle text-[11px]">
        این فیلدها هنوز توسط API پشتیبانی نمی‌شوند و ذخیره نمی‌شوند.
      </p>
    </AdminSurface>
  );
}

export function WorkspaceNote({ children }: { children: ReactNode }) {
  return (
    <p className="adm-muted rounded-[var(--adm-radius-md)] border border-[var(--adm-border)] bg-[var(--adm-surface-2)] px-3 py-2 text-[12px] leading-6">
      {children}
    </p>
  );
}
