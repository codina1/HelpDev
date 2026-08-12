import Link from "next/link";
import { ContentCard } from "@/components/ui/public/content-card";
import { GradientHeading } from "@/components/ui/public/gradient-heading";
import { Section } from "@/components/ui/public/section";
import type { ContentSummaryDto } from "@/lib/api/content";
import { labelForContentType } from "@/lib/admin/content/content-mappers";
import { contentMetaLine, publicHrefForContent } from "@/lib/public/content-helpers";

type LatestContentSectionProps = {
  items: ContentSummaryDto[];
};

export function LatestContentSection({ items }: LatestContentSectionProps) {
  return (
    <Section aria-labelledby="latest-content-title">
      <div className="mb-6 flex flex-wrap items-end justify-between gap-3">
        <GradientHeading
          as="h2"
          id="latest-content-title"
          subtitle="آخرین محتوای منتشرشده از Content API"
        >
          آخرین محتوا
        </GradientHeading>
        <Link
          href="/articles"
          className="focus-ring text-[13px] font-semibold text-violet-300 hover:text-violet-200"
        >
          همه مقالات ←
        </Link>
      </div>

      {items.length === 0 ? (
        <p className="rounded-2xl border border-dashed border-[color:var(--border-strong)] px-4 py-10 text-center text-sm text-[color:var(--muted)]">
          هنوز محتوای منتشرشده‌ای نیست. از پنل ادمین منتشر کنید.
        </p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.slice(0, 6).map((item) => (
            <ContentCard
              key={item.id}
              title={item.title}
              href={publicHrefForContent(item)}
              typeLabel={labelForContentType(item.type)}
              meta={contentMetaLine(item)}
              views={item.views}
            />
          ))}
        </div>
      )}
    </Section>
  );
}
