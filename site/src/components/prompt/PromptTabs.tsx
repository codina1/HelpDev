"use client";

import { motion } from "framer-motion";
import { PROMPT_DETAIL_TABS, type PromptDetailTabId } from "@/data/prompt-detail";

type PromptTabsProps = {
  active: PromptDetailTabId;
  onChange: (id: PromptDetailTabId) => void;
};

export function PromptTabs({ active, onChange }: PromptTabsProps) {
  return (
    <div
      className="overflow-x-auto rounded-xl border border-white/[0.08] bg-[#0B1224]/85 p-1.5 [scrollbar-width:none] [&::-webkit-scrollbar]:hidden"
      dir="rtl"
    >
      <div className="flex min-w-max gap-1" role="tablist" aria-label="بخش‌های پرامپت">
        {PROMPT_DETAIL_TABS.map((tab) => {
          const isActive = tab.id === active;
          return (
            <button
              key={tab.id}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => onChange(tab.id)}
              className={[
                "relative inline-flex h-10 items-center rounded-lg px-3.5 text-[12.5px] font-bold transition",
                isActive ? "text-white" : "text-[#94A3B8] hover:text-white",
              ].join(" ")}
            >
              {isActive ? (
                <motion.span
                  layoutId="prompt-tab-active"
                  className="absolute inset-0 rounded-lg bg-[#8B5CF6]/25 shadow-[0_0_16px_rgba(139,92,246,0.35)]"
                  transition={{ type: "spring", stiffness: 320, damping: 28 }}
                />
              ) : null}
              <span className="relative z-[1]">{tab.label}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
