import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";

/**
 * Icon asset slots — one unique uploaded icon per path (no semantic duplicates).
 * Files live under /public/home (icon-*.png) and /public/home/paths (path-*.png).
 */
export const LEARNING_PATH_ICON_SLOTS = {
  ai: "/home/paths/path-ai.png",
  backend: "/home/paths/path-backend.png",
  dotnet: "/home/paths/path-dotnet.png",
  devops: "/home/paths/path-devops.png",
  frontend: "/home/paths/path-frontend.png",
} as const;

export type LearningPathId = keyof typeof LEARNING_PATH_ICON_SLOTS;

export type LearningPathItem = {
  id: LearningPathId;
  title: string;
  description: string;
  href: string;
  lessons: string;
  difficulty: string;
  progress: number;
  iconSrc: string;
};

export const LEARNING_PATH_ITEMS: readonly LearningPathItem[] = [
  {
    id: "ai",
    title: "AI Engineer",
    description: "مسیر جامع مهندسی هوش مصنوعی",
    href: "/roadmap?track=ai-engineer",
    lessons: "28 درس",
    difficulty: "پیشرفته",
    progress: 60,
    iconSrc: LEARNING_PATH_ICON_SLOTS.ai,
  },
  {
    id: "backend",
    title: "Backend Developer",
    description: "تبدیل شدن به توسعه‌دهنده بک‌اند",
    href: "/roadmap?track=backend-developer",
    lessons: "32 درس",
    difficulty: "متوسط",
    progress: 40,
    iconSrc: LEARNING_PATH_ICON_SLOTS.backend,
  },
  {
    id: "dotnet",
    title: ".NET Developer",
    description: "تسلط بر اکوسیستم دات‌نت",
    href: "/roadmap?track=dotnet-developer",
    lessons: "24 درس",
    difficulty: "متوسط",
    progress: 55,
    iconSrc: LEARNING_PATH_ICON_SLOTS.dotnet,
  },
  {
    id: "devops",
    title: "DevOps Engineer",
    description: "تسلط بر CI/CD و زیرساخت‌ها",
    href: "/roadmap?track=devops-engineer",
    lessons: "27 درس",
    difficulty: "پیشرفته",
    progress: 65,
    iconSrc: LEARNING_PATH_ICON_SLOTS.devops,
  },
  {
    id: "frontend",
    title: "Frontend Developer",
    description: "توسعه رابط کاربری مدرن",
    href: "/roadmap?track=frontend-developer",
    lessons: "30 درس",
    difficulty: "متوسط",
    progress: 75,
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
 * Learning Paths — dark glass cards with uploaded 3D icons.
 * Desktop 5 · Tablet 3 · Mobile 1 · RTL.
 */
export function LearningPathsSection({ roadmaps = [] }: LearningPathsSectionProps) {
  const items = LEARNING_PATH_ITEMS.map((item) => ({
    ...item,
    href: resolveHref(item, roadmaps),
  }));

  return (
    <section
      className="home-learning-paths relative bg-[#050816] py-6 sm:py-7 lg:py-8"
      aria-labelledby="learning-paths-heading"
      dir="rtl"
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

        <ul className="grid grid-cols-1 gap-4 sm:grid-cols-3 lg:grid-cols-5">
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
        className="group focus-ring relative flex h-full min-h-[220px] w-full flex-col overflow-hidden rounded-[18px] border border-[rgba(255,255,255,0.08)] bg-[#0B1224]/80 px-4 pb-4 pt-5 no-underline shadow-[0_0_24px_rgba(124,58,237,0.12)] backdrop-blur-sm transition duration-300 hover:-translate-y-[6px] hover:border-[rgba(124,58,237,0.5)] hover:shadow-[0_16px_40px_rgba(2,6,23,0.55),0_0_36px_rgba(124,58,237,0.35)]"
      >
        <span
          className="pointer-events-none absolute inset-0 opacity-70 transition duration-300 group-hover:opacity-100"
          style={{
            background:
              "radial-gradient(ellipse 90% 70% at 50% 0%, rgba(124,58,237,0.18), transparent 70%)",
          }}
          aria-hidden
        />

        <span
          className="relative mx-auto flex h-16 w-16 shrink-0 items-center justify-center drop-shadow-[0_10px_22px_rgba(124,58,237,0.45)] transition duration-300 group-hover:scale-110"
          aria-hidden
        >
          <img
            src={item.iconSrc}
            alt=""
            width={64}
            height={64}
            decoding="async"
            className="h-16 w-16 object-contain"
            data-icon-slot={item.id}
          />
        </span>

        <div className="relative mt-3 min-h-0 flex-1 text-center">
          <h3 className="text-[15px] font-bold leading-tight text-white">{item.title}</h3>
          <p className="mt-1.5 line-clamp-2 text-[12px] leading-5 text-[#94A3B8]">{item.description}</p>
        </div>

        <div className="relative mt-auto pt-3">
          <div className="mb-2 flex items-center justify-between gap-2 text-[11px] font-semibold text-[#94A3B8]">
            <span>{item.lessons}</span>
            <span className="rounded-md bg-white/[0.06] px-1.5 py-0.5 text-[10px] text-[#C4B5FD]">
              {item.difficulty}
            </span>
          </div>
          <div className="mb-1.5 flex items-center justify-between gap-2 text-[10px] font-semibold text-[#64748B]">
            <span>پیشرفت</span>
            <span>{NUMBER_FA.format(progress)}٪</span>
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
