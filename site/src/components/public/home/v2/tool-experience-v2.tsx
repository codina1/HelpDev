import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { SmartEmptyState } from "@/components/experience/smart-empty-state";
import { ToolCardPro } from "@/components/public/pro/tool-card-pro";
import { PublicSection } from "@/components/ui/public/v2";
import type { ContentSummaryDto } from "@/lib/api/content";
import type { ToolSummaryDto } from "@/lib/api/toolbox";
import { inferTechTags, softUseCases } from "@/lib/public/display-meta";

type Props = {
  tools: ToolSummaryDto[];
  contentTools?: ContentSummaryDto[];
};

export function ToolExperienceV2({ tools, contentTools = [] }: Props) {
  const cards =
    tools.length > 0
      ? tools.slice(0, 6).map((t) => ({
          key: t.id,
          title: t.title,
          href: `/tools/${encodeURIComponent(t.slug)}`,
          category: t.categorySlug ?? "Tool",
          slug: t.slug,
        }))
      : contentTools.slice(0, 6).map((t) => ({
          key: t.id,
          title: t.title,
          href: `/tools/${encodeURIComponent(t.slug)}`,
          category: "Tool",
          slug: t.slug,
        }));

  return (
    <PublicSection
      className="ds-slide bg-[color:color-mix(in_srgb,var(--ds-bg-elevated)_85%,transparent)]"
      aria-labelledby="tool-exp-title"
    >
      <PremiumSectionHeader
        eyebrow="Tools"
        title="ابزارهای کار"
        description="ابزارهایی که توسعه نرم‌افزار را سریع‌تر می‌کنند — از Toolbox API"
        href="/toolbox"
        ctaLabel="مشاهده همه ابزارها"
        titleId="tool-exp-title"
        icon={<span aria-hidden>🛠️</span>}
      />

      {cards.length === 0 ? (
        <SmartEmptyState
          title="ابزاری برای نمایش نیست"
          description="پس از انتشار ابزار در Toolbox، ویترین اینجا پر می‌شود."
          ctaLabel="رفتن به Toolbox"
          ctaHref="/toolbox"
          badge="Tools"
        />
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {cards.map((card) => (
            <ToolCardPro
              key={card.key}
              title={card.title}
              href={card.href}
              category={card.category}
              useCases={softUseCases(card.category)}
              stackTags={inferTechTags(card.title, card.slug)}
            />
          ))}
        </div>
      )}
    </PublicSection>
  );
}
