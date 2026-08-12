import { getAdminEnvironmentMeta } from "@/lib/admin/environment";

export function AdminFooter() {
  const env = getAdminEnvironmentMeta();
  const year = new Date().getFullYear();

  return (
    <footer className="adm-border-b adm-subtle flex flex-wrap items-center justify-between gap-2 border-0 border-t px-4 py-3 text-[11px] lg:px-8">
      <span>© {year} HelpDev — پنل مدیریت</span>
      <span>محیط: {env.label}</span>
    </footer>
  );
}
