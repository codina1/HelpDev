import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";

export type RoadmapCardNode = { label: string };

export type RoadmapCardProps = {
  title: string;
  href: string;
  nodes?: RoadmapCardNode[];
  summary?: string | null;
  className?: string;
};

export function RoadmapCard({
  title,
  href,
  nodes = [],
  summary,
  className = "",
}: RoadmapCardProps) {
  const display = nodes.length > 0 ? nodes.slice(0, 4) : [{ label: "شروع" }, { label: "مهارت" }, { label: "پروژه" }];

  return (
    <Link href={href} className={["focus-ring block h-full rounded-[var(--ds-radius-xl)]", className].join(" ")}>
      <Card className="h-full" variant="glass">
        <div className="mb-3 flex items-center gap-2">
          <Badge variant="success">نقشه راه</Badge>
        </div>
        <h3 className="text-[15px] font-extrabold text-[color:var(--ds-fg)]">{title}</h3>
        {summary ? (
          <p className="mt-2 line-clamp-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{summary}</p>
        ) : null}
        <ol className="mt-4 space-y-2" aria-label={`مراحل ${title}`}>
          {display.map((node, index) => (
            <li key={`${node.label}-${index}`} className="flex items-center gap-2 text-[13px] font-semibold text-[color:var(--ds-fg)]/90">
              <span className="flex h-6 w-6 items-center justify-center rounded-full border border-[color:color-mix(in_srgb,var(--ds-primary)_45%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-primary)_16%,transparent)] text-[10px] text-[color:var(--ds-primary)]">
                {index + 1}
              </span>
              {node.label}
            </li>
          ))}
        </ol>
      </Card>
    </Link>
  );
}
