import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { HomeStat, type HomeStatItem } from "@/components/public/home/home-stat";

type HomeStatsStripProps = {
  items: HomeStatItem[];
};

/**
 * Compact glass statistics bar directly under the homepage hero.
 */
export function HomeStatsStrip({ items }: HomeStatsStripProps) {
  return (
    <section aria-label="آمار پلتفرم" className="home-stats home-reveal">
      <PublicContainer size="wide">
        <div className="home-stats-row flex flex-wrap backdrop-blur-xl">
          {items.map((item, index) => (
            <HomeStat key={item.id} item={item} separated={index > 0} />
          ))}
        </div>
      </PublicContainer>
    </section>
  );
}
