import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";

export type ArticleCardProps = {
  title: string;
  href: string;
  category?: string | null;
  summary?: string | null;
  readingTime?: string | null;
  className?: string;
};

export function ArticleCard({
  title,
  href,
  category,
  summary,
  readingTime,
  className = "",
}: ArticleCardProps) {
  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}>
      <Card className="flex h-full flex-col overflow-hidden !p-0" variant="glass">
        <div
          className="h-28 bg-gradient-to-bl from-[color:var(--ds-primary)]/30 via-[color:var(--ds-primary-strong)]/10 to-transparent"
          aria-hidden
        />
        <div className="flex flex-1 flex-col gap-2 p-4">
          <div className="flex flex-wrap gap-1.5">
            {category ? <Badge variant="primary">{category}</Badge> : null}
            {readingTime ? <Badge variant="outline">{readingTime}</Badge> : null}
          </div>
          <h3 className="line-clamp-2 text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
          {summary ? (
            <p className="line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{summary}</p>
          ) : null}
        </div>
      </Card>
    </Link>
  );
}
