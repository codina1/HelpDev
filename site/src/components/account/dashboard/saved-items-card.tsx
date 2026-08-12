"use client";

import { useState } from "react";
import { MOCK_SAVED_ITEMS } from "@/data/account-dashboard";

const TABS = [
  { id: "all", label: "همه" },
  { id: "articles", label: "مقالات" },
  { id: "tools", label: "ابزارها" },
  { id: "programs", label: "برنامه‌ها" },
] as const;

type TabId = (typeof TABS)[number]["id"];

export function SavedItemsCard() {
  const [activeTab, setActiveTab] = useState<TabId>("all");

  const items = MOCK_SAVED_ITEMS.filter(
    (item) => activeTab === "all" || item.tab === activeTab,
  );

  return (
    <section className="dash-card p-5">
      <h2 className="text-[15px] font-bold text-white">ذخیره‌شده‌ها</h2>

      <div className="mt-4 flex flex-wrap gap-1 border-b border-white/[0.06] pb-3">
        {TABS.map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setActiveTab(tab.id)}
            className={[
              "focus-ring rounded-lg px-3 py-1.5 text-[12px] font-semibold transition-colors",
              activeTab === tab.id
                ? "bg-violet-500/15 text-violet-200"
                : "text-slate-500 hover:text-slate-300",
            ].join(" ")}
          >
            {tab.label}
          </button>
        ))}
      </div>

      <ul className="mt-3 space-y-2">
        {items.map((item) => (
          <li
            key={item.id}
            className="flex items-center gap-3 rounded-xl border border-white/[0.06] bg-white/[0.02] px-3 py-2.5 transition-colors hover:border-violet-500/20 hover:bg-violet-500/[0.04]"
          >
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-white/[0.05] text-lg">
              {item.thumb}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate text-[13px] font-semibold text-white">{item.title}</p>
              <p className="text-[11px] text-slate-500">
                {item.category} · {item.time}
              </p>
            </div>
            <button
              type="button"
              className="focus-ring rounded-lg p-2 text-slate-500 transition-colors hover:bg-red-500/10 hover:text-red-300"
              aria-label="حذف"
            >
              🗑️
            </button>
          </li>
        ))}
      </ul>
    </section>
  );
}
