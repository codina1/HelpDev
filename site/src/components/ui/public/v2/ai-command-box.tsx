"use client";

import { useState } from "react";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

type AICommandBoxProps = {
  onOpenPalette?: () => void;
  onSubmit?: (query: string) => void;
  placeholder?: string;
  className?: string;
};

const SUGGESTIONS = [
  "چگونه یک معماری Microservice طراحی کنم؟",
  "تفاوت Cursor و GitHub Copilot چیست؟",
  "مسیر یادگیری Frontend Engineer",
] as const;

/**
 * Premium Ask HelpDev command surface near the hero.
 */
export function AICommandBox({
  onOpenPalette,
  onSubmit,
  placeholder = "از HelpDev بپرس...",
  className = "",
}: AICommandBoxProps) {
  const [value, setValue] = useState("");

  return (
    <GlassCard strong gradientBorder className={["p-4 sm:p-5", className].join(" ")}>
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <div className="flex items-center gap-2">
          <PremiumBadge variant="ai">Ask HelpDev</PremiumBadge>
          <span className="text-[13px] font-bold text-[color:var(--pub-fg)]">دستور AI</span>
        </div>
        <button
          type="button"
          onClick={onOpenPalette}
          className="focus-ring inline-flex items-center gap-1.5 rounded-lg border border-[color:var(--pub-glass-border)] bg-white/[0.03] px-2 py-1 text-[11px] font-semibold text-[color:var(--pub-muted)] hover:text-[color:var(--pub-fg)]"
          aria-label="باز کردن پالت جستجو با Ctrl+K"
        >
          <kbd className="rounded border border-white/10 px-1.5 py-0.5 font-mono text-[10px]">Ctrl</kbd>
          <kbd className="rounded border border-white/10 px-1.5 py-0.5 font-mono text-[10px]">K</kbd>
        </button>
      </div>

      <form
        role="search"
        aria-label="پرسش از HelpDev"
        onSubmit={(event) => {
          event.preventDefault();
          const q = value.trim();
          if (q) onSubmit?.(q);
          else onOpenPalette?.();
        }}
      >
        <label className="sr-only" htmlFor="ai-command-input">
          پرسش مهندسی
        </label>
        <div className="relative">
          <span
            className="pointer-events-none absolute inset-y-0 start-3.5 flex items-center text-[color:var(--pub-secondary)]"
            aria-hidden
          >
            <SparkIcon />
          </span>
          <input
            id="ai-command-input"
            type="search"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            onFocus={onOpenPalette}
            placeholder={placeholder}
            className="focus-ring h-14 w-full rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg-elevated)] pe-4 ps-11 text-[14px] text-[color:var(--pub-fg)] outline-none placeholder:text-[color:var(--pub-muted)] focus:border-[color:color-mix(in_srgb,var(--pub-primary)_50%,transparent)] focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--pub-primary)_18%,transparent)]"
          />
        </div>
      </form>

      <ul className="mt-3 flex flex-col gap-1.5 sm:flex-row sm:flex-wrap" aria-label="پیشنهاد پرسش">
        {SUGGESTIONS.map((suggestion) => (
          <li key={suggestion}>
            <button
              type="button"
              className="focus-ring w-full rounded-lg border border-transparent px-2.5 py-1.5 text-start text-[12px] text-[color:var(--pub-muted)] transition hover:border-[color:var(--pub-glass-border)] hover:bg-white/[0.04] hover:text-[color:var(--pub-fg)] sm:w-auto"
              onClick={() => {
                setValue(suggestion);
                onSubmit?.(suggestion);
              }}
            >
              {suggestion}
            </button>
          </li>
        ))}
      </ul>
    </GlassCard>
  );
}

function SparkIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M12 3l1.5 5.5L19 10l-5.5 1.5L12 17l-1.5-5.5L5 10l5.5-1.5L12 3z" />
    </svg>
  );
}
