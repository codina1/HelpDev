import Link from "next/link";
import { GlassCard } from "@/components/ui/public/v2/glass-card";

export type FeatureGridItem = {
  title: string;
  description: string;
  href: string;
  accent?: "primary" | "cyan" | "ai";
};

type FeatureGridProps = {
  items: FeatureGridItem[];
  className?: string;
};

const ACCENT = {
  primary: "from-[color:var(--pub-primary)]/25 to-transparent",
  cyan: "from-[color:var(--pub-secondary)]/20 to-transparent",
  ai: "from-[color:var(--pub-ai-from)]/25 to-[color:var(--pub-ai-to)]/10",
} as const;

export function FeatureGrid({ items, className = "" }: FeatureGridProps) {
  return (
    <ul className={["grid gap-3 sm:grid-cols-2 lg:grid-cols-4", className].join(" ")}>
      {items.map((item) => (
        <li key={item.href}>
          <Link href={item.href} className="focus-ring block h-full rounded-[var(--pub-radius)]">
            <GlassCard className="h-full overflow-hidden p-4">
              <div
                className={[
                  "mb-3 h-1.5 w-12 rounded-full bg-gradient-to-l",
                  ACCENT[item.accent ?? "primary"],
                ].join(" ")}
                aria-hidden
              />
              <h3 className="text-[14px] font-bold text-[color:var(--pub-fg)]">{item.title}</h3>
              <p className="mt-1.5 text-[12px] leading-6 text-[color:var(--pub-muted)]">{item.description}</p>
            </GlassCard>
          </Link>
        </li>
      ))}
    </ul>
  );
}
