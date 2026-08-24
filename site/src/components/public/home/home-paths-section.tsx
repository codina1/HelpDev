import Link from "next/link";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomePathCard, type HomePathItem } from "@/components/public/home/home-path-card";

/** Compact role paths — must stay smaller than Articles / News. */
export const HOME_PATH_ITEMS: readonly HomePathItem[] = [
  {
    id: "ai",
    title: "AI Engineer",
    description: "سیستم‌های هوشمند و ساخت محصول AI.",
    href: "/roadmap?track=ai-engineer",
    learners: 0,
    visual: "ai",
  },
  {
    id: "backend",
    title: "Backend Developer",
    description: "API، داده و خدمات سمت سرور.",
    href: "/roadmap?track=backend-developer",
    learners: 0,
    visual: "backend",
  },
  {
    id: "dotnet",
    title: ".NET Developer",
    description: "ASP.NET Core و اکوسیستم .NET.",
    href: "/roadmap?track=dotnet-developer",
    learners: 0,
    visual: "architect",
  },
  {
    id: "frontend",
    title: "Frontend Developer",
    description: "رابط کاربری و تجربه محصول.",
    href: "/roadmap?track=frontend-developer",
    learners: 0,
    visual: "frontend",
  },
  {
    id: "devops",
    title: "DevOps Engineer",
    description: "تحویل پیوسته و پایداری تولید.",
    href: "/roadmap?track=devops-engineer",
    learners: 0,
    visual: "devops",
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
  ai: ["ai engineer", "ai-engineer", "مهندس ai"],
  backend: ["backend", "بک‌اند", "بک اند"],
  dotnet: ["dotnet", ".net", "asp.net"],
  frontend: ["frontend", "فرانت"],
  devops: ["devops", "دواپس"],
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
 * Compact learning-paths strip — five role cards only.
 */
export function HomePathsSection({ roadmaps = [] }: HomePathsSectionProps) {
  const items = HOME_PATH_ITEMS.map((item) => ({
    ...item,
    href: resolveHref(item, roadmaps),
  }));

  return (
    <PublicSection
      className="home-paths home-reveal py-6 sm:py-7 lg:py-8"
      containerSize="wide"
      aria-labelledby="home-paths-heading"
    >
      <div className="mb-4 flex flex-wrap items-end justify-between gap-3 sm:mb-5">
        <div className="max-w-xl text-start">
          <p className="text-[12px] font-bold tracking-wide text-[#06B6D4]">مسیرها</p>
          <h2 id="home-paths-heading" className="mt-1 text-[1.15rem] font-extrabold text-white sm:text-[1.25rem]">
            مسیرهای یادگیری
          </h2>
        </div>
        <Link href="/roadmap" className="focus-ring text-[12px] font-semibold text-[#94A3B8] no-underline hover:text-white">
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
