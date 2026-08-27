import type { NewsArticle } from "@/types";

export const NEWS_TAGS = ["React", ".NET", "AI", "DevOps"] as const;

export const NEWS_CLOUD_TAGS = [
  "AI",
  "Cursor",
  "Claude",
  "MCP",
  "OpenAI",
  "NET",
  "NextJS",
  "DevOps",
  "Docker",
  "GitHub",
  "Copilot",
] as const;

export type NewsCloudTag = (typeof NEWS_CLOUD_TAGS)[number] | "همه";

export type NewsCategoryId =
  | "همه"
  | "AI"
  | "Programming"
  | ".NET"
  | "Frontend"
  | "Backend"
  | "DevOps"
  | "Tools"
  | "Security";

export const NEWS_CATEGORY_FILTERS: readonly {
  id: NewsCategoryId;
  label: string;
  icon: string;
}[] = [
  { id: "همه", label: "همه", icon: "all" },
  { id: "AI", label: "AI", icon: "ai" },
  { id: "Programming", label: "Programming", icon: "code" },
  { id: ".NET", label: ".NET", icon: "dotnet" },
  { id: "Frontend", label: "Frontend", icon: "frontend" },
  { id: "Backend", label: "Backend", icon: "backend" },
  { id: "DevOps", label: "DevOps", icon: "devops" },
  { id: "Tools", label: "Tools", icon: "tools" },
  { id: "Security", label: "Security", icon: "security" },
];

/** Reference screenshot articles — Persian copy + extracted covers. */
export const NEWS_ARTICLES: NewsArticle[] = [
  {
    id: "1",
    title: "معرفی Cursor 1.0؛ نسل جدید AI IDE برای توسعه‌دهندگان",
    tag: "AI",
    categoryLabel: "AI",
    summary:
      "Cursor با قابلیت‌های پیشرفته کدنویسی هوشمند، تجربه توسعه را متحول کرده و سرعت کار تیم‌ها را چند برابر می‌کند.",
    time: "۲ ساعت پیش",
    image: "/news/cover-cursor.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۱۲.۴K",
  },
  {
    id: "2",
    title: "Claude چیست؟ همه چیز درباره Terminal Agent جدید Anthropic",
    tag: "AI",
    categoryLabel: "AI",
    summary:
      "نگاهی کامل به قابلیت‌های Claude Code و اینکه چطور می‌تواند جایگزین دستیار کدنویسی فعلی شما شود.",
    time: "۴ ساعت پیش",
    image: "/news/cover-claude.png",
    readTime: "۷ دقیقه مطالعه",
    views: "۹.۸K",
  },
  {
    id: "3",
    title: "استاندارد جدید MCP؛ اتصال مدل‌ها به ابزارها آسان‌تر شد",
    tag: "DevOps",
    categoryLabel: "Tools",
    summary:
      "Model Context Protocol روشی یکپارچه برای اتصال LLMها به ابزارهای توسعه و داده‌های پروژه معرفی می‌کند.",
    time: "۶ ساعت پیش",
    image: "/news/cover-mcp.png",
    readTime: "۶ دقیقه مطالعه",
    views: "۷.۲K",
  },
  {
    id: "4",
    title: "GitHub Copilot Workspace؛ محیط توسعه هوشمند جدید",
    tag: "AI",
    categoryLabel: "Tools",
    summary:
      "Workspace مسیر جدیدی برای برنامه‌ریزی، پیاده‌سازی و بازبینی کد با کمک Copilot ارائه می‌دهد.",
    time: "۸ ساعت پیش",
    image: "/news/cover-copilot.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۵.۷K",
  },
  {
    id: "5",
    title: ".NET 9 منتشر شد؛ مرور کامل ویژگی‌ها و بهبودهای عملکردی",
    tag: ".NET",
    categoryLabel: ".NET",
    summary:
      "نسخه جدید .NET با بهینه‌سازی‌های runtime، APIهای تازه و تجربه بهتر برای اپلیکیشن‌های ابری همراه است.",
    time: "۱۰ ساعت پیش",
    image: "/news/cover-dotnet.png",
    readTime: "۸ دقیقه مطالعه",
    views: "۴.۳K",
  },
  {
    id: "6",
    title: "React 19 معرفی شد؛ تمام تغییرات مهم که باید بدانید",
    tag: "React",
    categoryLabel: "Frontend",
    summary:
      "از Actions تا بهبودهای Server Components؛ خلاصه‌ای از مهم‌ترین تغییرات React 19 برای تیم‌های فرانت‌اند.",
    time: "۱۲ ساعت پیش",
    image: "/news/cover-react.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۶.۱K",
  },
  {
    id: "7",
    title: "DevOps در ۲۰۲۴: بهترین ابزارها و روش‌های پیاده‌سازی",
    tag: "DevOps",
    categoryLabel: "DevOps",
    summary:
      "مرور ابزارها و الگوهای رایج CI/CD، observability و امنیت زنجیره تأمین در تیم‌های مدرن.",
    time: "۱ روز پیش",
    image: "/news/cover-devops.png",
    readTime: "۶ دقیقه مطالعه",
    views: "۳.۹K",
  },
];

export const NEWS_POPULAR = [
  {
    id: "1",
    title: "Cursor 1.0",
    summary: "تحولی بزرگ در AI Coding",
    views: "۱۲.۴K",
    image: "/news/cover-cursor.png",
  },
  {
    id: "2",
    title: "Claude چیست؟",
    summary: "Terminal Agent جدید Anthropic",
    views: "۹.۸K",
    image: "/news/cover-claude.png",
  },
  {
    id: "3",
    title: "استاندارد جدید MCP",
    summary: "اتصال مدل‌ها به ابزارها",
    views: "۷.۲K",
    image: "/news/cover-mcp.png",
  },
  {
    id: "4",
    title: "GitHub Copilot Workspace",
    summary: "محیط توسعه هوشمند جدید",
    views: "۵.۷K",
    image: "/news/cover-copilot.png",
  },
  {
    id: "5",
    title: ".NET 9 منتشر شد",
    summary: "ویژگی‌ها و تغییرات جدید",
    views: "۴.۳K",
    image: "/news/cover-dotnet.png",
  },
] as const;

function matchesCategory(article: NewsArticle, category: NewsCategoryId): boolean {
  if (category === "همه") return true;
  const hay = `${article.title} ${article.summary} ${article.tag}`.toLowerCase();
  switch (category) {
    case "AI":
      return article.tag === "AI";
    case "Programming":
      return article.tag === "React" || hay.includes("cursor") || hay.includes("کدنویسی");
    case ".NET":
      return article.tag === ".NET";
    case "Frontend":
      return article.tag === "React" || hay.includes("react");
    case "Backend":
      return article.tag === ".NET" || hay.includes("api");
    case "DevOps":
      return article.tag === "DevOps" || hay.includes("mcp");
    case "Tools":
      return hay.includes("copilot") || hay.includes("workspace") || hay.includes("ابزار");
    case "Security":
      return hay.includes("امنیت") || hay.includes("security");
    default:
      return true;
  }
}

export function filterNewsArticles(
  articles: NewsArticle[],
  category: NewsCategoryId,
  cloudTag: NewsCloudTag,
): NewsArticle[] {
  let next = articles.filter((article) => matchesCategory(article, category));
  if (cloudTag !== "همه") {
    const key = cloudTag.toLowerCase();
    next = next.filter((article) => {
      const hay = `${article.title} ${article.summary} ${article.tag}`.toLowerCase();
      return hay.includes(key) || article.tag.toLowerCase().includes(key);
    });
  }
  return next;
}

export function formatNewsViewsShort(views: string): string {
  return views.replace(/\s*بازدید\s*$/u, "").trim();
}
