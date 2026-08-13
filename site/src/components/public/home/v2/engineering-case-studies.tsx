import Link from "next/link";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Badge } from "@/components/ui/ds/badge";
import { Card } from "@/components/ui/ds/card";
import { PublicSection } from "@/components/ui/public/v2";
import type { ContentSummaryDto } from "@/lib/api/content";
import { ENGINEERING_STORIES } from "@/lib/public/intelligence-showcase";
import { publicHrefForContent } from "@/lib/public/content-helpers";

type Props = {
  publishedExamples?: ContentSummaryDto[];
};

/**
 * Engineering Stories — documentation-style case cards (Challenge / Architecture / Learning).
 */
export function EngineeringCaseStudies({ publishedExamples = [] }: Props) {
  const published = publishedExamples.slice(0, 2);

  return (
    <PublicSection className="ix-reveal" aria-labelledby="case-studies-title">
      <PremiumSectionHeader
        eyebrow="Engineering Stories"
        title="نمونه‌های مستند"
        description="مستندوار — چالش، معماری و یادگیری؛ نه کارت مقالهٔ معمولی"
        titleId="case-studies-title"
        href="/articles"
        ctaLabel="مشاهده مقالات"
        icon={<span aria-hidden>◇</span>}
      />

      <div className="grid gap-4 lg:grid-cols-3">
        {ENGINEERING_STORIES.map((story) => (
          <Link
            key={story.id}
            href={story.href}
            className="focus-ring group block h-full rounded-[var(--ds-radius-xl)]"
          >
            <Card variant="glass" className="ix-card-lift flex h-full flex-col gap-4 !p-0 overflow-hidden">
              <div className="border-b border-[color:var(--ds-border)] bg-gradient-to-l from-[color:var(--ds-primary)]/20 via-transparent to-[color:var(--ds-secondary)]/10 px-4 py-3 sm:px-5">
                <Badge variant="outline">Engineering Story</Badge>
                <h3 className="mt-2 text-base font-extrabold text-[color:var(--ds-fg)] group-hover:text-[#c4b5fd]">
                  {story.title}
                </h3>
              </div>
              <div className="flex flex-1 flex-col gap-3 px-4 pb-4 sm:px-5">
                <section>
                  <p className="text-[10px] font-bold uppercase tracking-wide text-[color:var(--ds-secondary)]">
                    Challenge
                  </p>
                  <p className="mt-1 text-[13px] leading-6 text-[color:var(--ds-muted)]">{story.challenge}</p>
                </section>
                <section>
                  <p className="text-[10px] font-bold uppercase tracking-wide text-[color:var(--ds-secondary)]">
                    Architecture
                  </p>
                  <ul className="mt-1.5 flex flex-wrap gap-1.5">
                    {story.architecture.map((item) => (
                      <li key={item}>
                        <Badge variant="secondary">{item}</Badge>
                      </li>
                    ))}
                  </ul>
                </section>
                <section className="mt-auto border-t border-[color:var(--ds-border)] pt-3">
                  <p className="text-[10px] font-bold uppercase tracking-wide text-[color:var(--ds-secondary)]">
                    Learning
                  </p>
                  <p className="mt-1 text-[13px] leading-6 text-[color:var(--ds-fg)]/90">{story.learning}</p>
                </section>
              </div>
            </Card>
          </Link>
        ))}
      </div>

      {published.length > 0 ? (
        <div className="mt-6">
          <p className="mb-3 text-[12px] font-bold text-[color:var(--pub-secondary)]">
            از پایگاه دانش منتشرشده
          </p>
          <div className="grid gap-3 sm:grid-cols-2">
            {published.map((item) => (
              <Link
                key={item.id}
                href={publicHrefForContent(item)}
                className="focus-ring group block rounded-[var(--ds-radius-xl)]"
              >
                <Card variant="elevated" className="ix-card-lift flex h-full flex-col gap-2">
                  <Badge variant="primary">Published</Badge>
                  <h3 className="text-[15px] font-extrabold text-[color:var(--ds-fg)] group-hover:text-[#c4b5fd]">
                    {item.title}
                  </h3>
                </Card>
              </Link>
            ))}
          </div>
        </div>
      ) : null}
    </PublicSection>
  );
}
