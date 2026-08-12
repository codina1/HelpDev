export const SITE = {
  name: "HelpDev",
  description: "پلتفرم دانش مهندسی هوش مصنوعی — مقالات، ابزارها، نقشه راه و دستیار یادگیری",
} as const;

export type NavItem = {
  href: string;
  label: string;
  badge?: string;
};

export const HEADER_NAV: NavItem[] = [
  { href: "/", label: "خانه" },
  { href: "/news", label: "اخبار" },
  { href: "/articles", label: "مقالات" },
  { href: "/courses", label: "دوره‌ها" },
  { href: "/learning", label: "یادگیری" },
  { href: "/learning/assistant", label: "دستیار یادگیری", badge: "AI" },
  { href: "/dashboard", label: "داشبورد" },
  { href: "/toolbox", label: "ابزارها" },
  { href: "/prompt-lab", label: "Prompt Lab", badge: "جدید" },
  { href: "/write", label: "نویسنده شو" },
];

export const CATEGORY_LINKS = [
  { href: "/news", label: "اخبار", icon: "📰", color: "from-violet-500 to-purple-600", badge: "داغ" as const },
  { href: "/articles", label: "مقالات", icon: "📄", color: "from-blue-500 to-cyan-500" },
  { href: "/courses", label: "دوره‌ها", icon: "🎓", color: "from-amber-400 to-orange-500", badge: "جدید" as const },
  { href: "/roadmap", label: "نقشه راه", icon: "🗺️", color: "from-emerald-400 to-green-500" },
  { href: "/toolbox", label: "ابزارها", icon: "🛠️", color: "from-sky-400 to-blue-500" },
  { href: "/prompt-lab", label: "Prompt Lab", icon: "✨", color: "from-pink-400 to-rose-500", badge: "AI" as const },
  { href: "/github-trending", label: "GitHub Trending", icon: "⭐", color: "from-slate-400 to-slate-600", badge: "ترند" as const },
  { href: "/starter-kit", label: "Dev Starter Kit", icon: "🚀", color: "from-orange-400 to-red-500" },
] as const;

export const HERO_QUICK_LINKS = [
  { href: "/roadmap", label: "Roadmap", icon: "🗺️" },
  { href: "/cheat-sheets", label: "Cheat Sheet", icon: "📋" },
  { href: "/prompt-lab", label: "Prompt Lab", icon: "✨" },
  { href: "/courses", label: "Courses", icon: "🎓" },
] as const;
