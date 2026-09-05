import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";

export type PromptDetailTabId =
  | "intro"
  | "prompt"
  | "usage"
  | "example"
  | "changelog";

export type PromptAiModelChip = {
  id: string;
  name: string;
  tone: string;
};

export type PromptUsageStep = {
  id: string;
  title: string;
  description: string;
  icon: "copy" | "input" | "detail" | "check";
};

export type PromptVersionRow = {
  id: string;
  version: string;
  dateLabel: string;
  summary: string;
  isLatest?: boolean;
};

export type PromptRelatedArticle = {
  id: string;
  title: string;
  href: string;
  image: string;
  viewsLabel: string;
};

export type PromptRelatedCourse = {
  id: string;
  title: string;
  href: string;
  image: string;
  durationLabel: string;
};

export type PromptRoadmapCard = {
  title: string;
  description: string;
  href: string;
  ctaLabel: string;
};

export type PromptDetailViewModel = {
  detail: PromptLabDetail;
  aiModels: PromptAiModelChip[];
  levelLabel: string;
  rating: number;
  ratingCount: number;
  language: string;
  usageSteps: PromptUsageStep[];
  sampleInput: string;
  sampleOutput: string;
  versions: PromptVersionRow[];
  similar: PromptLabCardItem[];
  relatedArticles: PromptRelatedArticle[];
  relatedCourses: PromptRelatedCourse[];
  roadmap: PromptRoadmapCard;
  breadcrumb: { label: string; href?: string }[];
};

export const PROMPT_DETAIL_TABS: { id: PromptDetailTabId; label: string }[] = [
  { id: "intro", label: "معرفی" },
  { id: "prompt", label: "پرامپت" },
  { id: "usage", label: "نحوه استفاده" },
  { id: "example", label: "نمونه خروجی" },
  { id: "changelog", label: "تغییرات" },
];

const DEFAULT_MODELS: PromptAiModelChip[] = [
  { id: "chatgpt", name: "ChatGPT", tone: "from-emerald-400/30 to-emerald-600/10" },
  { id: "claude", name: "Claude", tone: "from-orange-400/30 to-amber-600/10" },
  { id: "gemini", name: "Gemini", tone: "from-sky-400/30 to-blue-600/10" },
  { id: "copilot", name: "Copilot", tone: "from-violet-400/30 to-purple-600/10" },
];

const DEFAULT_USAGE: PromptUsageStep[] = [
  {
    id: "1",
    title: "کپی پرامپت",
    description: "متن کامل را با یک کلیک کپی کنید.",
    icon: "copy",
  },
  {
    id: "2",
    title: "در مدل AI وارد کنید",
    description: "پرامپت را در ChatGPT، Claude یا مدل دلخواه بچسبانید.",
    icon: "input",
  },
  {
    id: "3",
    title: "نیازمندی‌ها را اضافه کنید",
    description: "جزئیات پروژه، محدودیت‌ها و خروجی مطلوب را بنویسید.",
    icon: "detail",
  },
  {
    id: "4",
    title: "خروجی را بررسی کنید",
    description: "نتیجه را بازبینی، اصلاح و در پروژه استفاده کنید.",
    icon: "check",
  },
];

export function formatCompactCount(value: number): string {
  if (value >= 1000) {
    const n = value / 1000;
    const rounded = n >= 10 ? Math.round(n) : Math.round(n * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return value.toLocaleString("fa-IR");
}

export function formatPromptDate(iso: string): string {
  if (!iso) return "—";
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return "—";
  try {
    return new Intl.DateTimeFormat("fa-IR-u-ca-persian", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    }).format(date);
  } catch {
    return date.toLocaleDateString("fa-IR");
  }
}

function inferLanguage(content: string): string {
  if (/```tsx|:\s*React\.|jsx/i.test(content)) return "TSX";
  if (/```ts|TypeScript|interface\s+/i.test(content)) return "TS";
  if (/```python|def\s+/i.test(content)) return "PY";
  return "TXT";
}

function buildSampleOutput(title: string): string {
  return `import type { FC } from "react";

type UserCardProps = {
  name: string;
  role: string;
};

export const UserCard: FC<UserCardProps> = ({ name, role }) => {
  return (
    <article className="rounded-xl border border-white/10 bg-[#0B1224] p-4">
      <h3 className="text-sm font-bold text-white">{name}</h3>
      <p className="mt-1 text-xs text-slate-400">{role}</p>
    </article>
  );
};

// Generated for: ${title}
`;
}

/** Enrich API prompt detail into premium page view-model (UI-ready fields). */
export function buildPromptDetailViewModel(input: {
  detail: PromptLabDetail;
  similar: readonly PromptLabCardItem[];
}): PromptDetailViewModel {
  const { detail, similar } = input;
  const primaryModel = detail.aiModel?.trim() || "ChatGPT";
  const models = [
    { id: "primary", name: primaryModel, tone: "from-violet-400/35 to-fuchsia-600/10" },
    ...DEFAULT_MODELS.filter((m) => m.name.toLowerCase() !== primaryModel.toLowerCase()).slice(0, 3),
  ];

  const rating = 4.9;
  const ratingCount = Math.max(24, Math.round((detail.viewCount || 100) / 90));

  return {
    detail,
    aiModels: models,
    levelLabel: "Intermediate",
    rating,
    ratingCount,
    language: inferLanguage(detail.content),
    usageSteps: DEFAULT_USAGE,
    sampleInput: `Build a reusable React component for "${detail.title}".

Requirements:
- TypeScript
- Tailwind CSS
- Responsive
- Accessible
- Clean API`,
    sampleOutput: buildSampleOutput(detail.title),
    versions: [
      {
        id: "v12",
        version: "v1.2",
        dateLabel: formatPromptDate(detail.publishedAt) || "۱۴۰۴/۱۰/۱۵",
        summary: "بهبود خروجی و ساختار کامپوننت",
        isLatest: true,
      },
      {
        id: "v11",
        version: "v1.1",
        dateLabel: "۱۴۰۴/۰۹/۲۰",
        summary: "اضافه شدن TypeScript و Tailwind",
      },
      {
        id: "v10",
        version: "v1.0",
        dateLabel: "۱۴۰۴/۰۸/۰۱",
        summary: "انتشار اولیه",
      },
    ],
    similar: [...similar].slice(0, 4),
    relatedArticles: [
      {
        id: "a1",
        title: "راهنمای کامل کامپوننت‌های React",
        href: "/articles",
        image: "/courses/course-react.png",
        viewsLabel: "۸.۲K بازدید",
      },
    ],
    relatedCourses: [
      {
        id: "c1",
        title: "دوره React پیشرفته",
        href: "/courses/react-19",
        image: "/courses/course-react.png",
        durationLabel: "۶ ساعت",
      },
    ],
    roadmap: {
      title: "Frontend Developer Roadmap",
      description: "مسیر یادگیری ساخت اپلیکیشن‌های مدرن با React و Next.js",
      href: "/roadmap",
      ctaLabel: "مشاهده مسیر",
    },
    breadcrumb: [
      { label: "Prompt Lab", href: "/prompt-lab" },
      { label: detail.category, href: "/prompt-lab" },
      { label: detail.title },
    ],
  };
}
