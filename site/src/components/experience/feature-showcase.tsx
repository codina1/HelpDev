import Link from "next/link";
import { GlassCard } from "@/components/ui/public/v2/glass-card";

export type FeatureShowcaseItem = {
  title: string;
  description: string;
  href: string;
  accent?: "primary" | "cyan" | "ai";
};

type FeatureShowcaseProps = {
  items: FeatureShowcaseItem[];
  className?: string;
};

const ACCENT = {
  primary: "from-[color:var(--pub-primary)]/40 to-transparent",
  cyan: "from-[color:var(--pub-secondary)]/35 to-transparent",
  ai: "from-[color:var(--pub-ai-from)]/40 to-[color:var(--pub-ai-to)]/15",
} as const;

export function FeatureShowcase({ items, className = "" }: FeatureShowcaseProps) {
  return (
    <ul className={["grid gap-3 sm:grid-cols-2 lg:grid-cols-4", className].join(" ")}>
      {items.map((item) => (
        <li key={item.href}>
          <Link href={item.href} className="focus-ring block h-full rounded-[var(--pub-radius)]">
            <GlassCard className="exp-card-lift h-full overflow-hidden p-4">
              <div
                className={[
                  "mb-3 h-1.5 w-12 rounded-full bg-gradient-to-l",
                  ACCENT[item.accent ?? "primary"],
                ].join(" ")}
                aria-hidden
              />
              <h3 className="text-[14px] font-bold text-[color:var(--pub-fg)]">{item.title}</h3>
              <p className="mt-1.5 text-[12px] leading-6 text-[color:var(--pub-muted)]">
                {item.description}
              </p>
            </GlassCard>
          </Link>
        </li>
      ))}
    </ul>
  );
}
