import Link from "next/link";
import { Badge } from "@/components/ui/ds/badge";
import { Button } from "@/components/ui/ds/button";
import { Card } from "@/components/ui/ds/card";

export type AiCardProps = {
  title: string;
  description: string;
  href?: string;
  ctaLabel?: string;
  className?: string;
};

export function AiCard({
  title,
  description,
  href = "/learning/assistant",
  ctaLabel = "شروع گفتگو",
  className = "",
}: AiCardProps) {
  return (
    <Card
      variant="elevated"
      hover={false}
      className={[
        "relative overflow-hidden border-[color:color-mix(in_srgb,var(--ds-primary)_35%,transparent)]",
        className,
      ].join(" ")}
    >
      <div
        className="pointer-events-none absolute -end-10 -top-10 h-40 w-40 rounded-full bg-[color:var(--ds-primary)]/25 blur-3xl"
        aria-hidden
      />
      <Badge variant="ai" className="mb-3">
        AI
      </Badge>
      <h3 className="text-lg font-extrabold text-[color:var(--ds-fg)]">{title}</h3>
      <p className="mt-2 text-[13px] leading-7 text-[color:var(--ds-muted)]">{description}</p>
      <div className="mt-5">
        <Button href={href} size="sm">
          {ctaLabel}
        </Button>
      </div>
      {href ? (
        <Link href={href} className="sr-only">
          {ctaLabel}
        </Link>
      ) : null}
    </Card>
  );
}
