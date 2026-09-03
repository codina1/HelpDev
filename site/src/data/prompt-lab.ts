import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";

export const PROMPT_LAB_HERO_IMAGE_SRC = "/prompt-lab/hero-flask.png";

export const PROMPT_LAB_HERO_EYEBROW = "آزمایشگاه پرامپت";
export const PROMPT_LAB_HERO_TITLE = "Prompt Lab";
export const PROMPT_LAB_HERO_SUBTITLE =
  "مجموعه‌ای از بهترین پرامپت‌ها برای توسعه‌دهندگان، ابزارهای AI و ساخت محصول حرفه‌ای.";

export type PromptLabQuickFilterId =
  | "all"
  | "chatgpt"
  | "claude"
  | "gemini"
  | "copilot"
  | "code"
  | "design"
  | "devops"
  | "content"
  | "data"
  | "other";

export type PromptLabQuickFilter = {
  id: PromptLabQuickFilterId;
  label: string;
  icon: string;
  category?: string;
  aiModel?: string;
};

export const PROMPT_LAB_QUICK_FILTERS: readonly PromptLabQuickFilter[] = [
  { id: "all", label: "همه", icon: "all" },
  { id: "chatgpt", label: "ChatGPT", icon: "chatgpt", aiModel: "ChatGPT" },
  { id: "claude", label: "Claude", icon: "claude", aiModel: "Claude" },
  { id: "gemini", label: "Gemini", icon: "gemini", aiModel: "Gemini" },
  { id: "copilot", label: "Copilot", icon: "copilot", aiModel: "Copilot" },
  { id: "code", label: "Code", icon: "code", category: "Code" },
  { id: "design", label: "طراحی", icon: "design", category: "Design" },
  { id: "devops", label: "DevOps", icon: "devops", category: "DevOps" },
  { id: "content", label: "تولید محتوا", icon: "content", category: "Content" },
  { id: "data", label: "تولید داده", icon: "data", category: "Data" },
  { id: "other", label: "دیگر", icon: "other", category: "Other" },
] as const;

export type PromptLabSidebarCategory = {
  id: string;
  label: string;
  slug: string;
  count: number;
};

export const PROMPT_LAB_SIDEBAR_CATEGORIES: readonly PromptLabSidebarCategory[] = [
  { id: "code", label: "Code", slug: "Code", count: 68 },
  { id: "design", label: "طراحی", slug: "Design", count: 42 },
  { id: "devops", label: "DevOps", slug: "DevOps", count: 31 },
  { id: "content", label: "تولید محتوا", slug: "Content", count: 28 },
  { id: "data", label: "تولید داده", slug: "Data", count: 19 },
  { id: "other", label: "دیگر", slug: "Other", count: 12 },
] as const;

export type PromptLabSidebarModel = {
  id: string;
  label: string;
  slug: string;
};

export const PROMPT_LAB_SIDEBAR_MODELS: readonly PromptLabSidebarModel[] = [
  { id: "chatgpt", label: "ChatGPT", slug: "ChatGPT" },
  { id: "claude", label: "Claude", slug: "Claude" },
  { id: "gemini", label: "Gemini", slug: "Gemini" },
  { id: "copilot", label: "Copilot", slug: "Copilot" },
  { id: "midjourney", label: "Midjourney", slug: "Midjourney" },
] as const;

export type PromptLabLevelId = "all" | "beginner" | "intermediate" | "advanced";

export const PROMPT_LAB_LEVELS: readonly { id: PromptLabLevelId; label: string }[] = [
  { id: "all", label: "همه سطوح" },
  { id: "beginner", label: "مبتدی" },
  { id: "intermediate", label: "متوسط" },
  { id: "advanced", label: "پیشرفته" },
] as const;

export type PromptLabSortId = "newest" | "popular" | "views";

export const PROMPT_LAB_SORT_OPTIONS: readonly { id: PromptLabSortId; label: string }[] = [
  { id: "newest", label: "جدیدترین" },
  { id: "popular", label: "محبوب‌ترین" },
  { id: "views", label: "پربازدیدترین" },
] as const;

export const PROMPT_LAB_PAGE_SIZE = 12;
export const PROMPT_LAB_DISPLAY_TOTAL = 234;

/** Reference marketplace sample prompts — always visible on the public page. */
export const PROMPT_LAB_SAMPLE_PROMPTS: readonly PromptLabCardItem[] = [
  {
    id: "pl-sample-1",
    slug: "csv-data-analysis",
    title: "تحلیل داده‌های CSV",
    description: "پرامپت تحلیل، پاکسازی و استخراج بینش از فایل‌های CSV برای تیم داده.",
    coverImage: "/home/icon-database.png",
    aiModel: "Claude",
    category: "Data",
    categorySlug: "data",
    copyCount: 214,
    viewCount: 4820,
    featured: true,
    publishedAt: "2026-08-28T10:00:00.000Z",
  },
  {
    id: "pl-sample-2",
    slug: "optimized-dockerfile",
    title: "نوشتن Dockerfile بهینه",
    description: "ساخت Dockerfile چندمرحله‌ای سبک با کش لایه‌ها و امنیت پایه.",
    coverImage: "/home/icon-devops.png",
    aiModel: "ChatGPT",
    category: "DevOps",
    categorySlug: "devops",
    copyCount: 361,
    viewCount: 7100,
    featured: true,
    publishedAt: "2026-08-27T09:00:00.000Z",
  },
  {
    id: "pl-sample-3",
    slug: "modern-ui-design",
    title: "طراحی رابط کاربری مدرن",
    description: "تولید سیستم UI تیره با گرادیان بنفش/آبی و کامپوننت‌های شیشه‌ای.",
    coverImage: "/home/icon-frontend.png",
    aiModel: "Claude",
    category: "Design",
    categorySlug: "design",
    copyCount: 198,
    viewCount: 3560,
    featured: true,
    publishedAt: "2026-08-26T12:00:00.000Z",
  },
  {
    id: "pl-sample-4",
    slug: "nodejs-api-generator",
    title: "تولید API با Node.js",
    description: "طراحی REST API تمیز با Express، اعتبارسنجی و ساختار ماژولار.",
    coverImage: "/home/icon-backend.png",
    aiModel: "ChatGPT",
    category: "Code",
    categorySlug: "code",
    copyCount: 452,
    viewCount: 8920,
    featured: true,
    publishedAt: "2026-08-25T08:30:00.000Z",
  },
  {
    id: "pl-sample-5",
    slug: "advanced-react-component",
    title: "کامپوننت React پیشرفته",
    description: "ساخت کامپوننت reusable با hooks، state و الگوی composition.",
    coverImage: "/home/icon-code.png",
    aiModel: "Copilot",
    category: "Code",
    categorySlug: "code",
    copyCount: 287,
    viewCount: 5340,
    featured: false,
    publishedAt: "2026-08-24T15:00:00.000Z",
  },
  {
    id: "pl-sample-6",
    slug: "app-security-hardening",
    title: "افزایش امنیت برنامه",
    description: "چک‌لیست امنیتی برای احراز هویت، ورودی‌ها و محافظت از API.",
    coverImage: "/home/icon-security.png",
    aiModel: "ChatGPT",
    category: "Code",
    categorySlug: "code",
    copyCount: 165,
    viewCount: 2980,
    featured: false,
    publishedAt: "2026-08-23T11:20:00.000Z",
  },
  {
    id: "pl-sample-7",
    slug: "ai-prompt-architect",
    title: "معماری پرامپت سیستمی",
    description: "طراحی system prompt چندلایه برای دستیار توسعه و بازبینی کد.",
    coverImage: "/home/icon-ai.png",
    aiModel: "Gemini",
    category: "Other",
    categorySlug: "other",
    copyCount: 143,
    viewCount: 2410,
    featured: false,
    publishedAt: "2026-08-22T14:00:00.000Z",
  },
  {
    id: "pl-sample-8",
    slug: "content-release-brief",
    title: "بریف انتشار محصول",
    description: "تولید متن معرفی انتشار برای وبلاگ، خبرنامه و شبکه‌های اجتماعی.",
    coverImage: "/home/icon-prompt-lab.png",
    aiModel: "ChatGPT",
    category: "Content",
    categorySlug: "content",
    copyCount: 119,
    viewCount: 1870,
    featured: false,
    publishedAt: "2026-08-21T16:40:00.000Z",
  },
  {
    id: "pl-sample-9",
    slug: "sql-query-optimizer",
    title: "بهینه‌سازی کوئری SQL",
    description: "بازنویسی کوئری‌های کند با ایندکس، join و پلن اجرای شفاف.",
    coverImage: "/home/icon-db.png",
    aiModel: "Claude",
    category: "Data",
    categorySlug: "data",
    copyCount: 176,
    viewCount: 3120,
    featured: false,
    publishedAt: "2026-08-20T10:00:00.000Z",
  },
  {
    id: "pl-sample-10",
    slug: "ci-cd-pipeline",
    title: "پایپ‌لاین CI/CD",
    description: "طراحی workflow گیت‌هاب برای تست، بیلد و دیپلوی امن.",
    coverImage: "/home/icon-devops.png",
    aiModel: "Copilot",
    category: "DevOps",
    categorySlug: "devops",
    copyCount: 203,
    viewCount: 4010,
    featured: false,
    publishedAt: "2026-08-19T09:30:00.000Z",
  },
  {
    id: "pl-sample-11",
    slug: "mobile-ui-kit",
    title: "کیت رابط موبایل",
    description: "تولید کامپوننت‌های موبایل با حالت تاریک و فاصله‌گذاری استاندارد.",
    coverImage: "/home/icon-mobile.png",
    aiModel: "Gemini",
    category: "Design",
    categorySlug: "design",
    copyCount: 132,
    viewCount: 2210,
    featured: false,
    publishedAt: "2026-08-18T13:00:00.000Z",
  },
  {
    id: "pl-sample-12",
    slug: "dotnet-clean-architecture",
    title: "معماری تمیز در .NET",
    description: "ساختار لایه دامنه، اپلیکیشن و زیرساخت برای سرویس‌های سازمانی.",
    coverImage: "/home/icon-dotnet.png",
    aiModel: "ChatGPT",
    category: "Code",
    categorySlug: "code",
    copyCount: 248,
    viewCount: 4650,
    featured: false,
    publishedAt: "2026-08-17T11:15:00.000Z",
  },
  {
    id: "pl-sample-13",
    slug: "linux-server-hardening",
    title: "سخت‌سازی سرور لینوکس",
    description: "چک‌لیست فایروال، SSH، به‌روزرسانی و مانیتورینگ برای سرور production.",
    coverImage: "/home/icon-linux.png",
    aiModel: "Claude",
    category: "DevOps",
    categorySlug: "devops",
    copyCount: 156,
    viewCount: 2780,
    featured: false,
    publishedAt: "2026-08-16T10:00:00.000Z",
  },
  {
    id: "pl-sample-14",
    slug: "jwt-auth-flow",
    title: "جریان احراز هویت JWT",
    description: "طراحی login، refresh token و محافظت از routeهای حساس.",
    coverImage: "/home/icon-jwt.png",
    aiModel: "ChatGPT",
    category: "Code",
    categorySlug: "code",
    copyCount: 221,
    viewCount: 3910,
    featured: false,
    publishedAt: "2026-08-15T12:00:00.000Z",
  },
  {
    id: "pl-sample-15",
    slug: "brand-visual-system",
    title: "سیستم بصری برند",
    description: "تولید پالت رنگ، تایپوگرافی و قوانین فاصله‌گذاری برای محصول دیجیتال.",
    coverImage: "/home/icon-brand.png",
    aiModel: "Gemini",
    category: "Design",
    categorySlug: "design",
    copyCount: 98,
    viewCount: 1640,
    featured: false,
    publishedAt: "2026-08-14T09:00:00.000Z",
  },
  {
    id: "pl-sample-16",
    slug: "newsletter-ai-digest",
    title: "خبرنامه هوش مصنوعی",
    description: "خلاصه هفتگی اخبار AI با لحن حرفه‌ای و ساختار اسکین‌پذیر.",
    coverImage: "/home/icon-newsletter.png",
    aiModel: "ChatGPT",
    category: "Content",
    categorySlug: "content",
    copyCount: 87,
    viewCount: 1420,
    featured: false,
    publishedAt: "2026-08-13T15:00:00.000Z",
  },
];
