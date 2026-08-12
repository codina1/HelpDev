"use client";

import { computeStatistics } from "@/lib/admin/content/content-analyzer";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";

/** Factual content statistics (words, characters, reading estimate, structure). */
export function ContentStatisticsCard({ body }: { body: string }) {
  const stats = computeStatistics(body);

  const rows: Array<{ label: string; value: string }> = [
    { label: "کلمات", value: formatNumberFa(stats.words) },
    { label: "نویسه‌ها", value: formatNumberFa(stats.characters) },
    { label: "عناوین", value: formatNumberFa(stats.headings) },
    { label: "بلوک‌های کد", value: formatNumberFa(stats.codeBlocks) },
    { label: "پیوندها", value: formatNumberFa(stats.links) },
  ];

  return (
    <div className="space-y-3">
      <h3 className="adm-text text-[12px] font-bold">آمار محتوا</h3>
      <dl className="grid grid-cols-2 gap-2">
        {rows.map((row) => (
          <div
            key={row.label}
            className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-2.5"
          >
            <dt className="adm-subtle text-[11px]">{row.label}</dt>
            <dd className="adm-text mt-0.5 text-[16px] font-black tabular-nums">{row.value}</dd>
          </div>
        ))}
      </dl>
      <p className="adm-subtle text-[11px]">
        زمان تقریبی مطالعه: {formatNumberFa(stats.readingMinutes)} دقیقه (تخمینی، بر پایه ۲۰۰ کلمه در دقیقه).
      </p>
    </div>
  );
}
