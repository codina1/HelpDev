import { AdminSurface } from "@/components/admin/page/admin-surface";
import { ContentTypeBadge } from "@/components/admin/content/shared/content-type-badge";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import { formatDateFa, shortAuthorId } from "@/lib/admin/content/content-mappers";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type { ContentDetail } from "@/lib/admin/content/content-types";

/** Sidebar metadata for a content item. */
export function ContentMetaCard({ content }: { content: ContentDetail }) {
  return (
    <AdminSurface className="space-y-3">
      <h2 className="adm-text text-[14px] font-bold">مشخصات</h2>
      <dl className="space-y-3 text-[13px]">
        <Row label="وضعیت">
          <ContentStatusBadge status={content.status} />
        </Row>
        <Row label="نوع">
          <ContentTypeBadge type={content.type} />
        </Row>
        <Row label="نویسنده">
          <span dir="ltr" className="adm-muted font-mono text-[11px]">
            {shortAuthorId(content.authorId)}
          </span>
        </Row>
        <Row label="اسلاگ">
          <span dir="ltr" className="adm-muted text-start text-[11px]">
            /{content.slug}
          </span>
        </Row>
        <Row label="تاریخ ایجاد">
          <span className="adm-muted">{formatDateFa(content.createdAt)}</span>
        </Row>
        <Row label="بازدید">
          <span className="adm-muted tabular-nums">{formatNumberFa(content.views)}</span>
        </Row>
        <Row label="ذخیره">
          <span className="adm-muted tabular-nums">{formatNumberFa(content.saves)}</span>
        </Row>
      </dl>
    </AdminSurface>
  );
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <dt className="adm-subtle">{label}</dt>
      <dd className="text-end">{children}</dd>
    </div>
  );
}
