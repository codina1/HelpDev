export const DASHBOARD_MAIN_NAV = [
  { href: "/", label: "خانه", icon: "home" },
  { href: "/news", label: "اخبار", icon: "news" },
  { href: "/roadmap", label: "مسیرهای یادگیری", icon: "roadmap" },
  { href: "/toolbox", label: "ابزارها", icon: "tools" },
  { href: "/prompt-lab", label: "Prompt Lab", icon: "prompt" },
  { href: "/courses", label: "دوره‌ها", icon: "courses" },
] as const;

export const DASHBOARD_SECONDARY_NAV = [
  { href: "/articles", label: "منتخب سردبیر" },
  { href: "/news", label: "پرطرفدارترین‌ها" },
  { href: "/courses", label: "پیشنهادهای ویژه" },
] as const;

export const INTEREST_TAG_COLORS = [
  "bg-violet-500/15 text-violet-200 border-violet-500/25",
  "bg-blue-500/15 text-blue-200 border-blue-500/25",
  "bg-cyan-500/15 text-cyan-200 border-cyan-500/25",
  "bg-emerald-500/15 text-emerald-200 border-emerald-500/25",
  "bg-amber-500/15 text-amber-200 border-amber-500/25",
  "bg-rose-500/15 text-rose-200 border-rose-500/25",
] as const;

export const CATEGORY_PREFERENCES = [
  { id: "dotnet", label: ".NET", color: "from-violet-600 to-purple-600" },
  { id: "ai", label: "AI & ML", color: "from-cyan-500 to-blue-600" },
  { id: "devops", label: "DevOps", color: "from-emerald-500 to-teal-600" },
  { id: "frontend", label: "Frontend", color: "from-pink-500 to-rose-600" },
  { id: "backend", label: "Backend", color: "from-indigo-500 to-violet-600" },
  { id: "database", label: "Database", color: "from-amber-500 to-orange-600" },
] as const;

export const MOCK_STATS = {
  readArticles: 156,
  followedPaths: 7,
  savedNotes: 24,
  studyHours: 4.2,
  usedTools: 18,
  trends: {
    readArticles: 12,
    studyHours: 8,
    savedArticles: 5,
    usedTools: 15,
  },
} as const;

export const MOCK_LEARNING = {
  title: "نقشه راه توسعه‌دهنده بک‌اند",
  progress: 42,
  nextChapter: "فصل ۵: پایگاه داده",
  href: "/roadmap",
} as const;

export const MOCK_SAVED_ITEMS = [
  {
    id: "1",
    title: "راهنمای کامل .NET 8",
    category: "مقاله",
    tab: "articles" as const,
    time: "۲ روز پیش",
    thumb: "📘",
  },
  {
    id: "2",
    title: "SQL Cheat Sheet",
    category: "ابزار",
    tab: "tools" as const,
    time: "۵ روز پیش",
    thumb: "🗃️",
  },
  {
    id: "3",
    title: "ASP.NET Core Masterclass",
    category: "برنامه",
    tab: "programs" as const,
    time: "۱ هفته پیش",
    thumb: "🎓",
  },
  {
    id: "4",
    title: "Docker برای مبتدیان",
    category: "مقاله",
    tab: "articles" as const,
    time: "۲ هفته پیش",
    thumb: "🐳",
  },
] as const;

export const MOCK_ACTIVITY = [
  {
    id: "1",
    text: "مقاله «معماری Clean Architecture» را خواندید",
    time: "۳ ساعت پیش",
    icon: "📄",
    color: "text-violet-400 bg-violet-500/10",
  },
  {
    id: "2",
    text: "از ابزار Postman Collections استفاده کردید",
    time: "دیروز",
    icon: "🛠️",
    color: "text-cyan-400 bg-cyan-500/10",
  },
  {
    id: "3",
    text: "مسیر Backend را ادامه دادید — فصل ۴",
    time: "۲ روز پیش",
    icon: "🗺️",
    color: "text-emerald-400 bg-emerald-500/10",
  },
  {
    id: "4",
    text: "Prompt Lab — پرامپت SQL Generator",
    time: "۳ روز پیش",
    icon: "✨",
    color: "text-pink-400 bg-pink-500/10",
  },
] as const;

export const WEEKLY_ACTIVITY = [
  { day: "ش", value: 35 },
  { day: "ی", value: 55 },
  { day: "د", value: 42 },
  { day: "س", value: 78 },
  { day: "چ", value: 65 },
  { day: "پ", value: 48 },
  { day: "ج", value: 90 },
] as const;
