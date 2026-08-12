import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";

export type ToolCardProProps = {
  title: string;
  href: string;
  category?: string | null;
  description?: string | null;
  useCases?: string[];
  stackTags?: string[];
  className?: string;
};

/**
 * Premium tool card — icon, rating placeholder chrome, use cases, stack tags.
 * Rating is structural UI only (no invented scores).
 */
export function ToolCardPro({
  title,
  href,
  category,
  description,
  useCases = [],
  stackTags = [],
  className = "",
}: ToolCardProProps) {
  const monogram = title.trim().slice(0, 1).toUpperCase() || "T";

  return (
    <Link
      href={href}
      className={["focus-ring group block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}
    >
      <Card variant="elevated" className="flex h-full flex-col gap-3">
        <div className="flex items-start gap-3">
          <span
            className="ds-glow flex h-12 w-12 shrink-0 items-center justify-center rounded-[var(--ds-radius-lg)] border border-[color:var(--ds-border-strong)] bg-gradient-to-br from-[color:var(--ds-primary)]/30 to-[color:var(--ds-secondary)]/20 text-lg font-extrabold text-[color:var(--ds-fg)] transition-transform group-hover:scale-105"
            aria-hidden
          >
            {monogram}
          </span>
          <div className="min-w-0 flex-1">
            <div className="mb-1 flex flex-wrap items-center gap-2">
              <h3 className="truncate text-[15px] font-bold text-[color:var(--ds-fg)] group-hover:text-[color:var(--ds-secondary)]">
                {title}
              </h3>
              {category ? <Badge variant="secondary">{category}</Badge> : null}
            </div>
            {/* Structural rating chrome — not a real score */}
            <p className="flex items-center gap-1 text-[11px] text-[color:var(--ds-muted)]" aria-label="امتیاز به‌زودی">
              <span aria-hidden className="tracking-tight text-[color:var(--ds-secondary)]/50">
                ★★★★★
              </span>
              <span>امتیاز به‌زودی</span>
            </p>
          </div>
        </div>

        {description ? (
          <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{description}</p>
        ) : (
          <p className="text-[13px] text-[color:var(--ds-muted)]">ابزار مهندسی برای گردش‌کار توسعه</p>
        )}

        {useCases.length > 0 ? (
          <ul className="flex flex-wrap gap-1.5" aria-label="موارد استفاده">
            {useCases.map((item) => (
              <li key={item}>
                <Badge variant="outline">{item}</Badge>
              </li>
            ))}
          </ul>
        ) : null}

        {stackTags.length > 0 ? (
          <div className="mt-auto flex flex-wrap gap-1.5 border-t border-[color:var(--ds-border)] pt-3">
            {stackTags.map((tag) => (
              <Badge key={tag} variant="primary">
                {tag}
              </Badge>
            ))}
          </div>
        ) : null}
      </Card>
    </Link>
  );
}
