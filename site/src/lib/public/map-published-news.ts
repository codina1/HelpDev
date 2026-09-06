import type { ContentSummaryDto } from "@/lib/api/content";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import type { NewsArticle, NewsTag } from "@/types";

function isNewsType(type: string): boolean {
  return type.toLowerCase() === "news";
}

function inferTag(title: string, slug: string): NewsTag {
  const hay = `${title} ${slug}`.toLowerCase();
  if (/\.net|dotnet|c#/.test(hay)) return ".NET";
  if (/devops|docker|kubernetes|ci/.test(hay)) return "DevOps";
  if (/react|frontend|next/.test(hay)) return "React";
  return "AI";
}

function inferCategoryLabel(tag: NewsTag, title: string, slug: string): string {
  const hay = `${title} ${slug}`.toLowerCase();
  if (tag === ".NET") return ".NET";
  if (tag === "DevOps") return "DevOps";
  if (tag === "React") return "Frontend";
  if (/tool|vscode|copilot|mcp/.test(hay)) return "Tools";
  return "AI";
}

function formatRelativeFa(): string {
  return "اخیراً";
}

function formatViews(views: number): string {
  if (views >= 1000) {
    const value = views / 1000;
    const rounded = value >= 10 ? Math.round(value) : Math.round(value * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return views.toLocaleString("fa-IR");
}

function estimateReadTime(title: string): string {
  const minutes = Math.max(3, Math.min(12, Math.ceil(title.trim().length / 12)));
  return `${minutes.toLocaleString("fa-IR")} دقیقه`;
}

/** Map published News content into listing card shape. */
export function mapPublishedNewsToArticles(items: ContentSummaryDto[]): NewsArticle[] {
  return items
    .filter((item) => isNewsType(item.type))
    .map((item) => {
      const tag = inferTag(item.title, item.slug);
      const cover = resolveContentCoverUrl(item.coverImage);
      return {
        id: item.id,
        slug: item.slug,
        title: item.title,
        tag,
        categoryLabel: inferCategoryLabel(tag, item.title, item.slug),
        summary: `مطالعه خبر «${item.title}» در HelpDev.`,
        time: formatRelativeFa(),
        image: cover || "/news/cover-react.png",
        readTime: estimateReadTime(item.title),
        views: formatViews(item.views ?? 0),
      };
    });
}

export function publicNewsPath(slug: string): string {
  return `/news/${encodeURIComponent(slug)}`;
}
