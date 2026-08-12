import Link from "next/link";
import { GlowButton } from "@/components/ui/public/v2/glow-button";
import { GradientText } from "@/components/ui/public/v2/gradient-text";

type PremiumSectionHeaderProps = {
  eyebrow?: string;
  title: string;
  description?: string;
  href?: string;
  ctaLabel?: string;
  icon?: React.ReactNode;
  titleId?: string;
  className?: string;
};

export function PremiumSectionHeader({
  eyebrow,
  title,
  description,
  href,
  ctaLabel,
  icon,
  titleId,
  className = "",
}: PremiumSectionHeaderProps) {
  return (
    <div className={["mb-8 flex flex-wrap items-end justify-between gap-4", className].join(" ")}>
      <div className="max-w-2xl">
        <div className="mb-2 flex items-center gap-2">
          {icon ? (
            <span
              className="flex h-9 w-9 items-center justify-center rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-glass)] text-[color:var(--pub-ai-from)]"
              aria-hidden
            >
              {icon}
            </span>
          ) : null}
          {eyebrow ? (
            <p className="text-[11px] font-bold tracking-wide text-[color:var(--pub-secondary)]">
              {eyebrow}
            </p>
          ) : null}
        </div>
        <h2 id={titleId} className="text-2xl font-extrabold sm:text-3xl">
          <GradientText>{title}</GradientText>
        </h2>
        {description ? (
          <p className="mt-2 text-[13px] leading-7 text-[color:var(--pub-muted)] sm:text-[14px]">
            {description}
          </p>
        ) : null}
      </div>
      {href && ctaLabel ? (
        <GlowButton href={href} variant="secondary" className="!px-4 !py-2 text-[12px]">
          {ctaLabel}
        </GlowButton>
      ) : href ? (
        <Link
          href={href}
          className="focus-ring text-[13px] font-semibold text-[color:var(--pub-ai-from)] hover:underline"
        >
          {ctaLabel ?? "مشاهده همه ←"}
        </Link>
      ) : null}
    </div>
  );
}
