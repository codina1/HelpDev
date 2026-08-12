"use client";

import Link from "next/link";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type { ContentPerformanceDto } from "@/lib/admin/analytics/content/content-analytics-types";
import { AdminSurface } from "@/components/admin/page/admin-surface";

type TopContentTableProps = {
  items: ContentPerformanceDto[];
};

export function TopContentTable({ items }: TopContentTableProps) {
  if (items.length === 0) {
    return (
      <p className="adm-subtle text-[13px]">
        هنوز بازدیدی با subject محتوا در بازه انتخاب‌شده ثبت نشده است.
      </p>
    );
  }

  return (
    <AdminSurface padding="none" className="overflow-x-auto">
      <table className="adm-table w-full min-w-[32rem] text-[12px]">
        <thead>
          <tr>
            <th className="text-start">عنوان</th>
            <th className="text-start">بازدید</th>
            <th className="text-start">عمل</th>
          </tr>
        </thead>
        <tbody>
          {items.map((item) => (
            <tr key={item.contentId}>
              <td className="font-semibold">{item.title}</td>
              <td className="tabular-nums">{formatNumberFa(item.views)}</td>
              <td>
                <Link
                  href={`${ADMIN_ROUTES.content}/${encodeURIComponent(item.contentId)}/analytics`}
                  className="adm-link text-[11px] font-semibold"
                >
                  جزئیات
                </Link>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </AdminSurface>
  );
}
