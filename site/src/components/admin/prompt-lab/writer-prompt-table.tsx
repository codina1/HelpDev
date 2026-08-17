import { AdminSurface } from "@/components/admin/page/admin-surface";
import { formatDateFa } from "@/lib/admin/content/content-mappers";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type { WriterPromptListItem } from "@/lib/admin/prompt-lab/writer-prompt-types";
import { WriterPromptStatusBadge } from "@/components/admin/prompt-lab/writer-prompt-status-badge";

type WriterPromptTableProps = {
  items: WriterPromptListItem[];
};

/** Responsive writer prompt list: table on desktop, stacked cards on mobile. */
export function WriterPromptTable({ items }: WriterPromptTableProps) {
  return (
    <>
      <AdminSurface padding="none" className="hidden md:block overflow-x-auto">
        <table className="w-full min-w-[640px] text-start text-[13px]">
          <thead>
            <tr className="adm-border-b adm-subtle text-[11px]">
              <th className="px-4 py-3 text-start font-semibold">عنوان</th>
              <th className="px-4 py-3 text-start font-semibold">وضعیت</th>
              <th className="px-4 py-3 text-start font-semibold">تاریخ ایجاد</th>
              <th className="px-4 py-3 text-start font-semibold">کپی</th>
              <th className="px-4 py-3 text-start font-semibold">بازدید</th>
            </tr>
          </thead>
          <tbody className="adm-divide">
            {items.map((item) => (
              <tr key={item.id} className="adm-hover align-top">
                <td className="px-4 py-3">
                  <span
                    className="adm-text block max-w-[320px] truncate font-semibold"
                    title={item.title}
                  >
                    {item.title}
                  </span>
                  <span className="adm-subtle mt-0.5 block truncate text-[11px]" dir="ltr">
                    {item.slug}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <WriterPromptStatusBadge status={item.status} />
                </td>
                <td className="adm-muted px-4 py-3 text-[12px]">{formatDateFa(item.createdAt)}</td>
                <td className="adm-muted px-4 py-3 tabular-nums">{formatNumberFa(item.copyCount)}</td>
                <td className="adm-muted px-4 py-3 tabular-nums">{formatNumberFa(item.views)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </AdminSurface>

      <ul className="space-y-3 md:hidden">
        {items.map((item) => (
          <li key={item.id}>
            <AdminSurface className="space-y-2">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <p className="adm-text font-semibold">{item.title}</p>
                <WriterPromptStatusBadge status={item.status} />
              </div>
              <p className="adm-subtle truncate text-[11px]" dir="ltr">
                {item.slug}
              </p>
              <dl className="grid grid-cols-2 gap-2 text-[12px]">
                <div>
                  <dt className="adm-subtle">تاریخ ایجاد</dt>
                  <dd className="adm-muted">{formatDateFa(item.createdAt)}</dd>
                </div>
                <div>
                  <dt className="adm-subtle">کپی / بازدید</dt>
                  <dd className="adm-muted tabular-nums">
                    {formatNumberFa(item.copyCount)} / {formatNumberFa(item.views)}
                  </dd>
                </div>
              </dl>
            </AdminSurface>
          </li>
        ))}
      </ul>
    </>
  );
}
