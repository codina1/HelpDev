"use client";

import type { PromptVersionRow } from "@/data/prompt-detail";

export function PromptVersionHistory({ versions }: { versions: PromptVersionRow[] }) {
  return (
    <section id="changelog" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">
        نسخه‌ها و تاریخچه تغییرات
      </h2>

      <div className="mt-3 overflow-hidden rounded-2xl border border-white/[0.08] bg-[#0B1224]/95">
        <div className="overflow-x-auto">
          <table className="w-full min-w-[480px] border-collapse text-start" dir="rtl">
            <thead>
              <tr className="border-b border-white/[0.08] text-[12px] text-[#64748B]">
                <th className="px-4 py-3 font-bold">نسخه</th>
                <th className="px-4 py-3 font-bold">تاریخ</th>
                <th className="px-4 py-3 font-bold">تغییرات</th>
              </tr>
            </thead>
            <tbody>
              {versions.map((row) => (
                <tr key={row.id} className="border-b border-white/[0.05] last:border-0">
                  <td className="px-4 py-3">
                    <span className="inline-flex items-center gap-2 text-[13px] font-extrabold text-white">
                      {row.version}
                      {row.isLatest ? (
                        <span className="rounded-md bg-[#8B5CF6]/2 px-1.5 py-0.5 text-[10px] font-bold text-[#E9D5FF]">
                          آخرین نسخه
                        </span>
                      ) : null}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-[12.5px] text-[#94A3B8]">{row.dateLabel}</td>
                  <td className="px-4 py-3 text-[12.5px] text-[#CBD5E1]">{row.summary}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </section>
  );
}
