import { AdminSurface } from "@/components/admin/page/admin-surface";
import { ContentActions } from "@/components/admin/content/shared/content-actions";
import { ContentTypeBadge } from "@/components/admin/content/shared/content-type-badge";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import { formatDateFa, shortAuthorId } from "@/lib/admin/content/content-mappers";
import type { AdminContentListItem } from "@/lib/admin/content/content-types";

type ContentTableProps = {
  items: AdminContentListItem[];
  publishingId?: string | null;
  lastPublishId?: string | null;
  publishError?: unknown;
  onPublish?: (item: AdminContentListItem) => Promise<void> | void;
  selectedIds?: Set<string>;
  onToggleSelect?: (id: string) => void;
  onToggleSelectAll?: () => void;
};

/** Responsive content list: dense table on desktop, stacked cards on mobile. */
export function ContentTable({
  items,
  publishingId = null,
  lastPublishId = null,
  publishError,
  onPublish,
  selectedIds,
  onToggleSelect,
  onToggleSelectAll,
}: ContentTableProps) {
  const selectionEnabled = Boolean(selectedIds && onToggleSelect);
  const allSelected =
    selectionEnabled && items.length > 0 && items.every((item) => selectedIds!.has(item.id));
  const rowPublishError = (id: string) =>
    lastPublishId === id ? publishError : undefined;

  return (
    <>
      <AdminSurface padding="none" className="hidden md:block overflow-x-auto">
        <table className="w-full min-w-[760px] text-start text-[13px]">
          <thead>
            <tr className="adm-border-b adm-subtle text-[11px]">
              {selectionEnabled ? (
                <th className="px-3 py-3 text-start font-semibold">
                  <input
                    type="checkbox"
                    checked={allSelected}
                    onChange={onToggleSelectAll}
                    aria-label="انتخاب همه ردیف‌های صفحه"
                    className="adm-focus h-4 w-4 rounded border-[var(--adm-border)]"
                  />
                </th>
              ) : null}
              <th className="px-4 py-3 text-start font-semibold">عنوان</th>
              <th className="px-4 py-3 text-start font-semibold">نوع</th>
              <th className="px-4 py-3 text-start font-semibold">وضعیت</th>
              <th className="px-4 py-3 text-start font-semibold">نویسنده</th>
              <th className="px-4 py-3 text-start font-semibold">آخرین تغییر</th>
              <th className="px-4 py-3 text-start font-semibold">انتشار</th>
              <th className="px-4 py-3 text-start font-semibold">عملیات</th>
            </tr>
          </thead>
          <tbody className="adm-divide">
            {items.map((item) => {
              const selected = selectedIds?.has(item.id) ?? false;
              return (
                <tr key={item.id} className="adm-hover align-top">
                  {selectionEnabled ? (
                    <td className="px-3 py-3">
                      <input
                        type="checkbox"
                        checked={selected}
                        onChange={() => onToggleSelect?.(item.id)}
                        aria-label={`انتخاب ${item.title}`}
                        className="adm-focus h-4 w-4 rounded border-[var(--adm-border)]"
                      />
                    </td>
                  ) : null}
                  <td className="px-4 py-3">
                    <span className="adm-text block max-w-[280px] truncate font-semibold" title={item.title}>
                      {item.title}
                    </span>
                    <span dir="ltr" className="adm-subtle block max-w-[280px] truncate text-start text-[11px]">
                      {item.slug}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <ContentTypeBadge type={item.type} />
                  </td>
                  <td className="px-4 py-3">
                    <ContentStatusBadge status={item.status} />
                  </td>
                  <td
                    dir="ltr"
                    className="adm-muted px-4 py-3 text-start font-mono text-[11px]"
                    title={item.authorId}
                  >
                    {shortAuthorId(item.authorId)}
                  </td>
                  <td className="adm-muted px-4 py-3 text-[12px]">{formatDateFa(item.updatedAtUtc)}</td>
                  <td className="adm-muted px-4 py-3 text-[12px]">
                    {item.publishedAtUtc ? formatDateFa(item.publishedAtUtc) : "—"}
                  </td>
                  <td className="px-4 py-3">
                    <ContentActions
                      id={item.id}
                      title={item.title}
                      status={item.status}
                      compact
                      publishing={publishingId === item.id}
                      publishError={rowPublishError(item.id)}
                      onPublish={onPublish ? () => onPublish(item) : undefined}
                    />
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </AdminSurface>

      <ul className="space-y-3 md:hidden">
        {items.map((item) => (
          <li key={item.id}>
            <AdminSurface padding="sm" className="space-y-3">
              {selectionEnabled ? (
                <label className="flex items-center gap-2 text-[12px] adm-muted">
                  <input
                    type="checkbox"
                    checked={selectedIds?.has(item.id) ?? false}
                    onChange={() => onToggleSelect?.(item.id)}
                    aria-label={`انتخاب ${item.title}`}
                    className="adm-focus h-4 w-4 rounded border-[var(--adm-border)]"
                  />
                  انتخاب
                </label>
              ) : null}
              <div className="space-y-1">
                <h3 className="adm-text font-bold" title={item.title}>
                  {item.title}
                </h3>
                <p dir="ltr" className="adm-subtle truncate text-start text-[11px]">
                  {item.slug}
                </p>
              </div>
              <div className="flex flex-wrap items-center gap-2">
                <ContentTypeBadge type={item.type} />
                <ContentStatusBadge status={item.status} />
                <span className="adm-subtle text-[11px]">{formatDateFa(item.updatedAtUtc)}</span>
              </div>
              <ContentActions
                id={item.id}
                title={item.title}
                status={item.status}
                compact
                publishing={publishingId === item.id}
                publishError={rowPublishError(item.id)}
                onPublish={onPublish ? () => onPublish(item) : undefined}
              />
            </AdminSurface>
          </li>
        ))}
      </ul>
    </>
  );
}
