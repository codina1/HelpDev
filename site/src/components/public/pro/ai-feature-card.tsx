import { Badge } from "@/components/ui/ds/badge";
import { Button } from "@/components/ui/ds/button";
import { Card } from "@/components/ui/ds/card";

export type AiFeatureCardProps = {
  title: string;
  description: string;
  primaryHref?: string;
  primaryLabel?: string;
  secondaryHref?: string;
  secondaryLabel?: string;
  className?: string;
};

/**
 * Premium AI assistant / feature CTA card.
 */
export function AiFeatureCard({
  title,
  description,
  primaryHref = "/learning/assistant",
  primaryLabel = "شروع گفتگو",
  secondaryHref = "/search",
  secondaryLabel = "جستجوی دانش",
  className = "",
}: AiFeatureCardProps) {
  return (
    <Card
      variant="elevated"
      hover={false}
      className={[
        "relative overflow-hidden border-[color:color-mix(in_srgb,var(--ds-primary)_40%,transparent)] p-6 sm:p-8",
        className,
      ].join(" ")}
    >
      <div
        className="pointer-events-none absolute -end-16 -top-16 h-56 w-56 rounded-full bg-[color:var(--ds-primary)]/30 blur-3xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute -bottom-20 -start-10 h-48 w-48 rounded-full bg-[color:var(--ds-secondary)]/20 blur-3xl"
        aria-hidden
      />
      <Badge variant="ai" className="mb-4">
        Engineering Intelligence
      </Badge>
      <h3 className="max-w-2xl text-2xl font-extrabold leading-10 text-[color:var(--ds-fg)] sm:text-3xl">
        {title}
      </h3>
      <p className="mt-3 max-w-xl text-[14px] leading-8 text-[color:var(--ds-muted)]">{description}</p>
      <div className="mt-7 flex flex-wrap gap-3">
        <Button href={primaryHref}>{primaryLabel}</Button>
        <Button href={secondaryHref} variant="secondary">
          {secondaryLabel}
        </Button>
      </div>
    </Card>
  );
}
