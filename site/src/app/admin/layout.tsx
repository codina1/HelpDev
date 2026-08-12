import type { Metadata } from "next";
import type { ReactNode } from "react";
import { AdminShell } from "@/components/admin/layout/admin-shell";
import { ADMIN_PREFERENCES_STORAGE_KEY } from "@/lib/admin/preferences";

export const metadata: Metadata = {
  title: {
    default: "پنل مدیریت",
    template: "%s | مدیریت HelpDev",
  },
  robots: { index: false, follow: false },
};

// Applies the persisted admin theme + sidebar state before first paint to
// avoid a flash / layout shift. Reads only non-sensitive UI preferences.
const preferenceScript = `(function(){try{var k=${JSON.stringify(
  ADMIN_PREFERENCES_STORAGE_KEY,
)};var r=localStorage.getItem(k);var p=r?JSON.parse(r):null;var t=p&&p.theme?p.theme:'system';var dark=t==='dark'||(t==='system'&&window.matchMedia('(prefers-color-scheme: dark)').matches);var e=document.documentElement;e.setAttribute('data-admin-theme',dark?'dark':'light');e.setAttribute('data-admin-sidebar',p&&p.sidebarCollapsed?'collapsed':'expanded');}catch(_){document.documentElement.setAttribute('data-admin-theme','dark');document.documentElement.setAttribute('data-admin-sidebar','expanded');}})();`;

export default function AdminRootLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <script dangerouslySetInnerHTML={{ __html: preferenceScript }} />
      <AdminShell>{children}</AdminShell>
    </>
  );
}
