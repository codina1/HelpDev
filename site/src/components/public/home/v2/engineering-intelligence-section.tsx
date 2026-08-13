import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PublicSection } from "@/components/ui/public/v2";
import { INTELLIGENCE_CARDS } from "@/lib/public/intelligence-showcase";

const ACCENT = {
  primary: "from-[color:var(--pub-primary)]/25 via-transparent to-transparent",
  ai: "from-[color:var(--pub-primary)]/30 via-[color:var(--pub-secondary)]/10 to-transparent",
  cyan: "from-[color:var(--pub-secondary)]/25 via-transparent to-transparent",
} as const;

/**
 * Engineering Intelligence — three premium glass cards.
 */
export function EngineeringIntelligenceSection() {
  return (
    <PublicSection className="ix-reveal" aria-labelledby="eng-intel-title">
      <PremiumSectionHeader
        eyebrow="Intelligence OS"
        title="Engineering Intelligence"
        description="سه ستون HelpDev — دانش، هوش و حافظه تصمیم"
        titleId="eng-intel-title"
        icon={<span aria-hidden>◈</span>}
      />

      <div className="grid gap-4 md:grid-cols-3">
        {INTELLIGENCE_CARDS.map((card) => (
          <GlassCard
            key={card.id}
            strong
            gradientBorder
            className="ix-card-lift relative overflow-hidden p-5 sm:p-6"
          >
            <div
              className={["pointer-events-none absolute inset-0 bg-gradient-to-bl", ACCENT[card.accent]].join(" ")}
              aria-hidden
            />
            <div className="relative z-[1]">
              <Badge variant={card.accent === "ai" ? "ai" : card.accent === "cyan" ? "secondary" : "primary"}>
                {card.id === "knowledge" ? "Knowledge" : card.id === "ai" ? "AI" : "Memory"}
              </Badge>
              <h3 className="mt-4 text-lg font-extrabold text-[color:var(--pub-fg)] sm:text-xl">
                {card.title}
              </h3>
              <p className="mt-2 text-[13px] leading-7 text-[color:var(--pub-muted)]">{card.content}</p>
            </div>
          </GlassCard>
        ))}
      </div>
    </PublicSection>
  );
}
