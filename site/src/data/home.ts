export const DEV_DIGEST = [
  {
    id: "1",
    title: "Next.js 15 منتشر شد",
    time: "۲ ساعت پیش",
    icon: "▲",
    iconBg: "bg-black text-white",
    badge: "NEW" as const,
  },
  {
    id: "2",
    title: ".NET 9 Preview",
    time: "۵ ساعت پیش",
    icon: ".NET",
    iconBg: "bg-violet-600 text-white",
    badge: "Preview" as const,
  },
  {
    id: "3",
    title: "GitHub Copilot به‌روز شد",
    time: "دیروز",
    icon: "🐙",
    iconBg: "bg-slate-800 text-white",
  },
  {
    id: "4",
    title: "Tailwind CSS v4",
    time: "۲ روز پیش",
    icon: "〜",
    iconBg: "bg-cyan-500 text-white",
    badge: "v4" as const,
  },
];

export const HOT_NEWS = [
  { id: "1", title: "OpenAI o3 معرفی شد", time: "۱ ساعت پیش", tag: "AI", tagColor: "bg-violet-500/20 text-violet-300 border-violet-500/30", hot: true },
  { id: "2", title: "VS Code 1.88 منتشر شد", time: "۳ ساعت پیش", tag: "Tools", tagColor: "bg-slate-500/20 text-slate-300 border-slate-500/30" },
  { id: "3", title: "React Compiler پایدارتر شد", time: "۵ ساعت پیش", tag: "React", tagColor: "bg-sky-500/20 text-sky-300 border-sky-500/30", hot: true },
  { id: "4", title: "الگوهای معماری Backend", time: "دیروز", tag: "Backend", tagColor: "bg-emerald-500/20 text-emerald-300 border-emerald-500/30" },
  { id: "5", title: "Tailwind v4 و طراحی مدرن", time: "۲ روز پیش", tag: "CSS", tagColor: "bg-cyan-500/20 text-cyan-300 border-cyan-500/30" },
];

export const POPULAR_ROADMAPS = [
  { id: "1", title: "Frontend Developer", progress: 65, icon: "⚛️", color: "from-emerald-400 to-cyan-400", level: "محبوب" },
  { id: "2", title: "Backend Developer", progress: 42, icon: "⚙️", color: "from-violet-400 to-indigo-400", level: "متوسط" },
  { id: "3", title: ".NET Developer", progress: 28, icon: "🔷", color: "from-sky-400 to-blue-500", level: "جدید" },
];

export const LATEST_NEWS_GRID = [
  { id: "1", title: "React 19 Compiler نزدیک به stable", tag: "React", time: "۱ ساعت پیش", thumb: "⚛️", isNew: true },
  { id: "2", title: ".NET 10 Preview برای APIهای ابری", tag: ".NET", time: "۲ ساعت پیش", thumb: "🔷" },
  { id: "3", title: "ایجنت‌های AI در ریفکتور چندفایلی", tag: "AI", time: "۳ ساعت پیش", thumb: "🤖", isNew: true },
];

export const LATEST_ARTICLES = [
  { id: "1", title: "اصول SOLID به‌زبان ساده", meta: "۸ دقیقه · ۱.۲k بازدید", icon: "📐" },
  { id: "2", title: "راهنمای عملی Docker", meta: "۱۲ دقیقه · ۹۸۰ بازدید", icon: "🐳" },
  { id: "3", title: "احراز هویت با JWT", meta: "۱۰ دقیقه · ۸۷۰ بازدید", icon: "🔐" },
  { id: "4", title: "اشتباهات رایج JavaScript", meta: "۶ دقیقه · ۷۴۰ بازدید", icon: "📜" },
];

export const RECOMMENDED_COURSES = [
  { id: "1", title: "The Complete React Developer", platform: "Udemy", rating: 4.8, thumb: "⚛️", free: false, badge: "محبوب" as const },
  { id: "2", title: "ASP.NET Core Web API", platform: "Udemy", rating: 4.7, thumb: "🔷", free: false, badge: "پیشنهاد" as const },
  { id: "3", title: "Docker & Kubernetes", platform: "YouTube", rating: 4.6, thumb: "🐳", free: true },
];

export const CHEAT_SHEETS = [
  { id: "1", title: "Git", icon: "🔀", href: "/toolbox", updated: true },
  { id: "2", title: "SQL", icon: "🗄️", href: "/toolbox" },
  { id: "3", title: "JavaScript", icon: "📜", href: "/toolbox", updated: true },
  { id: "4", title: "Linux", icon: "🐧", href: "/toolbox" },
];

export const PROMPT_LAB_ITEMS = [
  { id: "1", title: "API Prompt Generator", href: "/prompt-lab", badge: "AI" as const },
  { id: "2", title: "Debugging Prompt", href: "/prompt-lab" },
  { id: "3", title: "Architecture Prompt", href: "/prompt-lab", badge: "Pro" as const },
  { id: "4", title: "Code Review Prompt", href: "/prompt-lab" },
];

export const GITHUB_TRENDING = [
  { id: "1", name: "microsoft/vscode", stars: "175k", icon: "💻", trending: true },
  { id: "2", name: "vercel/next.js", stars: "132k", icon: "▲", trending: true },
  { id: "3", name: "facebook/react", stars: "236k", icon: "⚛️" },
  { id: "4", name: "dotnet/aspnetcore", stars: "36k", icon: "🔷" },
];

export const STARTER_KITS = [
  { id: "1", title: "Next.js Starter", stack: "App Router + Tailwind", icon: "▲", badge: "آماده" as const },
  { id: "2", title: "React + Vite", stack: "TypeScript آماده", icon: "⚛️" },
  { id: "3", title: "ASP.NET Core", stack: "Web API قالب", icon: "🔷", badge: "جدید" as const },
  { id: "4", title: "Node.js API", stack: "Express + Prisma", icon: "🟢" },
];

export const PLATFORM_FEATURES = [
  { id: "1", title: "سریع و به‌روز", desc: "جدیدترین اخبار و ابزارها", icon: "⚡" },
  { id: "2", title: "محتوای باکیفیت", desc: "منابع منتخب برای یادگیری", icon: "✅" },
  { id: "3", title: "ابزارهای کاربردی", desc: "چیت‌شیت و اسنیپت آماده", icon: "🛠️" },
  { id: "4", title: "مسیر یادگیری شفاف", desc: "رودمپ مرحله‌به‌مرحله", icon: "🗺️" },
  { id: "5", title: "جامعه توسعه‌دهندگان", desc: "همراه با جامعه فعال", icon: "👥" },
];

export const HERO_STATS = [
  { label: "50K+ توسعه‌دهنده", variant: "tag" as const },
  { label: "روزانه به‌روز", variant: "live" as const },
  { label: "۱۰۰+ منبع", variant: "tag" as const },
] as const;
