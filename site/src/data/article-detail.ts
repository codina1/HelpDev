import { MARKETPLACE_ARTICLES, type MarketplaceArticle } from "@/data/articles";
import type { ContentDetailDto } from "@/lib/api/content";
import {
  formatDateFa,
  resolveContentCoverUrl,
  shortAuthorId,
} from "@/lib/admin/content/content-mappers";
import { estimateReadingLabel } from "@/lib/public/display-meta";
import {
  extractTocFromBody,
  extractTocFromHtml,
  isBlockArticle,
  type TocHeading,
} from "@/lib/public/content-helpers";

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
  image?: string;
  durationLabel?: string;
};

export type ArticleRoadmapCta = {
  title: string;
  description: string;
  href: string;
  ctaLabel: string;
};

export type ArticleRelatedNewsItem = {
  id: string;
  title: string;
  href: string;
  image: string;
  dateLabel: string;
};

export type ArticleDetailViewModel = {
  id: string;
  title: string;
  slug: string;
  category: string;
  description: string;
  coverImage: string | null;
  contentHtml: string | null;
  contentBody: string;
  usesBlocks: boolean;
  author: ArticleDetailAuthor;
  displayAuthor: string;
  tags: string[];
  views: number;
  viewsLabel: string;
  readingTime: string;
  publishedAtLabel: string;
  isFeatured: boolean;
  breadcrumb: { label: string; href?: string }[];
  toc: TocHeading[];
  relatedNews: ArticleRelatedNewsItem[];
  relatedArticles: MarketplaceArticle[];
  relatedCourse: ArticleRelatedCourse;
  roadmap: ArticleRoadmapCta;
  tools: readonly ArticleRelatedTool[];
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
  title: "مسیر یادگیری Frontend",
  description: "از مبانی تا Server Components و الگوهای حرفه‌ای",
  href: "/courses/react-19",
  coverTone: "from-[#61DAFB]/35 to-[#7C3AED]/20",
  image: "/courses/course-react.png",
  durationLabel: "۱۲ ساعت",
};

const ROADMAP_CTA: ArticleRoadmapCta = {
  title: "Frontend Developer Roadmap",
  description: "نقشه راه ساخت اپلیکیشن‌های مدرن با React و Next.js",
  href: "/roadmap",
  ctaLabel: "مشاهده مسیر",
};

const CATEGORY_TAGS: Record<string, string[]> = {
  frontend: ["React", "JavaScript", "Frontend", "Web Development", "Server Components"],
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
  if (type.includes("news")) return "خبر";
  return "فناوری‌های وب";
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
  if (plain.length > 160) return `${plain.slice(0, 160).trim()}…`;
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

  const isNews = article.type.toLowerCase() === "news";
  const hubHref = isNews ? "/news" : "/articles";
  const hubLabel = isNews ? "اخبار" : "مقالات";

  return [
    { label: "خانه", href: "/" },
    { label: hubLabel, href: hubHref },
    { label: category, href: hubHref },
    { label: topic, href: hubHref },
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
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return views.toLocaleString("fa-IR");
}

function toRelatedNews(items: MarketplaceArticle[], hub: "articles" | "news" = "articles"): ArticleRelatedNewsItem[] {
  return items.slice(0, 3).map((item) => ({
    id: item.id,
    title: item.title,
    href: `/${hub}/${item.slug}`,
    image: item.coverImage || "/home/cover-architecture.svg",
    dateLabel: formatDateFa(item.publishedAt) || "—",
  }));
}

/** Build UI-ready view-model for the premium article detail page. */
export function buildArticleDetailViewModel(article: ContentDetailDto): ArticleDetailViewModel {
  const usesBlocks = isBlockArticle(article.contentFormat, article.contentHtml);
  const toc = usesBlocks
    ? extractTocFromHtml(article.contentHtml ?? "")
    : extractTocFromBody(article.body ?? "");

  const readingMinutes = article.readingTimeMinutes;
  const readingTime = readingMinutes
    ? `${readingMinutes.toLocaleString("fa-IR")} دقیقه مطالعه`
    : estimateReadingLabel(article.title);

  const author = resolveArticleAuthor(article);
  const related = resolveRelatedArticles(article.slug, 6);
  const hub = article.type.toLowerCase() === "news" ? "news" : "articles";

  return {
    id: article.id,
    title: article.title,
    slug: article.slug,
    category: resolveArticleCategoryLabel(article),
    description: resolveArticleExcerpt(article),
    coverImage: resolveContentCoverUrl(article.coverImage) || null,
    contentHtml: article.contentHtml ?? null,
    contentBody: article.body ?? "",
    usesBlocks,
    author,
    displayAuthor: author.name || shortAuthorId(article.authorId),
    tags: resolveArticleTags(article),
    views: article.views,
    viewsLabel: formatViewsShort(article.views),
    readingTime,
    publishedAtLabel: formatDateFa(article.createdAt) || "—",
    isFeatured: (article.views ?? 0) >= 500 || Boolean(resolveMarketplaceMatch(article.slug)?.featured),
    breadcrumb: resolveBreadcrumbTrail(article),
    toc,
    relatedNews: toRelatedNews(related, hub),
    relatedArticles: related.slice(0, 3),
    relatedCourse: getArticleRelatedCourse(),
    roadmap: getArticleRoadmapCta(),
    tools: getArticleRelatedTools(),
  };
}
