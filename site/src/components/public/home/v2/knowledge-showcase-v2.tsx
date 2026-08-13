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
  const primary = items.slice(0, 2);
  const rest = items.slice(2, 6);

  return (
    <PublicSection className="ds-slide" aria-labelledby="knowledge-showcase-title">
      <PremiumSectionHeader
        eyebrow="Intelligence"
        title="مقالات منتخب"
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
          <div className="grid gap-4">
            {primary.map((item, index) => (
              <ShowcaseArticleCard
                key={item.id}
                item={item}
                coverTone={index === 0 ? "indigo" : "violet"}
              />
            ))}
          </div>
          {rest.length > 0 ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {rest.map((item, index) => (
                <ShowcaseArticleCard
                  key={item.id}
                  item={item}
                  coverTone={index % 2 === 0 ? "violet" : "cyan"}
                />
              ))}
            </div>
          ) : null}
        </div>
      )}
    </PublicSection>
  );
}

function ShowcaseArticleCard({
  item,
  coverTone,
}: {
  item: ContentSummaryDto;
  coverTone: "violet" | "cyan" | "indigo";
}) {
  return (
    <ArticleCardPro
      title={item.title}
      href={publicHrefForContent(item)}
      category={labelForContentType(item.type)}
      readingTime={estimateReadingLabel(item.title)}
      difficulty={softDifficulty(item.type)}
      tags={inferTechTags(item.title, item.slug)}
      aiSummary={softAiSummary(item.title, item.slug)}
      coverTone={coverTone}
    />
  );
}
