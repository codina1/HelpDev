import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

/**
 * Icon asset slots — replace files under /public/home without changing component code.
 */
export const LEARNING_PATH_ICON_SLOTS = {
  frontend: "/home/icon-frontend.png",
  devops: "/home/icon-devops.png",
  dotnet: "/home/icon-dotnet.png",
  backend: "/home/icon-backend.png",
  ai: "/home/icon-ai.png",
} as const;

export type LearningPathId = keyof typeof LEARNING_PATH_ICON_SLOTS;

export type LearningPathItem = {
  id: LearningPathId;
  title: string;
  description: string;
  href: string;
  levelLabel: string;
  progress: number;
  iconSrc: string;
};

export const LEARNING_PATH_ITEMS: readonly LearningPathItem[] = [
  {
    id: "frontend",
    title: "Frontend Developer",
    description: "رابط کاربری و تجربه محصول.",
    href: "/roadmap?track=frontend-developer",
    levelLabel: "میانی",
    progress: 62,
    iconSrc: LEARNING_PATH_ICON_SLOTS.frontend,
  },
  {
    id: "devops",
    title: "DevOps Engineer",
    description: "تحویل پیوسته و پایداری تولید.",
    href: "/roadmap?track=devops-engineer",
    levelLabel: "پیشرفته",
    progress: 48,
    iconSrc: LEARNING_PATH_ICON_SLOTS.devops,
  },
  {
    id: "dotnet",
    title: ".NET Developer",
    description: "ASP.NET Core و اکوسیستم .NET.",
    href: "/roadmap?track=dotnet-developer",
    levelLabel: "میانی",
    progress: 55,
    iconSrc: LEARNING_PATH_ICON_SLOTS.dotnet,
  },
  {
    id: "backend",
    title: "Backend Developer",
    description: "API، داده و خدمات سمت سرور.",
    href: "/roadmap?track=backend-developer",
    levelLabel: "میانی",
    progress: 58,
    iconSrc: LEARNING_PATH_ICON_SLOTS.backend,
  },
  {
    id: "ai",
    title: "AI Engineer",
    description: "سیستم‌های هوشمند و ساخت محصول AI.",
    href: "/roadmap?track=ai-engineer",
    levelLabel: "پیشرفته",
    progress: 40,
    iconSrc: LEARNING_PATH_ICON_SLOTS.ai,
  },
] as const;

type PublishedPath = {
  title: string;
  slug: string;
};

type LearningPathsSectionProps = {
  roadmaps?: PublishedPath[];
};

const TRACK_MATCH: Record<LearningPathId, string[]> = {
  frontend: ["frontend", "فرانت"],
  devops: ["devops", "دواپس"],
  dotnet: ["dotnet", ".net", "asp.net"],
  backend: ["backend", "بک‌اند", "بک اند"],
  ai: ["ai engineer", "ai-engineer", "مهندس ai"],
};

function resolveHref(item: LearningPathItem, roadmaps: PublishedPath[]): string {
  const keys = TRACK_MATCH[item.id] ?? [];
  const hit = roadmaps.find((roadmap) => {
    const title = roadmap.title.toLowerCase();
    return keys.some((key) => title.includes(key));
  });
  return hit ? `/roadmap?slug=${encodeURIComponent(hit.slug)}` : item.href;
}

/**
 * Learning Paths — Design Reference glass cards with replaceable icon assets.
 */
export function LearningPathsSection({ roadmaps = [] }: LearningPathsSectionProps) {
  const items = LEARNING_PATH_ITEMS.map((item) => ({
    ...item,
    href: resolveHref(item, roadmaps),
  }));

  return (
    <section
      className="home-learning-paths relative bg-[#050816] py-10 sm:py-12 lg:py-14"
      aria-labelledby="learning-paths-heading"
    >
      <PublicContainer size="wide">
        <div className="mb-7 flex flex-wrap items-end justify-between gap-4 sm:mb-9">
          <div className="max-w-xl text-start">
            <h2
              id="learning-paths-heading"
              className="text-[1.45rem] font-extrabold tracking-tight text-white sm:text-[1.7rem]"
            >
              مسیرهای یادگیری
            </h2>
            <p className="mt-2.5 text-[14px] leading-7 text-[#94A3B8]">
              مسیرهای نقش‌محور برای رشد در Frontend، Backend، .NET، DevOps و AI
            </p>
          </div>
          <Link
            href="/roadmap"
            className="focus-ring inline-flex items-center gap-1.5 text-[13px] font-semibold text-[#A78BFA] no-underline transition hover:text-white"
          >
            مشاهده همه مسیرها
            <ChevronIcon />
          </Link>
        </div>

        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-5">
          {items.map((item) => (
            <LearningPathCard key={item.id} item={item} />
          ))}
        </ul>
      </PublicContainer>
    </section>
  );
}

function LearningPathCard({ item }: { item: LearningPathItem }) {
  const progress = Math.max(0, Math.min(100, item.progress));

  return (
    <li className="min-w-0">
      <Link
        href={item.href}
        className="group focus-ring flex h-full flex-col overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#0B1224] p-4 no-underline transition duration-300 hover:-translate-y-[6px] hover:border-[rgba(124,58,237,0.45)] hover:shadow-[0_0_32px_rgba(124,58,237,0.28)] sm:p-5"
      >
        <span
          className="mx-auto flex h-[72px] w-[72px] items-center justify-center drop-shadow-[0_10px_24px_rgba(124,58,237,0.35)] transition duration-300 group-hover:scale-110"
          aria-hidden
        >
          {/* Icon slot — swap LEARNING_PATH_ICON_SLOTS assets */}
          <img
            src={item.iconSrc}
            alt=""
            width={72}
            height={72}
            decoding="async"
            className="h-full w-full object-contain"
            data-icon-slot={item.id}
          />
        </span>

        <h3 className="mt-4 text-center text-[15px] font-bold leading-snug text-white sm:text-[16px]">
          {item.title}
        </h3>
        <p className="mt-1.5 line-clamp-2 text-center text-[12px] leading-6 text-[#94A3B8]">
          {item.description}
        </p>

        <div className="mt-auto pt-5">
          <div className="mb-2 flex items-center justify-between gap-2 text-[11px] font-semibold text-[#94A3B8]">
            <span>{item.levelLabel}</span>
            <span>{progress.toLocaleString("fa-IR")}٪</span>
          </div>
          <div
            className="h-1.5 overflow-hidden rounded-full bg-white/[0.06]"
            role="progressbar"
            aria-valuenow={progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`پیشرفت مسیر ${item.title}`}
          >
            <span
              className="block h-full rounded-full bg-gradient-to-l from-[#7C3AED] to-[#2563EB] shadow-[0_0_12px_rgba(124,58,237,0.55)]"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      </Link>
    </li>
  );
}

function ChevronIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <path d="M15 6 9 12l6 6" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}
