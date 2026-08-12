import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";

export type ArticleCardProProps = {
  title: string;
  href: string;
  category?: string | null;
  summary?: string | null;
  /** Presentation AI insight derived from title/slug — not invented body text. */
  aiSummary?: string | null;
  readingTime?: string | null;
  difficulty?: string | null;
  tags?: string[];
  featured?: boolean;
  coverTone?: "violet" | "cyan" | "indigo";
  className?: string;
};

const COVER = {
  violet: "from-[color:var(--ds-primary)]/40 via-[color:var(--ds-primary-strong)]/15 to-transparent",
  cyan: "from-[color:var(--ds-secondary)]/35 via-transparent to-transparent",
  indigo: "from-indigo-500/40 via-[color:var(--ds-primary)]/15 to-transparent",
} as const;

/**
 * Intelligence article card — category, difficulty, tech tags, reading time, AI summary.
 */
export function ArticleCardPro({
  title,
  href,
  category,
  summary,
  aiSummary,
  readingTime,
  difficulty,
  tags = [],
  featured = false,
  coverTone = "violet",
  className = "",
}: ArticleCardProProps) {
  return (
    <Link
      href={href}
      className={["focus-ring group block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}
    >
      <Card
        variant="glass"
        className={[
          "flex h-full flex-col overflow-hidden !p-0",
          featured ? "sm:min-h-[300px]" : "",
        ].join(" ")}
      >
        <div
          className={[
            "relative overflow-hidden bg-gradient-to-bl",
            COVER[coverTone],
            featured ? "h-40 sm:h-48" : "h-28 sm:h-32",
          ].join(" ")}
          aria-hidden
        >
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_20%_20%,color-mix(in_srgb,var(--ds-primary)_30%,transparent),transparent_55%)] transition-opacity group-hover:opacity-100" />
          <div className="absolute inset-x-0 bottom-0 h-px bg-gradient-to-l from-transparent via-[color:var(--ds-primary)]/50 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
          <div className="absolute bottom-3 start-3 flex flex-wrap gap-1.5">
            {category ? <Badge variant="primary">{category}</Badge> : null}
            {difficulty ? <Badge variant="outline">{difficulty}</Badge> : null}
          </div>
        </div>
        <div className="flex flex-1 flex-col gap-2 p-4 sm:p-5">
          <h3
            className={[
              "font-extrabold leading-7 text-[color:var(--ds-fg)] transition-colors group-hover:text-[#c4b5fd]",
              featured ? "text-lg sm:text-xl" : "line-clamp-2 text-[15px]",
            ].join(" ")}
          >
            {title}
          </h3>
          {summary ? (
            <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{summary}</p>
          ) : null}
          {aiSummary ? (
            <div className="rounded-xl border border-[color:color-mix(in_srgb,var(--ds-secondary)_35%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-secondary)_8%,transparent)] px-3 py-2">
              <div className="mb-1 flex items-center gap-1.5">
                <Badge variant="ai">AI summary</Badge>
              </div>
              <p className="line-clamp-2 text-[12px] leading-5 text-[color:var(--ds-muted)]">{aiSummary}</p>
            </div>
          ) : null}
          <div className="mt-auto flex flex-wrap items-center gap-2 pt-2">
            {readingTime ? (
              <span className="text-[11px] font-semibold text-[color:var(--ds-muted)]">{readingTime}</span>
            ) : null}
            {tags.map((tag) => (
              <Badge key={tag} variant="secondary">
                {tag}
              </Badge>
            ))}
          </div>
        </div>
      </Card>
    </Link>
  );
}
