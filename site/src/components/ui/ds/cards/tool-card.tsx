import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";

export type ToolCardProps = {
  title: string;
  href: string;
  category?: string | null;
  description?: string | null;
  className?: string;
};

export function ToolCard({ title, href, category, description, className = "" }: ToolCardProps) {
  const monogram = title.trim().slice(0, 1) || "T";

  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}>
      <Card className="h-full" variant="elevated">
        <div className="flex items-start gap-3">
          <span
            className="flex h-12 w-12 shrink-0 items-center justify-center rounded-[var(--ds-radius-lg)] border border-[color:var(--ds-border-strong)] bg-gradient-to-br from-[color:var(--ds-primary)]/25 to-[color:var(--ds-secondary)]/15 text-lg font-extrabold text-[color:var(--ds-fg)] ds-glow"
            aria-hidden
          >
            {monogram}
          </span>
          <div className="min-w-0 flex-1">
            <div className="mb-1 flex flex-wrap items-center gap-2">
              <h3 className="truncate text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
              {category ? <Badge variant="secondary">{category}</Badge> : null}
            </div>
            {description ? (
              <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{description}</p>
            ) : (
              <p className="text-[13px] text-[color:var(--ds-muted)]">ابزار مهندسی برای گردش‌کار توسعه</p>
            )}
          </div>
        </div>
      </Card>
    </Link>
  );
}
