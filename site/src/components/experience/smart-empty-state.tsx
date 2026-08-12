import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { GlowButton } from "@/components/ui/public/v2/glow-button";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

export type SmartEmptyStateProps = {
  title: string;
  description: string;
  ctaLabel?: string;
  ctaHref?: string;
  onCtaClick?: () => void;
  badge?: string;
  className?: string;
};

/**
 * Premium empty state — never invents data; only messaging + CTA.
 */
export function SmartEmptyState({
  title,
  description,
  ctaLabel,
  ctaHref,
  onCtaClick,
  badge = "Experience",
  className = "",
}: SmartEmptyStateProps) {
  return (
    <GlassCard
      elevate={false}
      gradientBorder
      className={["px-5 py-10 text-center sm:px-8 sm:py-12", className].join(" ")}
    >
      <PremiumBadge variant="ai" className="mb-4">
        {badge}
      </PremiumBadge>
      <h3 className="text-lg font-extrabold text-[color:var(--pub-fg)] sm:text-xl">{title}</h3>
      <p className="mx-auto mt-2 max-w-md text-[13px] leading-7 text-[color:var(--pub-muted)]">
        {description}
      </p>
      {ctaLabel && (ctaHref || onCtaClick) ? (
        <div className="mt-6 flex justify-center">
          <GlowButton href={ctaHref} onClick={onCtaClick}>
            {ctaLabel}
          </GlowButton>
        </div>
      ) : null}
    </GlassCard>
  );
}
