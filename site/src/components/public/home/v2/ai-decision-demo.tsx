"use client";

import { useEffect, useState } from "react";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { Button } from "@/components/ui/ds/button";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PublicSection } from "@/components/ui/public/v2";
import { AI_DECISION_DEMO_STEPS } from "@/lib/public/intelligence-showcase";

const DEMO_PROMPT = "چطور یک SaaS چند مستاجری با ASP.NET Core طراحی کنم؟";

/**
 * Interactive AI Decision Demo — sequential pipeline after analyze click.
 */
export function AiDecisionDemo() {
  const [running, setRunning] = useState(false);
  const [visibleCount, setVisibleCount] = useState(0);

  useEffect(() => {
    if (!running) return;
    setVisibleCount(0);
    const reduce =
      typeof window !== "undefined" &&
      window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (reduce) {
      setVisibleCount(AI_DECISION_DEMO_STEPS.length);
      return;
    }

    let step = 0;
    const id = window.setInterval(() => {
      step += 1;
      setVisibleCount(step);
      if (step >= AI_DECISION_DEMO_STEPS.length) {
        window.clearInterval(id);
      }
    }, 700);
    return () => window.clearInterval(id);
  }, [running]);

  return (
    <PublicSection className="ix-reveal" aria-labelledby="ai-decision-title">
      <PremiumSectionHeader
        eyebrow="AI Decision"
        title="دمو تصمیم‌گیری مهندسی"
        description="از سؤال تا مسیر اجرا — شبیه‌سازی گردش‌کار هوش مهندسی HelpDev"
        titleId="ai-decision-title"
        icon={<span aria-hidden>✦</span>}
      />

      <GlassCard strong gradientBorder className="relative overflow-hidden p-5 sm:p-8">
        <div
          className="pointer-events-none absolute -end-20 -top-20 h-64 w-64 rounded-full bg-[color:var(--pub-primary)]/25 blur-3xl"
          aria-hidden
        />
        <div
          className="pointer-events-none absolute -bottom-24 -start-16 h-52 w-52 rounded-full bg-[color:var(--pub-secondary)]/20 blur-3xl"
          aria-hidden
        />

        <Badge variant="ai" className="mb-4">
          Interactive Demo
        </Badge>
        <h3 className="max-w-2xl text-xl font-extrabold leading-9 text-[color:var(--pub-fg)] sm:text-2xl sm:leading-10">
          با هوش مهندسی HelpDev، از سوال تا مسیر اجرا
        </h3>

        <label className="mt-5 block text-[12px] font-bold text-[color:var(--pub-muted)]" htmlFor="ai-decision-input">
          نمونه پرسش مهندسی
        </label>
        <input
          id="ai-decision-input"
          readOnly
          value={DEMO_PROMPT}
          className="mt-2 h-12 w-full rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg-elevated)] px-4 text-[13px] text-[color:var(--pub-fg)] outline-none sm:text-[14px]"
        />

        <div className="mt-4 flex flex-wrap gap-3">
          <Button
            type="button"
            className="ix-btn-glow"
            onClick={() => {
              setRunning(false);
              window.setTimeout(() => setRunning(true), 30);
            }}
          >
            تحلیل توسط HelpDev AI
          </Button>
          {running ? (
            <Button type="button" variant="secondary" onClick={() => { setRunning(false); setVisibleCount(0); }}>
              بازنشانی
            </Button>
          ) : null}
        </div>

        {running ? (
          <ol className="mt-7 space-y-3" aria-live="polite" aria-label="خط لوله تصمیم‌گیری">
            {AI_DECISION_DEMO_STEPS.map((step, index) => {
              const shown = index < visibleCount;
              const current = index === visibleCount - 1;
              const done = index < visibleCount - 1 || (visibleCount === AI_DECISION_DEMO_STEPS.length && index === visibleCount - 1);
              return (
                <li
                  key={step.id}
                  className={[
                    "ix-decision-step flex items-start gap-3 rounded-xl border px-3 py-3 transition",
                    shown
                      ? "translate-y-0 opacity-100"
                      : "pointer-events-none translate-y-2 opacity-0",
                    current
                      ? "border-[color:color-mix(in_srgb,var(--pub-primary)_50%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_12%,transparent)] shadow-[0_0_22px_var(--pub-glow)]"
                      : shown
                        ? "border-[color:var(--pub-glass-border)] bg-white/[0.03]"
                        : "border-transparent",
                  ].join(" ")}
                >
                  <span
                    className={[
                      "mt-0.5 flex h-7 w-7 shrink-0 items-center justify-center rounded-full border text-[11px] font-extrabold",
                      done || shown
                        ? "border-transparent bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-white"
                        : "border-[color:var(--pub-glass-border)] text-[color:var(--pub-muted)]",
                    ].join(" ")}
                  >
                    {shown ? "✓" : step.code}
                  </span>
                  <div>
                    <p className="text-[13px] font-extrabold text-[color:var(--pub-fg)]">
                      Step {index + 1}: {step.label}
                      {shown ? " ✓" : ""}
                    </p>
                    <p className="mt-0.5 text-[12px] text-[color:var(--pub-muted)]">{step.titleFa} — {step.detail}</p>
                  </div>
                </li>
              );
            })}
          </ol>
        ) : (
          <p className="mt-6 text-[13px] leading-7 text-[color:var(--pub-muted)]">
            روی «تحلیل توسط HelpDev AI» بزنید تا مراحل تصمیم‌گیری به‌صورت متوالی نمایش داده شوند.
          </p>
        )}
      </GlassCard>
    </PublicSection>
  );
}
