export const ARTICLES_HERO_IMAGE_SRC = "/articles/hero-book.png";

export type ArticleCategoryId =
  | "all"
  | "ai"
  | "programming"
  | "dotnet"
  | "frontend"
  | "backend"
  | "devops"
  | "tools"
  | "architecture"
  | "security";

export type ArticleLevelId = "all" | "beginner" | "intermediate" | "advanced";
export type ArticleSortId = "newest" | "popular" | "views";

export type ArticleCategoryChip = {
  id: ArticleCategoryId;
  label: string;
  icon: string;
  count?: number;
};

export const ARTICLE_CATEGORY_CHIPS: readonly ArticleCategoryChip[] = [
  { id: "all", label: "همه", icon: "all", count: 160 },
  { id: "ai", label: "AI", icon: "ai" },
  { id: "programming", label: "Programming", icon: "programming" },
  { id: "dotnet", label: ".NET", icon: "dotnet" },
  { id: "frontend", label: "Frontend", icon: "frontend" },
  { id: "backend", label: "Backend", icon: "backend" },
  { id: "devops", label: "DevOps", icon: "devops" },
  { id: "tools", label: "Tools", icon: "tools" },
  { id: "architecture", label: "Architecture", icon: "architecture" },
  { id: "security", label: "Security", icon: "security" },
] as const;

export type ArticleSidebarTopic = {
  id: Exclude<ArticleCategoryId, "all">;
  label: string;
  count: number;
};

export const ARTICLE_SIDEBAR_TOPICS: readonly ArticleSidebarTopic[] = [
  { id: "ai", label: "هوش مصنوعی", count: 125 },
  { id: "programming", label: "برنامه‌نویسی", count: 98 },
  { id: "dotnet", label: ".NET", count: 54 },
  { id: "frontend", label: "Frontend", count: 72 },
  { id: "backend", label: "Backend", count: 61 },
  { id: "devops", label: "DevOps", count: 43 },
  { id: "tools", label: "Tools", count: 37 },
  { id: "architecture", label: "Architecture", count: 29 },
  { id: "security", label: "Security", count: 22 },
] as const;

export const ARTICLE_LEVELS: readonly { id: ArticleLevelId; label: string }[] = [
  { id: "all", label: "همه سطوح" },
  { id: "beginner", label: "مقدماتی" },
  { id: "intermediate", label: "متوسط" },
  { id: "advanced", label: "پیشرفته" },
] as const;

export const ARTICLE_SORT_OPTIONS: readonly { id: ArticleSortId; label: string }[] = [
  { id: "newest", label: "جدیدترین" },
  { id: "popular", label: "محبوب‌ترین" },
  { id: "views", label: "بیشترین بازدید" },
] as const;

export type MarketplaceArticle = {
  id: string;
  slug: string;
  title: string;
  description: string;
  category: Exclude<ArticleCategoryId, "all">;
  categoryLabel: string;
  level: Exclude<ArticleLevelId, "all">;
  coverImage: string;
  coverTone: string;
  author: string;
  authorInitials: string;
  readingMinutes: number;
  views: number;
  publishedAt: string;
  featured?: boolean;
};

export const ARTICLES_PAGE_SIZE = 8;
export const ARTICLES_DISPLAY_TOTAL = 160;

/** Premium sample articles for the listing marketplace UI. */
export const MARKETPLACE_ARTICLES: readonly MarketplaceArticle[] = [
  {
    id: "a1",
    slug: "cursor-1-ai-ide",
    title: "Cursor 1.0 چیست؟ نسل جدید AI IDE",
    description: "بررسی کامل قابلیت‌های جدید Cursor و تاثیر آن روی آینده برنامه‌نویسی با هوش مصنوعی.",
    category: "ai",
    categoryLabel: "AI",
    level: "intermediate",
    coverImage: "/news/cover-cursor.png",
    coverTone: "from-[#3B82F6]/35 to-[#7C3AED]/10",
    author: "تیم HelpDev",
    authorInitials: "HD",
    readingMinutes: 12,
    views: 15400,
    publishedAt: "2026-09-01T10:00:00.000Z",
    featured: true,
  },
  {
    id: "a2",
    slug: "claude-code",
    title: "Claude Code چیست؟",
    description: "آشنایی با Claude Code و نقش آن در کدنویسی، بازبینی و اتوماسیون توسعه.",
    category: "ai",
    categoryLabel: "AI",
    level: "beginner",
    coverImage: "/news/cover-claude.png",
    coverTone: "from-[#F59E0B]/30 to-[#7C3AED]/10",
    author: "سارا نوری",
    authorInitials: "سن",
    readingMinutes: 9,
    views: 8700,
    publishedAt: "2026-08-30T12:00:00.000Z",
  },
  {
    id: "a3",
    slug: "what-is-mcp",
    title: "MCP چیست؟",
    description: "Model Context Protocol را ساده توضیح می‌دهیم و کاربردش در ابزارهای AI را می‌بینیم.",
    category: "ai",
    categoryLabel: "AI",
    level: "intermediate",
    coverImage: "/news/cover-mcp.png",
    coverTone: "from-[#22D3EE]/30 to-[#6366F1]/10",
    author: "علی رضایی",
    authorInitials: "عر",
    readingMinutes: 11,
    views: 6400,
    publishedAt: "2026-08-28T09:30:00.000Z",
  },
  {
    id: "a4",
    slug: "react-19-changes",
    title: "React 19 چه تغییراتی دارد؟",
    description: "مرور Actions، Server Components و بهبودهای مهم React 19 برای فرانت‌اند.",
    category: "frontend",
    categoryLabel: "Frontend",
    level: "intermediate",
    coverImage: "/news/cover-react.png",
    coverTone: "from-[#38BDF8]/30 to-[#0EA5E9]/10",
    author: "مینا کاظمی",
    authorInitials: "مک",
    readingMinutes: 10,
    views: 11200,
    publishedAt: "2026-08-26T14:00:00.000Z",
  },
  {
    id: "a5",
    slug: "dotnet-10-preview",
    title: ".NET 10 Preview",
    description: "نگاهی به قابلیت‌های جدید .NET 10 Preview و مسیر مهاجرت تیم‌های سازمانی.",
    category: "dotnet",
    categoryLabel: ".NET",
    level: "advanced",
    coverImage: "/news/cover-dotnet.png",
    coverTone: "from-[#818CF8]/30 to-[#6366F1]/10",
    author: "رضا محمدی",
    authorInitials: "رم",
    readingMinutes: 14,
    views: 5300,
    publishedAt: "2026-08-24T11:20:00.000Z",
  },
  {
    id: "a6",
    slug: "docker-compose-guide",
    title: "Docker Compose چیست؟",
    description: "ساخت محیط چندسرویسی با Compose برای توسعه لوکال و استقرار پایدار.",
    category: "devops",
    categoryLabel: "DevOps",
    level: "beginner",
    coverImage: "/news/cover-devops.png",
    coverTone: "from-[#38BDF8]/30 to-[#0EA5E9]/10",
    author: "نگار احمدی",
    authorInitials: "نا",
    readingMinutes: 8,
    views: 9800,
    publishedAt: "2026-08-22T16:00:00.000Z",
  },
  {
    id: "a7",
    slug: "hexagonal-architecture",
    title: "معماری Hexagonal چیست؟",
    description: "Ports & Adapters را با مثال عملی برای سرویس‌های بک‌اند توضیح می‌دهیم.",
    category: "architecture",
    categoryLabel: "Architecture",
    level: "advanced",
    coverImage: "/home/icon-architect.png",
    coverTone: "from-[#A78BFA]/30 to-[#7C3AED]/10",
    author: "کیان پارسا",
    authorInitials: "کپ",
    readingMinutes: 15,
    views: 4100,
    publishedAt: "2026-08-20T10:00:00.000Z",
  },
  {
    id: "a8",
    slug: "nodejs-api-design",
    title: "طراحی Node.js API",
    description: "الگوهای ساخت API تمیز با Express، اعتبارسنجی و ساختار لایه‌ای.",
    category: "backend",
    categoryLabel: "Backend",
    level: "intermediate",
    coverImage: "/home/icon-backend.png",
    coverTone: "from-[#34D399]/30 to-[#059669]/10",
    author: "پارسا کریمی",
    authorInitials: "پک",
    readingMinutes: 13,
    views: 7600,
    publishedAt: "2026-08-18T13:40:00.000Z",
  },
  {
    id: "a9",
    slug: "programming-fundamentals",
    title: "اصول برنامه‌نویسی تمیز",
    description: "نام‌گذاری، مسئولیت واحد و خوانایی کد برای تیم‌های محصول.",
    category: "programming",
    categoryLabel: "Programming",
    level: "beginner",
    coverImage: "/home/icon-code.png",
    coverTone: "from-[#C084FC]/25 to-[#6366F1]/10",
    author: "تیم HelpDev",
    authorInitials: "HD",
    readingMinutes: 7,
    views: 3900,
    publishedAt: "2026-08-16T09:00:00.000Z",
  },
  {
    id: "a10",
    slug: "security-checklist",
    title: "چک‌لیست امنیت اپلیکیشن",
    description: "اصول پایه برای محافظت از احراز هویت، ورودی‌ها و APIهای عمومی.",
    category: "security",
    categoryLabel: "Security",
    level: "intermediate",
    coverImage: "/home/icon-security.png",
    coverTone: "from-[#F87171]/25 to-[#7C3AED]/10",
    author: "سارا نوری",
    authorInitials: "سن",
    readingMinutes: 10,
    views: 5200,
    publishedAt: "2026-08-14T12:00:00.000Z",
  },
  {
    id: "a11",
    slug: "devtools-roundup",
    title: "بهترین ابزارهای توسعه‌دهنده ۲۰۲۶",
    description: "مرور ابزارهایی که سرعت تیم‌های مهندسی را واقعاً بالا می‌برند.",
    category: "tools",
    categoryLabel: "Tools",
    level: "beginner",
    coverImage: "/home/icon-tools.png",
    coverTone: "from-[#A5B4FC]/25 to-[#7C3AED]/10",
    author: "علی رضایی",
    authorInitials: "عر",
    readingMinutes: 9,
    views: 6800,
    publishedAt: "2026-08-12T15:00:00.000Z",
  },
  {
    id: "a12",
    slug: "frontend-performance",
    title: "بهینه‌سازی عملکرد Frontend",
    description: "تکنیک‌های عملی برای کاهش LCP، مدیریت bundle و تجربه سریع‌تر کاربر.",
    category: "frontend",
    categoryLabel: "Frontend",
    level: "advanced",
    coverImage: "/home/icon-frontend.png",
    coverTone: "from-[#22D3EE]/25 to-[#3B82F6]/10",
    author: "مینا کاظمی",
    authorInitials: "مک",
    readingMinutes: 12,
    views: 4500,
    publishedAt: "2026-08-10T11:00:00.000Z",
  },
];
