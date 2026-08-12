import Link from "next/link";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

export type ToolCardV2Props = {
  title: string;
  href: string;
  category?: string | null;
  description?: string | null;
  /** Optional logo URL; falls back to monogram placeholder. */
  logoUrl?: string | null;
  className?: string;
};

export function ToolCard({
  title,
  href,
  category,
  description,
  logoUrl,
  className = "",
}: ToolCardV2Props) {
  const monogram = title.trim().slice(0, 1) || "T";

  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--pub-radius)]", className].join(" ")}>
      <GlassCard className="h-full p-4" gradientBorder>
        <div className="flex items-start gap-3">
          <span
            className="flex h-12 w-12 shrink-0 items-center justify-center overflow-hidden rounded-2xl border border-[color:var(--pub-glass-border)] bg-gradient-to-br from-[color:var(--pub-primary)]/25 to-[color:var(--pub-secondary)]/15 text-lg font-extrabold text-[color:var(--pub-fg)] shadow-[0_0_24px_var(--pub-glow)]"
            aria-hidden
          >
            {logoUrl ? (
              // eslint-disable-next-line @next/next/no-img-element
              <img src={logoUrl} alt="" className="h-full w-full object-cover" />
            ) : (
              monogram
            )}
          </span>
          <div className="min-w-0 flex-1">
            <div className="mb-1 flex flex-wrap items-center gap-2">
              <h3 className="truncate text-[15px] font-bold text-[color:var(--pub-fg)]">{title}</h3>
              {category ? <PremiumBadge variant="cyan">{category}</PremiumBadge> : null}
            </div>
            {description ? (
              <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--pub-muted)]">{description}</p>
            ) : (
              <p className="text-[13px] text-[color:var(--pub-muted)]">ابزار مهندسی برای گردش‌کار توسعه</p>
            )}
          </div>
        </div>
      </GlassCard>
    </Link>
  );
}
