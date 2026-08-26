import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

/**
 * Icon asset slots — replace files under /public/home without changing component code.
 */
export const LEARNING_PATH_ICON_SLOTS = {
  ai: "/home/icon-ai.png",
  backend: "/home/icon-backend.png",
  dotnet: "/home/icon-dotnet.png",
  devops: "/home/icon-devops.png",
  frontend: "/home/icon-frontend.png",
} as const;

export type LearningPathId = keyof typeof LEARNING_PATH_ICON_SLOTS;

export type LearningPathItem = {
  id: LearningPathId;
  title: string;
  description: string;
  href: string;
  lessons: number;
  progress: number;
  iconSrc: string;
};

export const LEARNING_PATH_ITEMS: readonly LearningPathItem[] = [
  {
    id: "ai",
    title: "AI Engineer",
    description: "مسیر جامع مهندسی هوش مصنوعی",
    href: "/roadmap?track=ai-engineer",
    lessons: 48,
    progress: 42,
    iconSrc: LEARNING_PATH_ICON_SLOTS.ai,
  },
  {
    id: "backend",
    title: "Backend Developer",
    description: "تبدیل شدن به توسعه‌دهنده بک‌اند",
    href: "/roadmap?track=backend-developer",
    lessons: 36,
    progress: 58,
    iconSrc: LEARNING_PATH_ICON_SLOTS.backend,
  },
  {
    id: "dotnet",
    title: ".NET Developer",
    description: "تسلط بر اکوسیستم دات‌نت",
    href: "/roadmap?track=dotnet-developer",
    lessons: 40,
    progress: 55,
    iconSrc: LEARNING_PATH_ICON_SLOTS.dotnet,
  },
  {
    id: "devops",
    title: "DevOps Engineer",
    description: "تسلط بر CI/CD و زیرساخت‌ها",
    href: "/roadmap?track=devops-engineer",
    lessons: 32,
    progress: 48,
    iconSrc: LEARNING_PATH_ICON_SLOTS.devops,
  },
  {
    id: "frontend",
    title: "Frontend Developer",
    description: "توسعه رابط کاربری مدرن",
    href: "/roadmap?track=frontend-developer",
    lessons: 38,
    progress: 62,
    iconSrc: LEARNING_PATH_ICON_SLOTS.frontend,
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
  ai: ["ai engineer", "ai-engineer", "مهندس ai"],
  backend: ["backend", "بک‌اند", "بک اند"],
  dotnet: ["dotnet", ".net", "asp.net"],
  devops: ["devops", "دواپس"],
  frontend: ["frontend", "فرانت"],
};

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

function resolveHref(item: LearningPathItem, roadmaps: PublishedPath[]): string {
  const keys = TRACK_MATCH[item.id] ?? [];
  const hit = roadmaps.find((roadmap) => {
    const title = roadmap.title.toLowerCase();
    return keys.some((key) => title.includes(key));
  });
  return hit ? `/roadmap?slug=${encodeURIComponent(hit.slug)}` : item.href;
}

/**
 * Learning Paths — Design Reference compact premium cards (220×130).
 * Desktop 5 · Tablet 3 · Mobile 1.
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
        <div className="mb-7 flex flex-wrap items-end justify-between gap-4 sm:mb-8">
          <h2
            id="learning-paths-heading"
            className="text-start text-[1.45rem] font-extrabold tracking-tight text-white sm:text-[1.7rem]"
          >
            مسیرهای یادگیری
          </h2>
          <Link
            href="/roadmap"
            className="focus-ring inline-flex items-center gap-1.5 text-[13px] font-semibold text-[#A78BFA] no-underline transition hover:text-white"
          >
            مشاهده همه مسیرها
            <ChevronIcon />
          </Link>
        </div>

        <ul className="grid grid-cols-1 justify-items-center gap-4 sm:grid-cols-3 lg:grid-cols-5 lg:justify-items-stretch">
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
    <li className="w-full max-w-[220px] min-w-0 lg:max-w-none">
      <Link
        href={item.href}
        className="group focus-ring relative flex h-[130px] w-full max-w-[220px] flex-col overflow-hidden rounded-[18px] border border-white/[0.08] bg-[#0B1224] px-3.5 pb-3 pt-3 no-underline transition duration-300 hover:-translate-y-[6px] hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_0_32px_rgba(124,58,237,0.3)] lg:max-w-none"
      >
        <span
          className="pointer-events-none absolute inset-0 opacity-0 transition duration-300 group-hover:opacity-100"
          style={{
            background:
              "radial-gradient(ellipse 90% 70% at 50% 0%, rgba(124,58,237,0.2), transparent 70%)",
          }}
          aria-hidden
        />

        <span
          className="relative mx-auto flex h-9 w-9 shrink-0 items-center justify-center drop-shadow-[0_8px_18px_rgba(124,58,237,0.4)] transition duration-300 group-hover:scale-110"
          aria-hidden
        >
          <img
            src={item.iconSrc}
            alt=""
            width={36}
            height={36}
            decoding="async"
            className="h-9 w-9 object-contain"
            data-icon-slot={item.id}
          />
        </span>

        <div className="relative mt-1.5 min-h-0 flex-1 text-center">
          <h3 className="truncate text-[13px] font-bold leading-tight text-white">{item.title}</h3>
          <p className="mt-0.5 line-clamp-2 text-[10px] leading-4 text-[#94A3B8]">{item.description}</p>
        </div>

        <div className="relative mt-auto pt-1.5">
          <div className="mb-1 flex items-center justify-between gap-2 text-[10px] font-semibold text-[#94A3B8]">
            <span>{NUMBER_FA.format(item.lessons)} درس</span>
            <span>{NUMBER_FA.format(progress)}٪</span>
          </div>
          <div
            className="h-1 overflow-hidden rounded-full bg-white/[0.06]"
            role="progressbar"
            aria-valuenow={progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label={`پیشرفت مسیر ${item.title}`}
          >
            <span
              className="block h-full rounded-full bg-gradient-to-l from-[#7C3AED] to-[#2563EB] shadow-[0_0_10px_rgba(124,58,237,0.55)]"
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
