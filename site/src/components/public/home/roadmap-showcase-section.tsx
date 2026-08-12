import Link from "next/link";
import { RoadmapCard } from "@/components/ui/public/roadmap-card";
import { GradientHeading } from "@/components/ui/public/gradient-heading";
import { Section } from "@/components/ui/public/section";
import type { ContentSummaryDto } from "@/lib/api/content";

type RoadmapShowcaseSectionProps = {
  items: ContentSummaryDto[];
};

export function RoadmapShowcaseSection({ items }: RoadmapShowcaseSectionProps) {
  return (
    <Section aria-labelledby="roadmap-showcase-title">
      <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
        <GradientHeading
          as="h2"
          id="roadmap-showcase-title"
          subtitle="نقشه‌های راه منتشرشده برای مسیر یادگیری ساخت‌یافته"
        >
          ویترین نقشه راه
        </GradientHeading>
        <Link
          href="/roadmap"
          className="focus-ring text-[13px] font-semibold text-emerald-300 hover:text-emerald-200"
        >
          همه نقشه‌ها ←
        </Link>
      </div>

      {items.length === 0 ? (
        <p className="rounded-2xl border border-dashed border-[color:var(--border-strong)] px-4 py-10 text-center text-sm text-[color:var(--muted)]">
          نقشه راه منتشرشده‌ای نیست. می‌توانید از{" "}
          <Link href="/roadmap" className="text-emerald-300 underline-offset-2 hover:underline">
            صفحه نقشه راه
          </Link>{" "}
          شروع کنید.
        </p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.slice(0, 6).map((item) => (
            <RoadmapCard
              key={item.id}
              title={item.title}
              href={`/roadmap?slug=${encodeURIComponent(item.slug)}`}
              summary={null}
            />
          ))}
        </div>
      )}
    </Section>
  );
}
