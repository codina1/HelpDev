import Link from "next/link";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";

export type KnowledgeCardProps = {
  title: string;
  href: string;
  category?: string | null;
  summary?: string | null;
  readingTime?: string | null;
  difficulty?: string | null;
  author?: string | null;
  coverTone?: "violet" | "cyan" | "indigo";
  coverImage?: string | null;
  featured?: boolean;
  className?: string;
};

const COVER: Record<NonNullable<KnowledgeCardProps["coverTone"]>, string> = {
  violet: "from-[color:var(--pub-primary)]/30 via-[color:var(--pub-primary-2)]/15 to-transparent",
  cyan: "from-[color:var(--pub-secondary)]/25 via-transparent to-transparent",
  indigo: "from-indigo-500/30 via-violet-500/10 to-transparent",
};

export function KnowledgeCard({
  title,
  href,
  category,
  summary,
  readingTime,
  difficulty,
  author,
  coverTone = "violet",
  coverImage,
  featured = false,
  className = "",
}: KnowledgeCardProps) {
  const resolvedCover = coverImage?.trim() ?? "";
  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--pub-radius)]", className].join(" ")}>
      <GlassCard
        gradientBorder={featured}
        className={["h-full overflow-hidden p-0", featured ? "sm:min-h-[280px]" : ""].join(" ")}
      >
        <div
          className={[
            "relative h-28 overflow-hidden bg-gradient-to-bl sm:h-32",
            COVER[coverTone],
            featured ? "h-36 sm:h-44" : "",
          ].join(" ")}
          aria-hidden
        >
          {resolvedCover ? (
            <img src={resolvedCover} alt="" className="absolute inset-0 h-full w-full object-cover" />
          ) : (
            <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,color-mix(in_srgb,var(--pub-primary)_35%,transparent),transparent_55%)]" />
          )}
          <div className="absolute bottom-3 start-3 flex flex-wrap gap-1.5">
            {category ? <PremiumBadge variant="primary">{category}</PremiumBadge> : null}
            {difficulty ? <PremiumBadge variant="outline">{difficulty}</PremiumBadge> : null}
          </div>
        </div>
        <div className="space-y-2 p-4">
          <h3
            className={[
              "font-bold leading-7 text-[color:var(--pub-fg)]",
              featured ? "text-lg sm:text-xl" : "line-clamp-2 text-[15px]",
            ].join(" ")}
          >
            {title}
          </h3>
          {summary ? (
            <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--pub-muted)]">{summary}</p>
          ) : null}
          <div className="flex flex-wrap items-center gap-2 pt-1 text-[11px] text-[color:var(--pub-muted)]">
            {readingTime ? <span>{readingTime}</span> : null}
            {author ? <span>· {author}</span> : null}
          </div>
        </div>
      </GlassCard>
    </Link>
  );
}
