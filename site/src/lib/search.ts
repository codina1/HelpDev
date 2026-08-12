import { COURSES } from "@/data/courses";
import { NEWS_ARTICLES } from "@/data/news-articles";
import { FRONTEND_ROADMAP } from "@/data/roadmap";
import { TOOLBOX_ITEMS } from "@/data/toolbox";
import type { SearchResult, SearchTab } from "@/types";

export const SEARCH_TABS: Array<{ id: SearchTab; label: string }> = [
  { id: "news", label: "اخبار" },
  { id: "roadmap", label: "رودمپ" },
  { id: "tools", label: "ابزارها" },
  { id: "courses", label: "دوره‌ها" },
];

export const SEARCH_INDEX: SearchResult[] = [
  ...NEWS_ARTICLES.map((article) => ({
    id: `news-${article.id}`,
    tab: "news" as const,
    title: article.title,
    summary: article.summary,
    meta: `${article.tag} · ${article.time}`,
    href: "/news",
  })),
  ...FRONTEND_ROADMAP.steps.map((step) => ({
    id: `roadmap-${step.id}`,
    tab: "roadmap" as const,
    title: step.title,
    summary: step.description,
    meta: `${FRONTEND_ROADMAP.title} · Step`,
    href: "/roadmap",
  })),
  ...TOOLBOX_ITEMS.map((tool) => ({
    id: `tools-${tool.id}`,
    tab: "tools" as const,
    title: tool.title,
    summary: tool.description,
    meta: "Toolbox · Snippet",
    href: "/toolbox",
  })),
  ...COURSES.map((course) => ({
    id: `courses-${course.id}`,
    tab: "courses" as const,
    title: course.title,
    summary: `${course.level} course on ${course.platform}`,
    meta: `${course.category} · ${course.rating.toFixed(1)}★`,
    href: "/courses",
  })),
];

function matchesQuery(result: SearchResult, query: string): boolean {
  const haystack = `${result.title} ${result.summary} ${result.meta}`.toLowerCase();
  return query
    .toLowerCase()
    .split(/\s+/)
    .filter(Boolean)
    .every((token) => haystack.includes(token));
}

export function searchKnowledge(
  query: string,
  tab: SearchTab,
): SearchResult[] {
  const normalized = query.trim();

  return SEARCH_INDEX.filter((result) => {
    if (result.tab !== tab) return false;
    if (!normalized) return true;
    return matchesQuery(result, normalized);
  });
}

export function countByTab(query: string): Record<SearchTab, number> {
  const tabs = SEARCH_TABS.map((tab) => tab.id);

  return tabs.reduce(
    (counts, tab) => {
      counts[tab] = searchKnowledge(query, tab).length;
      return counts;
    },
    {} as Record<SearchTab, number>,
  );
}
