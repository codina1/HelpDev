import type { SearchResultItemDto } from "@/lib/api/search";

/**
 * Maps Search API items to public product routes.
 * Prefer server-provided absolute-path `url` when present.
 */
export function hrefForSearchResult(item: SearchResultItemDto): string {
  if (item.url && item.url.startsWith("/")) {
    return item.url;
  }

  const type = (item.type ?? item.sourceType ?? "").toLowerCase();
  const slug = item.slug?.trim();

  if (type === "article" || type === "news") {
    return slug ? `/articles/${encodeURIComponent(slug)}` : "/articles";
  }
  if (type === "content") {
    return slug ? `/articles/${encodeURIComponent(slug)}` : "/articles";
  }
  if (type === "roadmap" || type === "roadmapstep") {
    return slug ? `/roadmap?slug=${encodeURIComponent(slug)}` : "/roadmap";
  }
  if (type === "tool") {
    return slug ? `/tools/${encodeURIComponent(slug)}` : "/toolbox";
  }
  if (type === "course" || type === "lesson") {
    return slug ? `/courses?slug=${encodeURIComponent(slug)}` : "/courses";
  }
  if (type === "prompt") {
    return slug ? `/prompt-lab?slug=${encodeURIComponent(slug)}` : "/prompt-lab";
  }

  return `/search?q=${encodeURIComponent(item.title)}`;
}

export function labelForSearchSource(item: SearchResultItemDto): string {
  const type = (item.type ?? item.sourceType ?? "").toLowerCase();
  const labels: Record<string, string> = {
    content: "مقاله",
    article: "مقاله",
    news: "خبر",
    tool: "ابزار",
    course: "دوره",
    lesson: "درس",
    prompt: "پرامپت",
    roadmap: "نقشه راه",
    roadmapstep: "نقشه راه",
  };
  return labels[type] ?? (item.type ?? item.sourceType ?? "نتیجه");
}
