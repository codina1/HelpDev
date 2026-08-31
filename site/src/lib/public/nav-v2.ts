import type { NavItem } from "@/lib/constants";

/**
 * Compact product nav for the public header (RTL order as shown).
 * Home is prepended in the header itself.
 */
export const PUBLIC_PRODUCTS_NAV: NavItem[] = [
  { href: "/articles", label: "مقالات" },
  { href: "/courses", label: "یادگیری" },
  { href: "/roadmap", label: "Roadmap" },
  { href: "/prompt-lab", label: "Prompt Lab" },
  { href: "/toolbox", label: "ابزارها" },
  { href: "/news", label: "اخبار" },
];

export const PUBLIC_BOTTOM_NAV = [
  { href: "/", label: "خانه", icon: "home" as const },
  { href: "/search", label: "جستجو", icon: "search" as const },
  { href: "/courses", label: "یادگیری", icon: "learn" as const },
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
