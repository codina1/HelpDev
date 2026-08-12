import type { NavItem } from "@/lib/constants";

/** Compact product nav for Sprint 50B public header. */
export const PUBLIC_PRODUCTS_NAV: NavItem[] = [
  { href: "/articles", label: "مقالات" },
  { href: "/toolbox", label: "ابزارها" },
  { href: "/roadmap", label: "نقشه راه" },
  { href: "/learning", label: "یادگیری" },
];

export const PUBLIC_BOTTOM_NAV = [
  { href: "/", label: "خانه", icon: "home" as const },
  { href: "/search", label: "جستجو", icon: "search" as const },
  { href: "/learning", label: "یادگیری", icon: "learn" as const },
  { href: "/profile", label: "پروفایل", icon: "profile" as const },
] as const;

/** Decorative knowledge-graph nodes for hero (not content data). */
export const HERO_KNOWLEDGE_NODES = [
  { id: "articles", label: "Articles", x: 18, y: 28 },
  { id: "tools", label: "Tools", x: 78, y: 22 },
  { id: "roadmaps", label: "Roadmaps", x: 22, y: 72 },
  { id: "ai", label: "AI", x: 72, y: 68 },
] as const;

/** Decorative frontend path structure for roadmap experience (structural demo). */
export const FRONTEND_PATH_DEMO = {
  title: "Frontend Engineer",
  nodes: [
    { label: "HTML" },
    { label: "JavaScript" },
    { label: "React" },
    { label: "Next.js" },
  ],
} as const;
