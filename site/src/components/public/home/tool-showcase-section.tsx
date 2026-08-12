import Link from "next/link";
import { ToolCard } from "@/components/ui/public/tool-card";
import { GradientHeading } from "@/components/ui/public/gradient-heading";
import { Section } from "@/components/ui/public/section";
import type { ToolSummaryDto } from "@/lib/api/toolbox";
import type { ContentSummaryDto } from "@/lib/api/content";

type ToolShowcaseSectionProps = {
  tools: ToolSummaryDto[];
  /** Fallback CMS Tool content when toolbox catalog is empty. */
  contentTools?: ContentSummaryDto[];
};

export function ToolShowcaseSection({ tools, contentTools = [] }: ToolShowcaseSectionProps) {
  const cards =
    tools.length > 0
      ? tools.slice(0, 6).map((tool) => ({
          key: tool.id,
          title: tool.title,
          href: `/tools/${encodeURIComponent(tool.slug)}`,
          category: tool.categorySlug ?? "ابزار",
          status: tool.status,
        }))
      : contentTools.slice(0, 6).map((tool) => ({
          key: tool.id,
          title: tool.title,
          href: `/tools/${encodeURIComponent(tool.slug)}`,
          category: "Tool",
          status: tool.status,
        }));

  return (
    <Section aria-labelledby="tool-showcase-title" className="bg-[color:var(--surface)]/40">
      <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
        <GradientHeading
          as="h2"
          id="tool-showcase-title"
          subtitle="ابزارهای اجرایی Toolbox و کاتالوگ Tool Library"
        >
          ویترین ابزارها
        </GradientHeading>
        <Link
          href="/toolbox"
          className="focus-ring text-[13px] font-semibold text-sky-300 hover:text-sky-200"
        >
          همه ابزارها ←
        </Link>
      </div>

      {cards.length === 0 ? (
        <p className="rounded-2xl border border-dashed border-[color:var(--border-strong)] px-4 py-10 text-center text-sm text-[color:var(--muted)]">
          ابزاری برای نمایش نیست.
        </p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {cards.map((card) => (
            <ToolCard
              key={card.key}
              title={card.title}
              href={card.href}
              category={card.category}
              status={card.status}
            />
          ))}
        </div>
      )}
    </Section>
  );
}
