import { Button } from "@/components/ui/ds/button";
import { Badge } from "@/components/ui/ds/badge";

type EmptyStateProps = {
  title: string;
  description?: string;
  badge?: string;
  ctaLabel?: string;
  ctaHref?: string;
  onCtaClick?: () => void;
  className?: string;
};

export function EmptyState({
  title,
  description,
  badge = "Empty",
  ctaLabel,
  ctaHref,
  onCtaClick,
  className = "",
}: EmptyStateProps) {
  return (
    <div
      dir="rtl"
      role="status"
      className={[
        "ds-surface flex flex-col items-center gap-3 px-6 py-10 text-center",
        className,
      ].join(" ")}
    >
      <Badge variant="ai">{badge}</Badge>
      <h3 className="text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
      {description ? (
        <p className="mx-auto max-w-md text-[13px] leading-6 text-[color:var(--ds-muted)]">{description}</p>
      ) : null}
      {ctaLabel && (ctaHref || onCtaClick) ? (
        <Button href={ctaHref} onClick={onCtaClick} size="sm">
          {ctaLabel}
        </Button>
      ) : null}
    </div>
  );
}
