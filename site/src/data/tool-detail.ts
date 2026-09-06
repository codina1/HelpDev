import { MARKETPLACE_TOOLS, type MarketplaceTool } from "@/data/tools";

export type ToolDetailTabId =
  | "intro"
  | "features"
  | "learning"
  | "reviews"
  | "versions"
  | "similar";

export type ToolFeatureItem = {
  id: string;
  title: string;
  description: string;
  icon: string;
};

export type ToolScreenshotItem = {
  id: string;
  title: string;
  image: string;
};

export type ToolVersionItem = {
  id: string;
  version: string;
  dateLabel: string;
  summary: string;
  isLatest?: boolean;
};

export type ToolRelatedArticle = {
  id: string;
  title: string;
  href: string;
  meta: string;
  kind: "article" | "video";
};

export type ToolRelatedCourse = {
  id: string;
  title: string;
  href: string;
  level: string;
  duration: string;
  image: string;
};

export type ToolRelatedRoadmap = {
  id: string;
  title: string;
  href: string;
  description: string;
  stagesLabel: string;
};

export type ToolRelatedTool = {
  id: string;
  slug: string;
  name: string;
  categoryLabel: string;
  description: string;
  logo: string;
};

export type ToolRatingBreakdown = {
  stars: 1 | 2 | 3 | 4 | 5;
  percent: number;
};

export type ToolDetailModel = {
  id: string;
  slug: string;
  name: string;
  verified?: boolean;
  category: string;
  categoryLabel: string;
  description: string;
  longDescription: string;
  logo: string;
  websiteUrl: string;
  downloadUrl: string;
  developer: string;
  license: string;
  toolType: string;
  version: string;
  releaseDateLabel: string;
  platforms: string[];
  badges: string[];
  rating: number;
  ratingCount: number;
  viewsLabel: string;
  downloadsLabel: string;
  ratingBreakdown: ToolRatingBreakdown[];
  features: ToolFeatureItem[];
  screenshots: ToolScreenshotItem[];
  versions: ToolVersionItem[];
  learningResources: ToolRelatedArticle[];
  relatedArticles: ToolRelatedArticle[];
  relatedCourses: ToolRelatedCourse[];
  relatedRoadmaps: ToolRelatedRoadmap[];
  relatedTools: ToolRelatedTool[];
  breadcrumb: { label: string; href?: string }[];
};

export const TOOL_DETAIL_TABS: { id: ToolDetailTabId; label: string }[] = [
  { id: "intro", label: "معرفی" },
  { id: "features", label: "ویژگی‌ها" },
  { id: "learning", label: "آموزش و منابع" },
  { id: "reviews", label: "نظرات کاربران" },
  { id: "versions", label: "نسخه‌ها" },
  { id: "similar", label: "ابزارهای مشابه" },
];

const SLUG_ALIASES: Record<string, string> = {
  "visual-studio-code": "vscode",
  "vs-code": "vscode",
  "claude-code": "cursor",
};

function formatCompact(value: number): string {
  if (value >= 1_000_000) {
    return `${(value / 1_000_000).toFixed(1).replace(/\.0$/, "")}M`;
  }
  if (value >= 1000) {
    const n = value / 1000;
    const rounded = n >= 10 ? Math.round(n) : Math.round(n * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return value.toLocaleString("fa-IR");
}

const DEFAULT_FEATURES: ToolFeatureItem[] = [
  { id: "f1", title: "تجربه سریع", description: "رابط سبک و عملکرد مناسب کارهای روزمره.", icon: "⚡" },
  { id: "f2", title: "اکوسیستم", description: "افزونه و یکپارچگی با ابزارهای رایج توسعه.", icon: "◇" },
  { id: "f3", title: "همکاری", description: "مناسب کار تیمی و گردش‌کار مدرن.", icon: "◎" },
  { id: "f4", title: "مستندات", description: "منابع یادگیری و جامعه فعال.", icon: "▤" },
];

const VSCODE_FEATURES: ToolFeatureItem[] = [
  { id: "vf1", title: "IntelliSense", description: "تکمیل هوشمند کد و پیشنهادهای زمینه‌ای.", icon: "✦" },
  { id: "vf2", title: "Debugging", description: "دیباگ حرفه‌ای برای زبان‌های مختلف.", icon: "◈" },
  { id: "vf3", title: "Extensions", description: "هزاران افزونه برای گسترش قابلیت‌ها.", icon: "▣" },
  { id: "vf4", title: "Git Integration", description: "مدیریت Git داخل ویرایشگر.", icon: "⎇" },
  { id: "vf5", title: "Cross Platform", description: "ویندوز، مک و لینوکس.", icon: "⬡" },
  { id: "vf6", title: "Performance", description: "اجرای سریع حتی روی پروژه‌های بزرگ.", icon: "⚡" },
  { id: "vf7", title: "Terminal", description: "ترمینال یکپارچه برای دستورات روزمره.", icon: "⌘" },
  { id: "vf8", title: "Remote Dev", description: "توسعه روی سرور و کانتینر از راه دور.", icon: "☁" },
];

function buildScreenshots(name: string): ToolScreenshotItem[] {
  return [
    { id: "s1", title: "محیط اصلی", image: "/tools/hero-toolbox.png" },
    { id: "s2", title: "افزونه‌ها", image: "/tools/hero-toolbox.png" },
    { id: "s3", title: "دیباگ", image: "/tools/hero-toolbox.png" },
    { id: "s4", title: "Git", image: "/tools/hero-toolbox.png" },
  ].map((item) => ({ ...item, title: `${item.title} · ${name}` }));
}

function relatedFromMarketplace(currentSlug: string, category: string): ToolRelatedTool[] {
  return MARKETPLACE_TOOLS.filter((tool) => tool.slug !== currentSlug)
    .sort((a, b) => {
      const score = (t: MarketplaceTool) => (t.category === category ? 2 : 0) + t.rating;
      return score(b) - score(a);
    })
    .slice(0, 4)
    .map((tool) => ({
      id: tool.id,
      slug: tool.slug,
      name: tool.name,
      categoryLabel: tool.categoryLabel,
      description: tool.description,
      logo: tool.logo,
    }));
}

function enrichFromMarketplace(tool: MarketplaceTool): ToolDetailModel {
  const isVscode = tool.slug === "vscode";
  // Display name for design/SEO; marketplace catalog still uses short "VS Code".
  const downloads = Math.round(tool.reviewCount * 4.2);
  const views = Math.round(tool.reviewCount * 95);

  return {
    id: tool.id,
    slug: tool.slug,
    name: isVscode ? "Visual Studio Code" : tool.name,
    verified: isVscode || tool.rating >= 4.8,
    category: tool.category,
    categoryLabel: isVscode ? "ویرایشگر کد" : tool.categoryLabel,
    description: tool.description,
    longDescription: isVscode
      ? "Visual Studio Code یک ویرایشگر کد متن‌باز و چندسکویی از مایکروسافت است که با IntelliSense، دیباگ یکپارچه، Git داخلی و اکوسیستم عظیم افزونه‌ها، به انتخاب اول بسیاری از توسعه‌دهندگان تبدیل شده است. این ابزار برای فرانت‌اند، بک‌اند، DevOps و کار با AI Code Assistants بسیار مناسب است و گردش‌کار روزمره تیم‌های مدرن را پوشش می‌دهد."
      : `${tool.description} این ابزار در کاتالوگ HelpDev برای توسعه‌دهندگان و تیم‌های محصول انتخاب شده و با مسیرهای یادگیری، مقالات و دوره‌های مرتبط همراه است تا سریع‌تر به جریان واقعی کار برسید.`,
    logo: tool.logo,
    websiteUrl: tool.href,
    downloadUrl: tool.href,
    developer: isVscode ? "Microsoft" : tool.name,
    license: tool.price === "free" ? "رایگان / Open Source" : tool.price === "freemium" ? "فریمیوم" : "پولی",
    toolType: isVscode ? "IDE / Code Editor" : tool.categoryLabel,
    version: isVscode ? "1.86.2" : "Latest",
    releaseDateLabel: "۱۴۰۴/۱۰/۱۵",
    platforms: isVscode
      ? ["Windows", "macOS", "Linux"]
      : tool.category === "web" || tool.category === "frontend" || tool.category === "ai"
        ? ["Web", "Desktop"]
        : ["Cross-platform"],
    badges: isVscode
      ? ["Windows", "macOS", "Linux", "Open Source", "Microsoft"]
      : [tool.categoryLabel, tool.price === "free" ? "رایگان" : tool.price === "freemium" ? "فریمیوم" : "پولی"],
    rating: tool.rating,
    ratingCount: tool.reviewCount,
    viewsLabel: formatCompact(views),
    downloadsLabel: formatCompact(downloads),
    ratingBreakdown: [
      { stars: 5, percent: 72 },
      { stars: 4, percent: 18 },
      { stars: 3, percent: 7 },
      { stars: 2, percent: 2 },
      { stars: 1, percent: 1 },
    ],
    features: isVscode ? VSCODE_FEATURES : DEFAULT_FEATURES,
    screenshots: buildScreenshots(tool.name),
    versions: [
      {
        id: "v1",
        version: isVscode ? "v1.86.2" : "v1.0",
        dateLabel: "۱۴۰۴/۱۰/۱۵",
        summary: "بهبود پایداری و تجربه توسعه‌دهنده",
        isLatest: true,
      },
      {
        id: "v0",
        version: isVscode ? "v1.85.0" : "v0.9",
        dateLabel: "۱۴۰۴/۰۸/۲۰",
        summary: "افزودن قابلیت‌های کلیدی و رفع باگ‌ها",
      },
    ],
    learningResources: [
      {
        id: "lr1",
        title: `راهنمای شروع با ${tool.name}`,
        href: "/articles",
        meta: "مقاله آموزشی · ۸ دقیقه",
        kind: "article",
      },
      {
        id: "lr2",
        title: `نکات حرفه‌ای ${tool.name}`,
        href: "/articles",
        meta: "ویدئو · ۲۵ دقیقه",
        kind: "video",
      },
      {
        id: "lr3",
        title: "بهترین تنظیمات برای توسعه مدرن",
        href: "/articles",
        meta: "مقاله · ۱۲ دقیقه",
        kind: "article",
      },
      {
        id: "lr4",
        title: "افزونه‌ها و میانبرهای ضروری",
        href: "/articles",
        meta: "مقاله · ۱۰ دقیقه",
        kind: "article",
      },
    ],
    relatedArticles: [
      {
        id: "a1",
        title: `آموزش ${tool.name} برای توسعه‌دهندگان`,
        href: "/articles",
        meta: "۸ دقیقه · ۱.۲K بازدید",
        kind: "article",
      },
      {
        id: "a2",
        title: "ابزارهای ضروری محیط توسعه",
        href: "/articles",
        meta: "۶ دقیقه · ۹۸۰ بازدید",
        kind: "article",
      },
    ],
    relatedCourses: [
      {
        id: "c1",
        title: isVscode ? "دوره جامع VS Code" : `دوره کار با ${tool.name}`,
        href: "/courses/react-19",
        level: "متوسط",
        duration: "۸ ساعت",
        image: "/courses/course-react.png",
      },
    ],
    relatedRoadmaps: [
      {
        id: "rm1",
        title: "Frontend Developer Roadmap",
        href: "/roadmap/react-developer",
        description: "مسیر یادگیری ساخت اپلیکیشن‌های مدرن وب",
        stagesLabel: "۱۱ مرحله",
      },
    ],
    relatedTools: relatedFromMarketplace(tool.slug, tool.category),
    breadcrumb: [
      { label: "خانه", href: "/" },
      { label: "ابزارها", href: "/toolbox" },
      { label: isVscode ? "ویرایشگر کد" : tool.categoryLabel, href: "/toolbox" },
      { label: tool.name },
    ],
  };
}

export function resolveToolSlug(slug: string): string {
  const key = decodeURIComponent(slug).trim().toLowerCase();
  return SLUG_ALIASES[key] ?? key;
}

export function getToolDetailBySlug(slug: string): ToolDetailModel | null {
  const key = resolveToolSlug(slug);
  if (!key) return null;
  const tool = MARKETPLACE_TOOLS.find((item) => item.slug.toLowerCase() === key);
  if (!tool) return null;
  return enrichFromMarketplace(tool);
}

export function publicToolPath(slug: string): string {
  return `/tools/${encodeURIComponent(slug)}`;
}

/** Merge optional API summary onto catalog detail when available. */
export function mergeToolApiDetail(
  base: ToolDetailModel,
  api?: { title?: string; description?: string | null; status?: string } | null,
): ToolDetailModel {
  if (!api) return base;
  return {
    ...base,
    name: api.title?.trim() || base.name,
    description: api.description?.trim() || base.description,
  };
}
