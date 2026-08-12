import { EngineeringTimeline } from "@/components/experience/engineering-timeline";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { SmartEmptyState } from "@/components/experience/smart-empty-state";
import { RoadmapCardPro } from "@/components/public/pro/roadmap-card-pro";
import { PublicSection } from "@/components/ui/public/v2";
import type { ContentSummaryDto } from "@/lib/api/content";
import { FRONTEND_PATH_DEMO } from "@/lib/public/nav-v2";
import { roadmapLevelLabel } from "@/lib/public/display-meta";

type Props = { items: ContentSummaryDto[] };

export function RoadmapExperienceV2({ items }: Props) {
  return (
    <PublicSection className="ds-slide" aria-labelledby="roadmap-exp-title">
      <PremiumSectionHeader
        eyebrow="Roadmaps"
        title="مسیرهای یادگیری مهندسی"
        description="سطوح، گام‌های باز/قفل، نشانگر تکمیل و پیش‌نمای پیشرفت ساخت‌یافته"
        href="/roadmap"
        ctaLabel="مشاهده همه مسیرها"
        titleId="roadmap-exp-title"
        icon={<span aria-hidden>🗺️</span>}
      />

      <div className="grid gap-4 lg:grid-cols-[1.05fr_0.95fr]">
        <EngineeringTimeline
          title={FRONTEND_PATH_DEMO.title}
          badge="ساختار نمایشی"
          level={roadmapLevelLabel(0)}
          note="الگوی بصری سطح، قفل و تکمیل — داده پیشرفت کاربر جعلی نیست."
          nodes={[...FRONTEND_PATH_DEMO.nodes]}
        />

        <div className="grid gap-4">
          {items.length === 0 ? (
            <SmartEmptyState
              title="مسیر مهندسی منتشرشده‌ای نیست"
              description="نقشه‌های راه Content API اینجا نمایش داده می‌شوند. می‌توانید از دستیار AI مسیر شخصی بسازید."
              ctaLabel="ساخت مسیر با AI"
              ctaHref="/learning/assistant"
              badge="Roadmap"
            />
          ) : (
            items.slice(0, 2).map((item, index) => (
              <RoadmapCardPro
                key={item.id}
                title={item.title}
                href={`/roadmap?slug=${encodeURIComponent(item.slug)}`}
                level={roadmapLevelLabel(index)}
                nodes={[...FRONTEND_PATH_DEMO.nodes]}
              />
            ))
          )}
        </div>
      </div>
    </PublicSection>
  );
}
