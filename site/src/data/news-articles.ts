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

/** Catalog fallback when News API content is empty — real slugs for detail routes. */
export const NEWS_ARTICLES: NewsArticle[] = [
  {
    id: "1",
    slug: "cursor-1-ai-ide",
    title: "معرفی Cursor 1.0؛ نسل جدید AI IDE",
    tag: "AI",
    categoryLabel: "AI",
    summary: "کدنویسی هوشمند با سرعتی چند برابر برای تیم‌های توسعه.",
    time: "۲ ساعت پیش",
    image: "/news/cover-cursor.png",
    readTime: "۵ دقیقه",
    views: "۱۲.۴K",
  },
  {
    id: "2",
    slug: "claude-terminal-agent",
    title: "Claude چیست؟ نگاهی به Terminal Agent",
    tag: "AI",
    categoryLabel: "AI",
    summary: "جایگزین تازه‌ای برای دستیار کدنویسی فعلی شما.",
    time: "۴ ساعت پیش",
    image: "/news/cover-claude.png",
    readTime: "۷ دقیقه",
    views: "۹.۸K",
  },
  {
    id: "3",
    slug: "mcp-standard-tools",
    title: "استاندارد MCP؛ اتصال مدل‌ها به ابزارها",
    tag: "DevOps",
    categoryLabel: "Tools",
    summary: "روشی یکپارچه برای وصل‌کردن LLMها به ابزارهای توسعه.",
    time: "۶ ساعت پیش",
    image: "/news/cover-mcp.png",
    readTime: "۶ دقیقه",
    views: "۷.۲K",
  },
  {
    id: "4",
    slug: "github-copilot-workspace",
    title: "GitHub Copilot Workspace معرفی شد",
    tag: "AI",
    categoryLabel: "Tools",
    summary: "برنامه‌ریزی، پیاده‌سازی و بازبینی کد با کمک Copilot.",
    time: "۸ ساعت پیش",
    image: "/news/cover-copilot.png",
    readTime: "۵ دقیقه",
    views: "۵.۷K",
  },
  {
    id: "5",
    slug: "dotnet-9-release",
    title: ".NET 9 منتشر شد؛ ویژگی‌ها و بهبودها",
    tag: ".NET",
    categoryLabel: ".NET",
    summary: "بهینه‌سازی runtime و APIهای تازه برای اپ‌های ابری.",
    time: "۱۰ ساعت پیش",
    image: "/news/cover-dotnet.png",
    readTime: "۸ دقیقه",
    views: "۴.۳K",
  },
  {
    id: "6",
    slug: "react-19-release",
    title: "React 19 معرفی شد؛ تغییرات مهم",
    tag: "React",
    categoryLabel: "Frontend",
    summary: "از Actions تا بهبودهای Server Components.",
    time: "۱۲ ساعت پیش",
    image: "/news/cover-react.png",
    readTime: "۵ دقیقه",
    views: "۶.۱K",
  },
  {
    id: "7",
    slug: "devops-2024-tools",
    title: "DevOps در ۲۰۲۴؛ بهترین ابزارها",
    tag: "DevOps",
    categoryLabel: "DevOps",
    summary: "الگوهای رایج CI/CD، observability و امنیت زنجیره تأمین.",
    time: "۱ روز پیش",
    image: "/news/cover-devops.png",
    readTime: "۶ دقیقه",
    views: "۳.۹K",
  },
];

export type PopularNewsItem = {
  id: string;
  slug: string;
  title: string;
  summary: string;
  time: string;
  views: string;
  image: string;
};

export const NEWS_POPULAR: PopularNewsItem[] = NEWS_ARTICLES.slice(0, 5).map((item) => ({
  id: item.id,
  slug: item.slug,
  title: item.title.includes("؛") ? item.title.split("؛")[0]! : item.title,
  summary: item.summary,
  time: item.time,
  views: item.views,
  image: item.image,
}));

export function getNewsArticleBySlug(slug: string): NewsArticle | null {
  const normalized = slug.trim().toLowerCase();
  return NEWS_ARTICLES.find((item) => item.slug.toLowerCase() === normalized) ?? null;
}

/** Full markdown body for catalog news detail fallback (no CMS placeholder copy). */
export function getNewsDetailBody(item: NewsArticle): string {
  if (item.slug === "claude-terminal-agent") {
    return `## Claude Terminal Agent چیست؟

${item.summary} Claude Terminal Agent یک دستیار کدنویسی مبتنی بر مدل‌های Anthropic است که مستقیماً داخل ترمینال اجرا می‌شود و می‌تواند فایل‌ها را بخواند، دستور اجرا کند و تغییرات را پیشنهاد دهد.

## چرا برای توسعه‌دهندگان مهم است؟

برخلاف چت‌بات‌های جدا از محیط توسعه، این Agent در همان مسیری کار می‌کند که روزانه کد می‌نویسید. همین نزدیکی به فایل‌ها، گیت و اسکریپت‌ها باعث می‌شود چرخهٔ «سوال → اجرا → بازبینی» کوتاه‌تر شود.

> نکته مهم: قبل از اجرای دستورهای پیشنهادی Agent روی پروژهٔ اصلی، حتماً در یک شاخه یا محیط آزمایشی تست کنید.

## شروع سریع

\`\`\`bash
# نمونه تعامل در ترمینال
claude "خلاصهٔ ساختار پروژه را بگو و تست‌های واحد را پیشنهاد بده"
\`\`\`

## نکات امنیتی و عملی

- دسترسی Agent به فایل‌های حساس (.env و کلیدها) را محدود کنید.
- خروجی را مثل کد یک همکار بررسی کنید؛ به‌صورت خودکار merge نکنید.
- برای کارهای تکراری، پرامپت‌های کوتاه و مشخص بنویسید.

## جمع‌بندی

Claude Terminal Agent گزینهٔ جدی برای تیم‌هایی است که می‌خواهند دستیار AI را داخل جریان ترمینال نگه دارند. اگر همین حالا از Cursor یا Copilot استفاده می‌کنید، مقایسهٔ سرعت و کیفیت خروجی در یک تسک واقعی بهترین معیار تصمیم است.
`;
  }

  return `## ${item.title}

${item.summary}

## جزئیات خبر

این خبر در دستهٔ **${item.categoryLabel ?? item.tag}** منتشر شده و برای توسعه‌دهندگان HelpDev گردآوری شده است. در ادامه مهم‌ترین نکات و تأثیر آن روی جریان کار روزمره را مرور می‌کنیم.

> نکته مهم: برای به‌روز ماندن، خبرنامه هفتگی HelpDev را فعال کنید تا خلاصهٔ همین مطالب را در ایمیل بگیرید.

## آنچه باید بدانید

- موضوع اصلی: ${item.tag}
- زمان تقریبی مطالعه: ${item.readTime}
- منبع پوشش: تیم تحریریه HelpDev

## گام بعدی

اگر این خبر به مسیر یادگیری شما مربوط است، بخش مطالب مرتبط و نقشه راه سایت را برای ادامه مطالعه پیشنهاد می‌کنیم.
`;
}

export function parseNewsViews(viewsLabel: string): number {
  const ascii = viewsLabel.replace(/[۰-۹]/g, (digit) => String("۰۱۲۳۴۵۶۷۸۹".indexOf(digit)));
  const normalized = ascii.replace(/[^\d.]/g, "");
  const value = Number.parseFloat(normalized);
  if (!Number.isFinite(value)) return 0;
  if (/k/i.test(ascii)) return Math.round(value * 1000);
  return Math.round(value);
}

export function buildPopularFromArticles(articles: NewsArticle[]): PopularNewsItem[] {
  return articles.slice(0, 5).map((item) => ({
    id: item.id,
    slug: item.slug,
    title: item.title.length > 42 ? `${item.title.slice(0, 40)}…` : item.title,
    summary: item.summary,
    time: item.time,
    views: item.views,
    image: item.image,
  }));
}

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
      return article.tag === "React" || (article.categoryLabel ?? "").includes("Frontend");
    case "Backend":
      return hay.includes("backend") || hay.includes("api");
    case "DevOps":
      return article.tag === "DevOps";
    case "Tools":
      return (article.categoryLabel ?? "") === "Tools" || hay.includes("copilot") || hay.includes("mcp");
    case "Security":
      return hay.includes("security") || hay.includes("امنیت");
    default:
      return true;
  }
}

function matchesCloudTag(article: NewsArticle, tag: NewsCloudTag): boolean {
  if (tag === "همه") return true;
  const hay = `${article.title} ${article.summary} ${article.tag} ${article.slug}`.toLowerCase();
  return hay.includes(tag.toLowerCase());
}

export function filterNewsArticles(
  articles: NewsArticle[],
  category: NewsCategoryId,
  cloudTag: NewsCloudTag,
): NewsArticle[] {
  return articles.filter(
    (article) => matchesCategory(article, category) && matchesCloudTag(article, cloudTag),
  );
}
