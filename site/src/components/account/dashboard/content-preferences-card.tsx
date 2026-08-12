"use client";

import { useState } from "react";
import { CATEGORY_PREFERENCES } from "@/data/account-dashboard";

const DISPLAY_MODES = ["برای من", "پرطرفدارترین", "جدیدترین"] as const;

export function ContentPreferencesCard() {
  const [selected, setSelected] = useState<string[]>(["dotnet", "ai", "devops"]);
  const [displayMode, setDisplayMode] = useState<(typeof DISPLAY_MODES)[number]>("برای من");

  function toggleCategory(id: string) {
    setSelected((current) =>
      current.includes(id) ? current.filter((item) => item !== id) : [...current, id],
    );
  }

  return (
    <section className="dash-card p-5">
      <h2 className="text-[15px] font-bold text-white">ترجیحات محتوا</h2>

      <p className="mt-4 text-[11px] font-bold text-slate-500">دسته‌بندی‌ها</p>
      <div className="mt-3 grid grid-cols-2 gap-2 sm:grid-cols-3">
        {CATEGORY_PREFERENCES.map((category) => {
          const active = selected.includes(category.id);
          return (
            <button
              key={category.id}
              type="button"
              onClick={() => toggleCategory(category.id)}
              className={[
                "focus-ring rounded-xl border px-3 py-2.5 text-[12px] font-bold transition-all",
                active
                  ? `border-transparent bg-gradient-to-l ${category.color} text-white shadow-lg`
                  : "border-white/10 bg-white/[0.03] text-slate-400 hover:border-white/20",
              ].join(" ")}
            >
              {active && <span className="me-1">✓</span>}
              {category.label}
            </button>
          );
        })}
      </div>

      <p className="mt-5 text-[11px] font-bold text-slate-500">نوع نمایش</p>
      <div className="mt-2 flex flex-wrap gap-2">
        {DISPLAY_MODES.map((mode) => (
          <button
            key={mode}
            type="button"
            onClick={() => setDisplayMode(mode)}
            className={[
              "focus-ring rounded-lg px-3 py-1.5 text-[12px] font-semibold transition-colors",
              displayMode === mode
                ? "bg-violet-500/20 text-violet-200"
                : "bg-white/[0.04] text-slate-400 hover:text-white",
            ].join(" ")}
          >
            {mode}
          </button>
        ))}
      </div>

      <button
        type="button"
        className="focus-ring mt-5 w-full rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 py-2.5 text-[12px] font-bold text-white"
      >
        ذخیره تنظیمات
      </button>
    </section>
  );
}
