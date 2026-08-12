"use client";

import Link from "next/link";
import { listContentWorkspaces } from "@/lib/admin/content/registry";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

const PERSISTENCE_LABEL: Record<string, string> = {
  "content-api": "API محتوا",
  "prompt-lab": "Prompt Lab",
  none: "فاندیشن (بدون ذخیره)",
};

/**
 * `/admin/content` — Content Platform hub (workspace navigation).
 */
export function ContentPlatformHub() {
  const workspaces = listContentWorkspaces();

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="پلتفرم محتوا"
        description="فضاهای کار تخصصی برای مقالات، اخبار، ابزارها، نقشه راه و سایر قالب‌ها."
        breadcrumbs={[{ title: "محتوا", current: true }]}
        secondaryActions={
          <Link
            href={ADMIN_ROUTES.contentAll}
            className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="folder" size={16} />
            همه محتواها
          </Link>
        }
        primaryAction={
          <Link
            href={ADMIN_ROUTES.contentArticlesNew}
            className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="plus" size={16} />
            مقاله جدید
          </Link>
        }
      />

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {workspaces.map((ws) => (
          <Link key={ws.id} href={ws.route} className="group block">
            <AdminSurface className="h-full space-y-3 transition group-hover:border-[var(--adm-accent)]">
              <div className="flex items-start gap-3">
                <span className="flex h-10 w-10 items-center justify-center rounded-full bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]">
                  <AdminIcon name={ws.icon} size={20} />
                </span>
                <div className="min-w-0 space-y-1">
                  <h2 className="adm-text text-[14px] font-bold">{ws.shortTitle}</h2>
                  <p className="adm-muted text-[12px] leading-6">{ws.description}</p>
                </div>
              </div>
              <p className="adm-subtle text-[11px]">
                ذخیره: {PERSISTENCE_LABEL[ws.persistence] ?? ws.persistence}
              </p>
            </AdminSurface>
          </Link>
        ))}
      </div>

      <AdminSurface className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h3 className="adm-text text-[13px] font-bold">گردش کار و رسانه</h3>
          <p className="adm-muted text-[12px]">ابزارهای جانبی CMS بدون ترک پلتفرم محتوا.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link href={ADMIN_ROUTES.contentWorkflows} className="adm-btn adm-btn-outline adm-focus">
            گردش کار AI
          </Link>
          <Link href={ADMIN_ROUTES.media} className="adm-btn adm-btn-outline adm-focus">
            رسانه‌ها
          </Link>
          <Link href={ADMIN_ROUTES.seo} className="adm-btn adm-btn-outline adm-focus">
            SEO
          </Link>
        </div>
      </AdminSurface>
    </div>
  );
}
