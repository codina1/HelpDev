import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { HelpDevLogo } from "@/components/account/dashboard/dashboard-icons";

type AdminLogoProps = {
  /** When true, only the mark is shown (used in the collapsed sidebar). */
  compact?: boolean;
};

export function AdminLogo({ compact = false }: AdminLogoProps) {
  return (
    <Link
      href={ADMIN_ROUTES.dashboard}
      className="adm-focus flex items-center gap-2 rounded-lg"
      aria-label="داشبورد مدیریت HelpDev"
    >
      <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[var(--adm-accent-soft)]">
        <HelpDevLogo size={22} />
      </span>
      {!compact ? (
        <span className="adm-collapsible flex min-w-0 flex-col leading-tight">
          <span className="adm-text text-[13px] font-extrabold">HelpDev</span>
          <span className="adm-subtle text-[10px] font-semibold">پنل مدیریت</span>
        </span>
      ) : null}
    </Link>
  );
}
