export const TOOLS_HERO_IMAGE_SRC = "/tools/hero-toolbox.png";

export type ToolCategoryId =
  | "all"
  | "ai"
  | "web"
  | "frontend"
  | "backend"
  | "devops"
  | "design"
  | "security"
  | "database"
  | "mobile";

export type ToolPriceId = "all" | "free" | "freemium" | "paid";
export type ToolSortId = "newest" | "popular" | "rating";

export type ToolCategoryChip = {
  id: ToolCategoryId;
  label: string;
  icon: string;
};

export const TOOL_CATEGORY_CHIPS: readonly ToolCategoryChip[] = [
  { id: "all", label: "همه", icon: "all" },
  { id: "ai", label: "AI", icon: "ai" },
  { id: "web", label: "توسعه وب", icon: "web" },
  { id: "frontend", label: "Frontend", icon: "frontend" },
  { id: "backend", label: "Backend", icon: "backend" },
  { id: "devops", label: "DevOps", icon: "devops" },
  { id: "design", label: "طراحی", icon: "design" },
  { id: "security", label: "امنیت", icon: "security" },
  { id: "database", label: "دیتابیس", icon: "database" },
  { id: "mobile", label: "موبایل", icon: "mobile" },
] as const;

export type ToolSidebarCategory = {
  id: Exclude<ToolCategoryId, "all">;
  label: string;
  count: number;
};

export const TOOL_SIDEBAR_CATEGORIES: readonly ToolSidebarCategory[] = [
  { id: "ai", label: "AI", count: 28 },
  { id: "web", label: "توسعه وب", count: 42 },
  { id: "frontend", label: "Frontend", count: 36 },
  { id: "backend", label: "Backend", count: 31 },
  { id: "devops", label: "DevOps", count: 24 },
  { id: "design", label: "طراحی", count: 19 },
  { id: "security", label: "امنیت", count: 14 },
  { id: "database", label: "دیتابیس", count: 17 },
  { id: "mobile", label: "موبایل", count: 12 },
] as const;

export const TOOL_PRICE_OPTIONS: readonly { id: ToolPriceId; label: string }[] = [
  { id: "all", label: "همه" },
  { id: "free", label: "رایگان" },
  { id: "freemium", label: "فریمیوم" },
  { id: "paid", label: "پولی" },
] as const;

export const TOOL_SORT_OPTIONS: readonly { id: ToolSortId; label: string }[] = [
  { id: "newest", label: "جدیدترین" },
  { id: "popular", label: "محبوب‌ترین" },
  { id: "rating", label: "بیشترین امتیاز" },
] as const;

export type MarketplaceTool = {
  id: string;
  name: string;
  slug: string;
  description: string;
  category: Exclude<ToolCategoryId, "all">;
  categoryLabel: string;
  price: Exclude<ToolPriceId, "all">;
  rating: number;
  reviewCount: number;
  logo: string;
  href: string;
  publishedAt: string;
};

export const TOOLS_PAGE_SIZE = 12;
export const TOOLS_DISPLAY_TOTAL = 128;

/** Premium marketplace sample tools — matches the reference grid. */
export const MARKETPLACE_TOOLS: readonly MarketplaceTool[] = [
  {
    id: "chatgpt",
    name: "ChatGPT",
    slug: "chatgpt",
    description: "دستیار هوشمند AI برای تولید محتوا، کدنویسی و حل مسئله",
    category: "ai",
    categoryLabel: "AI",
    price: "freemium",
    rating: 4.9,
    reviewCount: 12840,
    logo: "chatgpt",
    href: "https://chatgpt.com",
    publishedAt: "2026-08-28T10:00:00.000Z",
  },
  {
    id: "github",
    name: "GitHub",
    slug: "github",
    description: "پلتفرم میزبانی کد، همکاری تیمی و مدیریت نسخه‌ها",
    category: "devops",
    categoryLabel: "DevOps",
    price: "freemium",
    rating: 4.9,
    reviewCount: 21400,
    logo: "github",
    href: "https://github.com",
    publishedAt: "2026-08-27T09:00:00.000Z",
  },
  {
    id: "vercel",
    name: "Vercel",
    slug: "vercel",
    description: "پلتفرم دیپلوی سریع برای فرانت‌اند و اپلیکیشن‌های مدرن",
    category: "frontend",
    categoryLabel: "Frontend",
    price: "freemium",
    rating: 4.8,
    reviewCount: 9320,
    logo: "vercel",
    href: "https://vercel.com",
    publishedAt: "2026-08-26T12:00:00.000Z",
  },
  {
    id: "figma",
    name: "Figma",
    slug: "figma",
    description: "ابزار طراحی رابط کاربری و همکاری تیمی روی UI/UX",
    category: "design",
    categoryLabel: "طراحی",
    price: "freemium",
    rating: 4.9,
    reviewCount: 15600,
    logo: "figma",
    href: "https://figma.com",
    publishedAt: "2026-08-25T11:00:00.000Z",
  },
  {
    id: "vscode",
    name: "VS Code",
    slug: "vscode",
    description: "ویرایشگر کد سبک و قدرتمند با اکوسیستم افزونه‌های گسترده",
    category: "web",
    categoryLabel: "توسعه وب",
    price: "free",
    rating: 4.9,
    reviewCount: 28600,
    logo: "vscode",
    href: "https://code.visualstudio.com",
    publishedAt: "2026-08-24T08:00:00.000Z",
  },
  {
    id: "postman",
    name: "Postman",
    slug: "postman",
    description: "تست، مستندسازی و همکاری روی APIها در یک محیط واحد",
    category: "backend",
    categoryLabel: "Backend",
    price: "freemium",
    rating: 4.7,
    reviewCount: 8740,
    logo: "postman",
    href: "https://www.postman.com",
    publishedAt: "2026-08-23T14:00:00.000Z",
  },
  {
    id: "docker",
    name: "Docker",
    slug: "docker",
    description: "کانتینرسازی اپلیکیشن‌ها برای توسعه و دیپلوی یکسان",
    category: "devops",
    categoryLabel: "DevOps",
    price: "freemium",
    rating: 4.8,
    reviewCount: 14220,
    logo: "docker",
    href: "https://www.docker.com",
    publishedAt: "2026-08-22T10:30:00.000Z",
  },
  {
    id: "mongodb",
    name: "MongoDB",
    slug: "mongodb",
    description: "دیتابیس سندگرا برای اپلیکیشن‌های مقیاس‌پذیر و منعطف",
    category: "database",
    categoryLabel: "دیتابیس",
    price: "freemium",
    rating: 4.7,
    reviewCount: 7650,
    logo: "mongodb",
    href: "https://www.mongodb.com",
    publishedAt: "2026-08-21T09:20:00.000Z",
  },
  {
    id: "tailwind",
    name: "Tailwind CSS",
    slug: "tailwind-css",
    description: "فریم‌ورک utility-first برای ساخت UI سریع و یکدست",
    category: "frontend",
    categoryLabel: "Frontend",
    price: "free",
    rating: 4.9,
    reviewCount: 11890,
    logo: "tailwind",
    href: "https://tailwindcss.com",
    publishedAt: "2026-08-20T13:00:00.000Z",
  },
  {
    id: "netlify",
    name: "Netlify",
    slug: "netlify",
    description: "میزبانی و دیپلوی خودکار سایت‌های استاتیک و Jamstack",
    category: "web",
    categoryLabel: "توسعه وب",
    price: "freemium",
    rating: 4.6,
    reviewCount: 5430,
    logo: "netlify",
    href: "https://www.netlify.com",
    publishedAt: "2026-08-19T16:00:00.000Z",
  },
  {
    id: "prisma",
    name: "Prisma",
    slug: "prisma",
    description: "ORM مدرن برای TypeScript با schema و تایپ‌سیفتی کامل",
    category: "backend",
    categoryLabel: "Backend",
    price: "freemium",
    rating: 4.8,
    reviewCount: 6920,
    logo: "prisma",
    href: "https://www.prisma.io",
    publishedAt: "2026-08-18T11:40:00.000Z",
  },
  {
    id: "firebase",
    name: "Firebase",
    slug: "firebase",
    description: "بک‌اند آماده گوگل برای احراز هویت، دیتابیس و موبایل",
    category: "mobile",
    categoryLabel: "موبایل",
    price: "freemium",
    rating: 4.7,
    reviewCount: 10250,
    logo: "firebase",
    href: "https://firebase.google.com",
    publishedAt: "2026-08-17T10:00:00.000Z",
  },
  {
    id: "linear",
    name: "Linear",
    slug: "linear",
    description: "مدیریت پروژه نرم‌افزاری با سرعت بالا و تجربه کاربری ممتاز",
    category: "web",
    categoryLabel: "توسعه وب",
    price: "paid",
    rating: 4.8,
    reviewCount: 4210,
    logo: "linear",
    href: "https://linear.app",
    publishedAt: "2026-08-16T09:00:00.000Z",
  },
  {
    id: "snyk",
    name: "Snyk",
    slug: "snyk",
    description: "اسکن امنیت وابستگی‌ها و آسیب‌پذیری‌های کد و کانتینر",
    category: "security",
    categoryLabel: "امنیت",
    price: "freemium",
    rating: 4.6,
    reviewCount: 3180,
    logo: "snyk",
    href: "https://snyk.io",
    publishedAt: "2026-08-15T12:30:00.000Z",
  },
  {
    id: "supabase",
    name: "Supabase",
    slug: "supabase",
    description: "جایگزین متن‌باز Firebase با Postgres و API آماده",
    category: "database",
    categoryLabel: "دیتابیس",
    price: "freemium",
    rating: 4.8,
    reviewCount: 5870,
    logo: "supabase",
    href: "https://supabase.com",
    publishedAt: "2026-08-14T15:20:00.000Z",
  },
  {
    id: "cursor",
    name: "Cursor",
    slug: "cursor",
    description: "ویرایشگر کد مبتنی بر AI برای توسعه سریع‌تر و هوشمندتر",
    category: "ai",
    categoryLabel: "AI",
    price: "paid",
    rating: 4.9,
    reviewCount: 9740,
    logo: "cursor",
    href: "https://cursor.com",
    publishedAt: "2026-08-13T08:50:00.000Z",
  },
];
