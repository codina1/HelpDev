import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PublicSection } from "@/components/ui/public/v2";
import { DEVELOPER_JOURNEY } from "@/lib/public/intelligence-showcase";

/**
 * Developer Journey Timeline — Beginner → Developer → AI Engineer → Architect.
 */
export function DeveloperJourneyTimeline() {
  return (
    <PublicSection className="ds-slide" aria-labelledby="dev-journey-title">
      <PremiumSectionHeader
        eyebrow="Journey"
        title="مسیر رشد حرفه‌ای"
        description="از مبتدی تا معمار — مراحل ساختاری رشد در اکوسیستم HelpDev"
        titleId="dev-journey-title"
        href="/learning"
        ctaLabel="شروع یادگیری"
        icon={<span aria-hidden>↗</span>}
      />

      <GlassCard strong gradientBorder className="p-4 sm:p-6">
        <ol
          className="relative grid gap-4 md:grid-cols-4"
          aria-label="تایم‌لاین مسیر توسعه‌دهنده"
        >
          <span
            className="showcase-journey-line pointer-events-none absolute start-[12%] end-[12%] top-7 hidden h-px bg-gradient-to-l from-[color:var(--pub-primary)] via-[color:var(--pub-secondary)] to-[color:var(--pub-primary)] opacity-60 md:block"
            aria-hidden
          />
          {DEVELOPER_JOURNEY.map((stage, index) => (
            <li key={stage.id} className="relative flex flex-col items-center text-center">
              <span
                className={[
                  "showcase-journey-node relative z-[1] mb-3 flex h-14 w-14 items-center justify-center rounded-2xl border text-[13px] font-extrabold",
                  index === DEVELOPER_JOURNEY.length - 1
                    ? "border-transparent bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-white shadow-[0_0_28px_var(--pub-glow)]"
                    : "border-[color:var(--pub-glass-border)] bg-[color:var(--pub-glass-strong)] text-[color:var(--pub-fg)]",
                ].join(" ")}
              >
                {index + 1}
              </span>
              <Badge variant={index >= 2 ? "ai" : "outline"} className="mb-2">
                {stage.label}
              </Badge>
              <p className="text-[15px] font-extrabold text-[color:var(--pub-fg)]">{stage.titleFa}</p>
              <p className="mt-1.5 max-w-[14rem] text-[12px] leading-6 text-[color:var(--pub-muted)]">
                {stage.description}
              </p>
              {index < DEVELOPER_JOURNEY.length - 1 ? (
                <span className="mt-3 text-[color:var(--pub-secondary)] md:hidden" aria-hidden>
                  ↓
                </span>
              ) : null}
            </li>
          ))}
        </ol>
      </GlassCard>
    </PublicSection>
  );
}
