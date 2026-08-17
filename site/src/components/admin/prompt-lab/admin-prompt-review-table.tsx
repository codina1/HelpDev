import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { WriterPromptStatusBadge } from "@/components/admin/prompt-lab/writer-prompt-status-badge";
import { shortAuthorId } from "@/lib/admin/content/content-mappers";
import { labelForWriterPromptCategory } from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import type { AdminPromptReviewItem } from "@/lib/admin/prompt-lab/admin-prompt-review-types";

type AdminPromptReviewTableProps = {
  items: AdminPromptReviewItem[];
  showActions?: boolean;
  busyId?: string | null;
  onApprove?: (item: AdminPromptReviewItem) => void;
  onReject?: (item: AdminPromptReviewItem) => void;
};

/** Admin review list: title, author, category, preview, and pending actions. */
export function AdminPromptReviewTable({
  items,
  showActions = false,
  busyId = null,
  onApprove,
  onReject,
}: AdminPromptReviewTableProps) {
  return (
    <>
      <AdminSurface padding="none" className="hidden md:block overflow-x-auto">
        <table className="w-full min-w-[720px] text-start text-[13px]">
          <thead>
            <tr className="adm-border-b adm-subtle text-[11px]">
              <th className="px-4 py-3 text-start font-semibold">عنوان</th>
              <th className="px-4 py-3 text-start font-semibold">نویسنده</th>
              <th className="px-4 py-3 text-start font-semibold">دسته‌بندی</th>
              <th className="px-4 py-3 text-start font-semibold">پیش‌نمایش</th>
              {showActions ? (
                <th className="px-4 py-3 text-start font-semibold">عملیات</th>
              ) : (
                <th className="px-4 py-3 text-start font-semibold">وضعیت</th>
              )}
            </tr>
          </thead>
          <tbody className="adm-divide">
            {items.map((item) => (
              <tr key={item.id} className="adm-hover align-top">
                <td className="px-4 py-3">
                  <span className="adm-text block max-w-[240px] truncate font-semibold" title={item.title}>
                    {item.title}
                  </span>
                  <span className="adm-subtle mt-0.5 block truncate text-[11px]" dir="ltr">
                    {item.slug}
                  </span>
                </td>
                <td className="adm-muted px-4 py-3 font-mono text-[12px]" dir="ltr">
                  {shortAuthorId(item.authorId)}
                </td>
                <td className="adm-muted px-4 py-3">
                  {labelForWriterPromptCategory(item.categoryName)}
                </td>
                <td className="adm-muted px-4 py-3">
                  <p className="max-w-[280px] line-clamp-3 text-[12px] leading-5">{item.preview || "—"}</p>
                  {item.rejectionReason ? (
                    <p className="mt-1 text-[11px] text-[var(--adm-danger)]">دلیل رد: {item.rejectionReason}</p>
                  ) : null}
                </td>
                <td className="px-4 py-3">
                  {showActions ? (
                    <RowActions item={item} busy={busyId === item.id} onApprove={onApprove} onReject={onReject} />
                  ) : (
                    <WriterPromptStatusBadge status={item.status} />
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </AdminSurface>

      <ul className="space-y-3 md:hidden">
        {items.map((item) => (
          <li key={item.id}>
            <AdminSurface className="space-y-3">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <p className="adm-text font-semibold">{item.title}</p>
                <WriterPromptStatusBadge status={item.status} />
              </div>
              <dl className="grid grid-cols-2 gap-2 text-[12px]">
                <div>
                  <dt className="adm-subtle">نویسنده</dt>
                  <dd className="adm-muted font-mono" dir="ltr">
                    {shortAuthorId(item.authorId)}
                  </dd>
                </div>
                <div>
                  <dt className="adm-subtle">دسته‌بندی</dt>
                  <dd className="adm-muted">{labelForWriterPromptCategory(item.categoryName)}</dd>
                </div>
              </dl>
              <p className="adm-muted text-[12px] leading-5">{item.preview || "—"}</p>
              {item.rejectionReason ? (
                <p className="text-[12px] text-[var(--adm-danger)]">دلیل رد: {item.rejectionReason}</p>
              ) : null}
              {showActions ? (
                <RowActions item={item} busy={busyId === item.id} onApprove={onApprove} onReject={onReject} />
              ) : null}
            </AdminSurface>
          </li>
        ))}
      </ul>
    </>
  );
}

function RowActions({
  item,
  busy,
  onApprove,
  onReject,
}: {
  item: AdminPromptReviewItem;
  busy: boolean;
  onApprove?: (item: AdminPromptReviewItem) => void;
  onReject?: (item: AdminPromptReviewItem) => void;
}) {
  return (
    <div className="flex flex-wrap gap-2">
      <button
        type="button"
        disabled={busy}
        onClick={() => onApprove?.(item)}
        className="adm-btn adm-btn-primary adm-focus inline-flex items-center gap-1.5"
      >
        <AdminIcon name="check" size={14} />
        تأیید
      </button>
      <button
        type="button"
        disabled={busy}
        onClick={() => onReject?.(item)}
        className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5 text-[var(--adm-danger)]"
      >
        <AdminIcon name="close" size={14} />
        رد
      </button>
    </div>
  );
}
