import Link from "next/link";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomePathCard, type HomePathItem } from "@/components/public/home/home-path-card";

export const HOME_PATH_ITEMS: readonly HomePathItem[] = [
  {
    id: "architect",
    title: "Software Architect",
    description: "طراحی سیستم، مرز ماژول‌ها و تصمیم‌های معماری پایدار.",
    href: "/roadmap?track=software-architect",
    learners: 0,
    visual: "architect",
  },
  {
    id: "frontend",
    title: "Frontend Developer",
    description: "رابط کاربری، تجربه محصول و مسیر ساخت فرانت‌اند.",
    href: "/roadmap?track=frontend-developer",
    learners: 0,
    visual: "frontend",
  },
  {
    id: "devops",
    title: "DevOps Engineer",
    description: "تحویل پیوسته، زیرساخت و پایداری سیستم در تولید.",
    href: "/roadmap?track=devops-engineer",
    learners: 0,
    visual: "devops",
  },
  {
    id: "ai",
    title: "AI Engineer",
    description: "سیستم‌های هوشمند، دانش بازیابی‌شده و ساخت محصول AI.",
    href: "/roadmap?track=ai-engineer",
    learners: 0,
    visual: "ai",
  },
  {
    id: "backend",
    title: "Backend Developer",
    description: "API، داده و خدمات سمت سرور برای محصول واقعی.",
    href: "/roadmap?track=backend-developer",
    learners: 0,
    visual: "backend",
  },
];

type PublishedPath = {
  title: string;
  slug: string;
};

type HomePathsSectionProps = {
  roadmaps?: PublishedPath[];
};

const TRACK_MATCH: Record<string, string[]> = {
  architect: ["architect", "معمار"],
  frontend: ["frontend", "فرانت"],
  devops: ["devops", "دواپس"],
  ai: ["ai engineer", "ai-engineer", "مهندس ai"],
  backend: ["backend", "بک‌اند", "بک اند"],
};

function resolveHref(item: HomePathItem, roadmaps: PublishedPath[]): string {
  const keys = TRACK_MATCH[item.id] ?? [];
  const hit = roadmaps.find((roadmap) => {
    const title = roadmap.title.toLowerCase();
    return keys.some((key) => title.includes(key));
  });
  return hit ? `/roadmap?slug=${encodeURIComponent(hit.slug)}` : item.href;
}

/**
 * Homepage engineering learning paths — horizontal cards, honest learner counts.
 */
export function HomePathsSection({ roadmaps = [] }: HomePathsSectionProps) {
  const items = HOME_PATH_ITEMS.map((item) => ({
    ...item,
    href: resolveHref(item, roadmaps),
  }));

  return (
    <PublicSection
      className="home-paths home-reveal"
      containerSize="wide"
      aria-labelledby="home-paths-heading"
    >
      <div className="mb-8 flex flex-wrap items-end justify-between gap-3 sm:mb-10">
        <div className="max-w-xl text-start">
          <h2 id="home-paths-heading" className="home-section-title">
            مسیرهای یادگیری نقش‌محور
          </h2>
          <p
            className="mt-3 text-[color:var(--home-text-muted)]"
            style={{
              fontSize: "var(--home-body-size)",
              lineHeight: "var(--home-body-leading)",
            }}
          >
            مسیر نقش‌محور برای معمار نرم‌افزار، فرانت‌اند، DevOps، AI و بک‌اند.
          </p>
        </div>
        <Link
          href="/roadmap"
          className="home-section-more focus-ring"
        >
          همه مسیرها
        </Link>
      </div>

      <ul className="home-paths-scroller">
        {items.map((item) => (
          <HomePathCard key={item.id} item={item} />
        ))}
      </ul>
    </PublicSection>
  );
}
