import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { HomeStat, type HomeStatItem } from "@/components/public/home/home-stat";

type HomeStatsStripProps = {
  items: HomeStatItem[];
};

/**
 * Glass statistics strip directly under the homepage hero.
 */
export function HomeStatsStrip({ items }: HomeStatsStripProps) {
  return (
    <section aria-label="آمار پلتفرم" className="relative pb-8 sm:pb-10">
      <PublicContainer size="wide">
        <div
          className="flex flex-wrap overflow-hidden rounded-[var(--home-radius-xl)] border backdrop-blur-xl"
          style={{
            background: "var(--home-surface)",
            borderColor: "var(--home-border)",
            boxShadow: "var(--home-shadow-sm), 0 0 24px color-mix(in srgb, var(--home-purple) 10%, transparent)",
          }}
        >
          {items.map((item, index) => (
            <HomeStat key={item.id} item={item} separated={index > 0} />
          ))}
        </div>
      </PublicContainer>
    </section>
  );
}
