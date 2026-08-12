"use client";

import { useState } from "react";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

const SUGGESTIONS = [
  "چطور معماری یک سیستم SaaS را طراحی کنم؟",
  "ساخت API با ASP.NET Core",
  "یادگیری React و Next.js",
  "معماری Microservice",
] as const;

type CommandSearchBoxProps = {
  onOpenPalette?: () => void;
  onSubmit?: (query: string) => void;
  placeholder?: string;
  title?: string;
  className?: string;
};

/**
 * AI prompt interface — Ask HelpDev AI entry for the product experience layer.
 */
export function CommandSearchBox({
  onOpenPalette,
  onSubmit,
  placeholder = "چطور معماری یک سیستم SaaS را طراحی کنم؟",
  title = "Ask HelpDev AI",
  className = "",
}: CommandSearchBoxProps) {
  const [value, setValue] = useState("");

  return (
    <GlassCard strong gradientBorder className={["p-4 sm:p-5", className].join(" ")}>
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <PremiumBadge variant="ai">{title}</PremiumBadge>
          <span className="text-[13px] font-bold text-[color:var(--pub-fg)]">پرسش مهندسی با AI</span>
        </div>
        <button
          type="button"
          onClick={onOpenPalette}
          className="focus-ring inline-flex items-center gap-1.5 rounded-lg border border-[color:var(--pub-glass-border)] bg-white/[0.03] px-2 py-1 text-[11px] font-semibold text-[color:var(--pub-muted)]"
          aria-label="باز کردن پالت فرمان با Ctrl+K"
        >
          <kbd className="rounded border border-white/10 px-1.5 py-0.5 font-mono text-[10px]">Ctrl</kbd>
          <kbd className="rounded border border-white/10 px-1.5 py-0.5 font-mono text-[10px]">K</kbd>
        </button>
      </div>

      <form
        role="search"
        aria-label="Ask HelpDev AI"
        onSubmit={(e) => {
          e.preventDefault();
          const q = value.trim();
          if (q) onSubmit?.(q);
          else onOpenPalette?.();
        }}
      >
        <label className="sr-only" htmlFor="exp-command-input">
          Ask HelpDev AI
        </label>
        <div className="relative">
          <span
            className="pointer-events-none absolute start-3 top-1/2 -translate-y-1/2 text-[color:var(--pub-secondary)]"
            aria-hidden
          >
            ✦
          </span>
          <input
            id="exp-command-input"
            type="search"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            onFocus={onOpenPalette}
            placeholder={placeholder}
            className="focus-ring h-14 w-full rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg-elevated)] pe-4 ps-9 text-[14px] text-[color:var(--pub-fg)] outline-none placeholder:text-[color:var(--pub-muted)] focus:border-[color:color-mix(in_srgb,var(--pub-primary)_50%,transparent)] focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--pub-primary)_18%,transparent),0_0_28px_color-mix(in_srgb,var(--pub-secondary)_18%,transparent)]"
          />
        </div>
      </form>

      <ul className="mt-3 flex flex-col gap-1.5 sm:flex-row sm:flex-wrap" aria-label="نمونه پرسش‌های مهندسی">
        {SUGGESTIONS.map((s) => (
          <li key={s}>
            <button
              type="button"
              className="focus-ring w-full rounded-lg px-2.5 py-1.5 text-start text-[12px] text-[color:var(--pub-muted)] transition hover:bg-white/[0.04] hover:text-[color:var(--pub-fg)] sm:w-auto"
              onClick={() => {
                setValue(s);
                onSubmit?.(s);
              }}
            >
              {s}
            </button>
          </li>
        ))}
      </ul>
    </GlassCard>
  );
}
