import { MARKETPLACE_ARTICLES, type MarketplaceArticle } from "@/data/articles";
import type { ContentDetailDto } from "@/lib/api/content";

export type ArticleDetailAuthor = {
  name: string;
  role: string;
  bio: string;
  initials: string;
  avatarUrl?: string;
};

export type ArticleRelatedTool = {
  id: string;
  name: string;
  description: string;
  href: string;
  iconTone: string;
};

export type ArticleRelatedCourse = {
  title: string;
  description: string;
  href: string;
  coverTone: string;
};

export type ArticleRoadmapCta = {
  title: string;
  description: string;
  href: string;
  ctaLabel: string;
};

const DEFAULT_AUTHOR: ArticleDetailAuthor = {
  name: "علیرضا محمدی",
  role: "Frontend Developer",
  bio: "توسعه‌دهنده فرانت‌اند با تمرکز روی React، Next.js و تجربه کاربری محصول‌های آموزشی.",
  initials: "عم",
};

const RELATED_TOOLS: readonly ArticleRelatedTool[] = [
  {
    id: "vscode",
    name: "VS Code",
    description: "ویرایشگر اصلی توسعه وب",
    href: "/toolbox",
    iconTone: "from-[#007ACC]/50 to-[#1E293B]",
  },
  {
    id: "react-devtools",
    name: "React DevTools",
    description: "دیباگ کامپوننت‌ها و پروفایل",
    href: "/toolbox",
    iconTone: "from-[#61DAFB]/40 to-[#0F172A]",
  },
  {
    id: "vercel",
    name: "Vercel",
    description: "دیپلوی سریع اپ‌های Next.js",
    href: "/toolbox",
    iconTone: "from-[#FFFFFF]/20 to-[#111827]",
  },
];

const RELATED_COURSE: ArticleRelatedCourse = {
  title: "دوره جامع React",
  description: "از مبانی تا Server Components و الگوهای حرفه‌ای",
  href: "/courses",
  coverTone: "from-[#61DAFB]/35 to-[#7C3AED]/20",
};

const ROADMAP_CTA: ArticleRoadmapCta = {
  title: "مسیر یادگیری Frontend Pro",
  description: "نقشه راه ساخت اپلیکیشن‌های مدرن با React و Next.js",
  href: "/roadmap",
  ctaLabel: "مشاهده مسیر",
};

const CATEGORY_TAGS: Record<string, string[]> = {
  frontend: ["React", "Frontend", "JavaScript", "Next.js", "Hooks", "UI"],
  ai: ["AI", "LLM", "Prompt", "Tools"],
  backend: ["Backend", "API", "Node.js"],
  devops: ["DevOps", "CI/CD", "Docker"],
  dotnet: [".NET", "C#", "ASP.NET"],
  programming: ["Programming", "Clean Code"],
  tools: ["Tools", "Developer Experience"],
  architecture: ["Architecture", "System Design"],
  security: ["Security", "Best Practices"],
};

export function resolveMarketplaceMatch(slug: string): MarketplaceArticle | undefined {
  return MARKETPLACE_ARTICLES.find((item) => item.slug === slug);
}

export function resolveArticleAuthor(article: ContentDetailDto): ArticleDetailAuthor {
  const apiName = article.authorName?.trim();
  if (apiName) {
    return {
      name: apiName,
      role: article.authorRole?.trim() || "نویسنده HelpDev",
      bio: article.authorBio?.trim() || `مقالات و محتوای تخصصی از ${apiName}.`,
      initials: initialsFromName(apiName),
      avatarUrl: article.authorAvatarUrl?.trim() || undefined,
    };
  }

  const match = resolveMarketplaceMatch(article.slug);
  if (match) {
    return {
      name: match.author,
      role: match.categoryLabel === "Frontend" ? "Frontend Developer" : "Technical Writer",
      bio: match.description,
      initials: match.authorInitials,
    };
  }

  return DEFAULT_AUTHOR;
}

function initialsFromName(name: string): string {
  const parts = name.replace(/[^\p{L}\p{N}\s]/gu, " ").trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "HD";
  if (parts.length === 1) return parts[0].slice(0, 2);
  return `${parts[0][0] ?? ""}${parts[1][0] ?? ""}`;
}

export function resolveArticleCategoryLabel(article: ContentDetailDto): string {
  const match = resolveMarketplaceMatch(article.slug);
  if (match) return match.categoryLabel;
  const type = article.type.toLowerCase();
  if (type.includes("news")) return "News";
  return "Article";
}

export function resolveArticleTags(article: ContentDetailDto): string[] {
  const match = resolveMarketplaceMatch(article.slug);
  const base = CATEGORY_TAGS[match?.category ?? "frontend"] ?? CATEGORY_TAGS.frontend;
  const fromTitle = article.title
    .split(/[\s،,?\-_/]+/)
    .map((part) => part.trim())
    .filter((part) => /^[A-Za-z0-9.]+$/.test(part) && part.length > 1)
    .slice(0, 3);
  return Array.from(new Set([...fromTitle, ...base])).slice(0, 8);
}

export function resolveRelatedArticles(currentSlug: string, limit = 3): MarketplaceArticle[] {
  return MARKETPLACE_ARTICLES.filter((item) => item.slug !== currentSlug).slice(0, limit);
}

export function resolveArticleExcerpt(article: ContentDetailDto): string {
  const match = resolveMarketplaceMatch(article.slug);
  if (match?.description) return match.description;
  const plain = (article.body ?? "")
    .replace(/```[\s\S]*?```/g, " ")
    .replace(/[#>*_`~\-\[\]\(\)!]/g, " ")
    .replace(/\s+/g, " ")
    .trim();
  if (plain.length > 140) return `${plain.slice(0, 140).trim()}…`;
  return plain || "بررسی تخصصی برای توسعه‌دهندگان و تیم‌های محصول.";
}

export function resolveBreadcrumbTrail(article: ContentDetailDto): { label: string; href?: string }[] {
  const match = resolveMarketplaceMatch(article.slug);
  const category = match?.categoryLabel ?? resolveArticleCategoryLabel(article);
  const topic =
    match?.category === "frontend"
      ? "React"
      : match?.category === "ai"
        ? "AI"
        : match?.category === "dotnet"
          ? ".NET"
          : category;

  return [
    { label: "خانه", href: "/" },
    { label: category, href: "/articles" },
    { label: topic, href: "/articles" },
    { label: article.title },
  ];
}

export function getArticleRelatedTools(): readonly ArticleRelatedTool[] {
  return RELATED_TOOLS;
}

export function getArticleRelatedCourse(): ArticleRelatedCourse {
  return RELATED_COURSE;
}

export function getArticleRoadmapCta(): ArticleRoadmapCta {
  return ROADMAP_CTA;
}

export function formatViewsShort(views: number): string {
  if (views >= 1000) {
    const value = views / 1000;
    const rounded = value >= 10 ? Math.round(value) : Math.round(value * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}k`;
  }
  return views.toLocaleString("fa-IR");
}
