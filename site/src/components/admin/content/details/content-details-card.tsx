import { AdminSurface } from "@/components/admin/page/admin-surface";
import { MarkdownPreview } from "@/components/admin/content/shared/markdown-preview";
import { ContentTypeBadge } from "@/components/admin/content/shared/content-type-badge";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import type { ContentDetail } from "@/lib/admin/content/content-types";

/** Main content preview for the details page. */
export function ContentDetailsCard({ content }: { content: ContentDetail }) {
  return (
    <AdminSurface className="space-y-4">
      <div className="space-y-2 border-b border-[var(--adm-border)] pb-4">
        <div className="flex flex-wrap items-center gap-2">
          <ContentTypeBadge type={content.type} />
          <ContentStatusBadge status={content.status} />
        </div>
        <h1 className="adm-text text-xl font-black">{content.title}</h1>
        <p dir="ltr" className="adm-subtle text-start text-[12px]">
          /{content.slug}
        </p>
      </div>

      <MarkdownPreview source={content.body} />
    </AdminSurface>
  );
}
