"use client";

import { useState } from "react";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { Button } from "@/components/ui/ds/button";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PublicSection } from "@/components/ui/public/v2";
import { DEVELOPER_IDENTITY_OPTIONS } from "@/lib/public/intelligence-showcase";

/**
 * Developer Identity — interactive path selector with profile preview chrome.
 */
export function DeveloperIdentitySection() {
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [analyzed, setAnalyzed] = useState(false);

  const selected = DEVELOPER_IDENTITY_OPTIONS.find((o) => o.id === selectedId) ?? null;

  return (
    <PublicSection className="ix-reveal" aria-labelledby="dev-identity-title">
      <PremiumSectionHeader
        eyebrow="Identity"
        title="نقش خود را پیدا کنید"
        description="نقش خود را انتخاب کنید و یک پیش‌نمای پروفایل دریافت کنید"
        titleId="dev-identity-title"
        href="/learning"
        ctaLabel="هاب یادگیری"
        icon={<span aria-hidden>◎</span>}
      />

      <GlassCard strong gradientBorder className="p-4 sm:p-6">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4" role="listbox" aria-label="انتخاب نقش">
          {DEVELOPER_IDENTITY_OPTIONS.map((option) => {
            const active = selectedId === option.id;
            return (
              <button
                key={option.id}
                type="button"
                role="option"
                aria-selected={active}
                onClick={() => {
                  setSelectedId(option.id);
                  setAnalyzed(false);
                }}
                className={[
                  "ix-card-lift focus-ring rounded-2xl border p-4 text-start",
                  active
                    ? "border-[color:color-mix(in_srgb,var(--pub-primary)_55%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_14%,transparent)] shadow-[0_0_24px_var(--pub-glow)]"
                    : "border-[color:var(--pub-glass-border)] bg-white/[0.02]",
                ].join(" ")}
              >
                <Badge variant={active ? "ai" : "outline"}>{option.label}</Badge>
                <p className="mt-3 text-[13px] font-bold text-[color:var(--pub-fg)]">{option.profile}</p>
              </button>
            );
          })}
        </div>

        <div className="mt-5 flex flex-wrap gap-3">
          <Button
            type="button"
            className="ix-btn-glow"
            disabled={!selectedId}
            onClick={() => setAnalyzed(true)}
          >
            Analyze My Path
          </Button>
        </div>

        {analyzed && selected ? (
          <div
            className="ix-decision-step mt-6 rounded-2xl border border-[color:color-mix(in_srgb,var(--pub-secondary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-secondary)_8%,transparent)] p-4 sm:p-5"
            aria-live="polite"
          >
            <Badge variant="ai" className="mb-3">
              Engineering Profile
            </Badge>
            <dl className="grid gap-3 sm:grid-cols-3">
              <div>
                <dt className="text-[11px] font-bold text-[color:var(--pub-muted)]">Engineering Profile</dt>
                <dd className="mt-1 text-[15px] font-extrabold text-[color:var(--pub-fg)]">{selected.profile}</dd>
              </div>
              <div>
                <dt className="text-[11px] font-bold text-[color:var(--pub-muted)]">Strength</dt>
                <dd className="mt-1 text-[15px] font-extrabold text-[color:var(--pub-fg)]">{selected.strength}</dd>
              </div>
              <div>
                <dt className="text-[11px] font-bold text-[color:var(--pub-muted)]">Next Growth</dt>
                <dd className="mt-1 text-[15px] font-extrabold text-[color:var(--pub-fg)]">{selected.nextGrowth}</dd>
              </div>
            </dl>
            <p className="mt-3 text-[12px] text-[color:var(--pub-muted)]">
              پیش‌نمای ساختاری مسیر — بر اساس انتخاب شما؛ داده پیشرفت کاربر جعلی نیست.
            </p>
          </div>
        ) : null}
      </GlassCard>
    </PublicSection>
  );
}
