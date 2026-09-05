"use client";

import { motion } from "framer-motion";
import type { PromptUsageStep } from "@/data/prompt-detail";

const ICONS: Record<PromptUsageStep["icon"], string> = {
  copy: "⧉",
  input: "⌘",
  detail: "✎",
  check: "✓",
};

export function PromptUsageGuide({ steps }: { steps: PromptUsageStep[] }) {
  return (
    <section id="usage" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">
        نحوه استفاده از این پرامپت
      </h2>
      <div className="mt-4 grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {steps.map((step, index) => (
          <motion.article
            key={step.id}
            initial={{ opacity: 0, y: 10 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: index * 0.05 }}
            whileHover={{ y: -3 }}
            className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4 shadow-[0_0_18px_rgba(139,92,246,0.08)]"
          >
            <div className="flex items-center gap-2.5">
              <span className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-[#8B5CF6]/2 text-[13px] font-extrabold text-[#E9D5FF] shadow-[0_0_12px_rgba(139,92,246,0.35)]">
                {(index + 1).toLocaleString("fa-IR")}
              </span>
              <span className="text-[16px] text-[#22D3EE]" aria-hidden>
                {ICONS[step.icon]}
              </span>
            </div>
            <h3 className="mt-3 text-[13.5px] font-extrabold text-white">{step.title}</h3>
            <p className="mt-1.5 text-[12px] leading-6 text-[#94A3B8]">{step.description}</p>
          </motion.article>
        ))}
      </div>
    </section>
  );
}
