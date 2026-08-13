"use client";

import { useEffect, useState } from "react";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PublicSection } from "@/components/ui/public/v2";
import { AI_WORKFLOW_STEPS } from "@/lib/public/intelligence-showcase";

/**
 * AI Workflow Demo V2 — horizontal timeline with active glow and progress line.
 */
export function AiWorkflowDemo() {
  const [active, setActive] = useState(0);

  useEffect(() => {
    const reduce =
      typeof window !== "undefined" &&
      window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduce) return;

    const id = window.setInterval(() => {
      setActive((prev) => (prev + 1) % AI_WORKFLOW_STEPS.length);
    }, 2400);
    return () => window.clearInterval(id);
  }, []);

  const progress = ((active + 1) / AI_WORKFLOW_STEPS.length) * 100;

  return (
    <PublicSection className="ix-reveal" aria-labelledby="ai-workflow-title">
      <PremiumSectionHeader
        eyebrow="Workflow V2"
        title="AI Workflow Demo"
        description="خط زمانی تصمیم‌گیری — از درک مسئله تا راه‌حل"
        titleId="ai-workflow-title"
        icon={<span aria-hidden>→</span>}
      />

      <GlassCard strong gradientBorder className="overflow-hidden p-4 sm:p-6">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-2">
          <Badge variant="ai">Horizontal timeline</Badge>
          <p className="text-[12px] text-[color:var(--pub-muted)]" aria-live="polite">
            Active:{" "}
            <span className="font-bold text-[color:var(--pub-fg)]">
              {AI_WORKFLOW_STEPS[active]?.label}
            </span>
          </p>
        </div>

        <div className="relative">
          <div className="absolute start-0 end-0 top-5 hidden h-0.5 rounded-full bg-white/[0.08] md:block" aria-hidden />
          <div
            className="absolute start-0 top-5 hidden h-0.5 rounded-full bg-gradient-to-l from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] transition-[width] duration-500 md:block"
            style={{ width: `${progress}%` }}
            aria-hidden
          />

          <ol className="grid gap-3 md:grid-cols-5" aria-label="تایم‌لاین گردش‌کار AI V2">
            {AI_WORKFLOW_STEPS.map((step, index) => {
              const isActive = index === active;
              const isDone = index < active;
              return (
                <li key={step.id}>
                  <button
                    type="button"
                    onClick={() => setActive(index)}
                    className={[
                      "ix-card-lift focus-ring flex w-full flex-col gap-2 rounded-2xl border p-3 text-start",
                      isActive
                        ? "border-[color:color-mix(in_srgb,var(--pub-primary)_55%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_14%,transparent)] shadow-[0_0_28px_var(--pub-glow)]"
                        : isDone
                          ? "border-[color:color-mix(in_srgb,var(--pub-secondary)_35%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-secondary)_8%,transparent)]"
                          : "border-[color:var(--pub-glass-border)] bg-white/[0.02]",
                    ].join(" ")}
                    aria-current={isActive ? "step" : undefined}
                  >
                    <span
                      className={[
                        "relative z-[1] flex h-10 w-10 items-center justify-center rounded-full border text-[12px] font-extrabold",
                        isActive
                          ? "border-transparent bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-white"
                          : "border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg)] text-[color:var(--pub-muted)]",
                      ].join(" ")}
                    >
                      {step.code}
                    </span>
                    <span className="text-[12px] font-extrabold leading-5 text-[color:var(--pub-fg)] sm:text-[13px]">
                      {step.label}
                    </span>
                    <span className="text-[11px] leading-5 text-[color:var(--pub-muted)]">
                      {step.titleFa}
                    </span>
                  </button>
                </li>
              );
            })}
          </ol>
        </div>

        <div className="mt-5 h-1 overflow-hidden rounded-full bg-white/[0.06] md:hidden">
          <div
            className="h-full rounded-full bg-gradient-to-l from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] transition-[width] duration-500"
            style={{ width: `${progress}%` }}
          />
        </div>
      </GlassCard>
    </PublicSection>
  );
}
