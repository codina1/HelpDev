"use client";

import { useState } from "react";
import { AnimatePresence, motion } from "framer-motion";
import type { RoadmapDetailModel, RoadmapTimelineStep } from "@/data/roadmap-detail";

export function RoadmapAbout({ model }: { model: RoadmapDetailModel }) {
  return (
    <section id="intro" className="scroll-mt-28 space-y-4">
      <div className="rounded-2xl border border-[rgba(139,92,246,0.18)] bg-[#10182D]/9 p-5 sm:p-6">
        <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">درباره این نقشه راه</h2>
        <p className="mt-3 text-[14.5px] leading-8 text-[#94A3B8]">{model.about}</p>
      </div>

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        {model.features.map((feature, index) => (
          <motion.article
            key={feature.id}
            initial={{ opacity: 0, y: 8 }}
            whileInView={{ opacity: 1, y: 0 }}
            viewport={{ once: true }}
            transition={{ delay: index * 0.04 }}
            whileHover={{ y: -2 }}
            className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4 shadow-[0_0_18px_rgba(139,92,246,0.08)]"
          >
            <span className="inline-flex h-9 w-9 items-center justify-center rounded-xl bg-[#2563EB]/20 text-[#93C5FD]">
              {feature.icon === "path" ? "◎" : feature.icon === "project" ? "▣" : feature.icon === "book" ? "▤" : "★"}
            </span>
            <h3 className="mt-3 text-[14px] font-extrabold text-white">{feature.title}</h3>
            <p className="mt-1.5 text-[12.5px] leading-6 text-[#94A3B8]">{feature.description}</p>
          </motion.article>
        ))}
      </div>
    </section>
  );
}

export function RoadmapTimeline({ steps }: { steps: RoadmapTimelineStep[] }) {
  const [openId, setOpenId] = useState(steps[0]?.id ?? "");

  return (
    <section id="steps" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">مراحل نقشه راه</h2>
      <div className="relative mt-4 space-y-3">
        <span
          className="pointer-events-none absolute top-3 bottom-3 end-[1.15rem] w-px bg-gradient-to-b from-[#8B5CF6]/70 via-[#8B5CF6]/25 to-transparent"
          aria-hidden
        />
        {steps.map((step) => {
          const open = openId === step.id;
          return (
            <article
              key={step.id}
              className={[
                "relative rounded-2xl border bg-[#10182D]/9 transition",
                open
                  ? "border-[rgba(139,92,246,0.4)] shadow-[0_0_24px_rgba(139,92,246,0.15)]"
                  : "border-white/[0.08] hover:border-white/[0.14]",
              ].join(" ")}
            >
              <button
                type="button"
                onClick={() => setOpenId(open ? "" : step.id)}
                className="flex w-full items-center gap-3 px-4 py-3.5 text-start"
              >
                <span
                  className={[
                    "relative z-[1] inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-[13px] font-extrabold text-white",
                    step.tone,
                  ].join(" ")}
                >
                  {step.order.toLocaleString("fa-IR")}
                </span>
                <span className="min-w-0 flex-1">
                  <span className="block text-[14.5px] font-extrabold text-white">{step.title}</span>
                  <span className="mt-0.5 block text-[11.5px] text-[#64748B]">
                    {step.lessonsLabel} · {step.durationLabel}
                    {step.completed ? " · تکمیل شده" : ""}
                  </span>
                </span>
                <span className="text-[12px] font-bold text-[#94A3B8]">{open ? "−" : "+"}</span>
              </button>

              <AnimatePresence initial={false}>
                {open ? (
                  <motion.div
                    initial={{ height: 0, opacity: 0 }}
                    animate={{ height: "auto", opacity: 1 }}
                    exit={{ height: 0, opacity: 0 }}
                    transition={{ duration: 0.22 }}
                    className="overflow-hidden"
                  >
                    <div className="border-t border-white/[0.06] px-4 pb-4 pt-3">
                      <p className="text-[13.5px] leading-7 text-[#94A3B8]">{step.description}</p>
                      <div className="mt-3 h-1.5 overflow-hidden rounded-full bg-white/[0.06]">
                        <div
                          className="h-full rounded-full bg-gradient-to-l from-[#8B5CF6] to-[#22D3EE]"
                          style={{ width: `${step.progress}%` }}
                        />
                      </div>
                      <p className="mt-1.5 text-[11px] font-semibold text-[#64748B]">
                        پیشرفت: {step.progress.toLocaleString("fa-IR")}٪
                      </p>
                      <div className="mt-3 flex flex-wrap gap-1.5">
                        {step.topics.map((topic) => (
                          <span
                            key={topic.id}
                            className="rounded-lg border border-white/[0.08] bg-[#070B18] px-2.5 py-1 text-[11.5px] font-bold text-[#CBD5E1]"
                          >
                            {topic.title}
                          </span>
                        ))}
                      </div>
                    </div>
                  </motion.div>
                ) : null}
              </AnimatePresence>
            </article>
          );
        })}
      </div>
    </section>
  );
}
