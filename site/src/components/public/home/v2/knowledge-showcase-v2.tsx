import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { SmartEmptyState } from "@/components/experience/smart-empty-state";
import { ArticleCardPro } from "@/components/public/pro/article-card-pro";
import { PublicSection } from "@/components/ui/public/v2";
import type { ContentSummaryDto } from "@/lib/api/content";
import { labelForContentType } from "@/lib/admin/content/content-mappers";
import {
  estimateReadingLabel,
  inferTechTags,
  softAiSummary,
  softDifficulty,
} from "@/lib/public/display-meta";
import { publicHrefForContent } from "@/lib/public/content-helpers";

type Props = { items: ContentSummaryDto[] };

export function KnowledgeShowcaseV2({ items }: Props) {
  const featured = items[0];
  const rest = items.slice(1, 7);

  return (
    <PublicSection className="ds-slide" aria-labelledby="knowledge-showcase-title">
      <PremiumSectionHeader
        eyebrow="Intelligence"
        title="مقالات مهندسی هوشمند"
        description="کارت‌های Intelligence با دسته‌بندی، سطح سختی، برچسب فناوری، زمان مطالعه و بینش AI"
        href="/articles"
        ctaLabel="مشاهده همه مقالات"
        titleId="knowledge-showcase-title"
        icon={<span aria-hidden>◈</span>}
      />

      {items.length === 0 ? (
        <SmartEmptyState
          title="هنوز مقاله‌ای در پایگاه دانش نیست"
          description="پس از انتشار Article یا News از پنل محتوا، اینجا ظاهر می‌شود."
          ctaLabel="پرسش از AI"
          ctaHref="/search"
          badge="Articles"
        />
      ) : (
        <div className="grid gap-4 lg:grid-cols-2">
          {featured ? (
            <ArticleCardPro
              featured
              title={featured.title}
              href={publicHrefForContent(featured)}
              category={labelForContentType(featured.type)}
              readingTime={estimateReadingLabel(featured.title)}
              difficulty={softDifficulty(featured.type)}
              tags={inferTechTags(featured.title, featured.slug)}
              aiSummary={softAiSummary(featured.title, featured.slug)}
              coverTone="indigo"
            />
          ) : null}
          <div className="grid gap-4 sm:grid-cols-2">
            {rest.slice(0, 4).map((item, index) => (
              <ArticleCardPro
                key={item.id}
                title={item.title}
                href={publicHrefForContent(item)}
                category={labelForContentType(item.type)}
                readingTime={estimateReadingLabel(item.title)}
                difficulty={softDifficulty(item.type)}
                tags={inferTechTags(item.title, item.slug)}
                aiSummary={softAiSummary(item.title, item.slug)}
                coverTone={index % 2 === 0 ? "violet" : "cyan"}
              />
            ))}
          </div>
        </div>
      )}
    </PublicSection>
  );
}
