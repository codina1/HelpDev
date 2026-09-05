import type { MarketplaceArticle, ArticleCategoryId } from "@/data/articles";
import type { ContentSummaryDto } from "@/lib/api/content";
import { resolveContentCoverUrl, labelForContentType } from "@/lib/admin/content/content-mappers";
import { isArticleType } from "@/lib/public/content-helpers";

const COVER_TONES = [
  "from-[#3B82F6]/35 to-[#7C3AED]/10",
  "from-[#F59E0B]/30 to-[#7C3AED]/10",
  "from-[#22D3EE]/30 to-[#6366F1]/10",
  "from-[#38BDF8]/30 to-[#0EA5E9]/10",
  "from-[#A855F7]/30 to-[#7C3AED]/10",
  "from-[#34D399]/25 to-[#059669]/10",
] as const;

const CATEGORY_RULES: Array<{
  id: Exclude<ArticleCategoryId, "all">;
  label: string;
  match: RegExp;
}> = [
  { id: "ai", label: "AI", match: /\bai\b|llm|gpt|claude|openai|mcp|cursor|هوش\s*مصنوعی/i },
  { id: "frontend", label: "Frontend", match: /react|next\.?js|frontend|فرانت|vue|angular/i },
  { id: "dotnet", label: ".NET", match: /\.net|dotnet|c#|asp\.?\s*net/i },
  { id: "backend", label: "Backend", match: /backend|بک\s*اند|node\.?js|api|nestjs/i },
  { id: "devops", label: "DevOps", match: /devops|docker|kubernetes|ci\/?cd|linux/i },
  { id: "security", label: "Security", match: /security|امنیت|oauth|jwt/i },
  { id: "architecture", label: "Architecture", match: /architecture|معماری|microservice|ddd/i },
  { id: "tools", label: "Tools", match: /tool|ابزار|vscode|git/i },
  { id: "programming", label: "Programming", match: /program|کد|typescript|javascript|الگوریتم/i },
];

function inferCategory(title: string, slug: string, type: string): {
  category: Exclude<ArticleCategoryId, "all">;
  categoryLabel: string;
} {
  const hay = `${title} ${slug} ${type}`;
  for (const rule of CATEGORY_RULES) {
    if (rule.match.test(hay)) {
      return { category: rule.id, categoryLabel: rule.label };
    }
  }
  if (type.toLowerCase() === "news") {
    return { category: "programming", categoryLabel: "News" };
  }
  return { category: "programming", categoryLabel: labelForContentType(type) || "مقاله" };
}

function initialsFromTitle(title: string): string {
  const cleaned = title.replace(/[^\p{L}\p{N}\s]/gu, " ").trim();
  const parts = cleaned.split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "HD";
  if (parts.length === 1) return parts[0].slice(0, 2);
  return `${parts[0][0] ?? ""}${parts[1][0] ?? ""}`;
}

function estimateMinutes(title: string): number {
  return Math.max(3, Math.min(18, Math.ceil(title.trim().length / 10)));
}

/** Map published API content into marketplace card shape (UI-only enrichment). */
export function mapPublishedContentToMarketplace(
  items: ContentSummaryDto[],
): MarketplaceArticle[] {
  const articles = items.filter((item) => isArticleType(item.type));

  return articles.map((item, index) => {
    const { category, categoryLabel } = inferCategory(item.title, item.slug, item.type);
    const cover = resolveContentCoverUrl(item.coverImage);
    return {
      id: item.id,
      slug: item.slug,
      title: item.title,
      description: `مطالعه مقاله «${item.title}» در HelpDev.`,
      category,
      categoryLabel,
      level: "intermediate",
      coverImage: cover || "/news/cover-react.png",
      coverTone: COVER_TONES[index % COVER_TONES.length],
      author: "تیم HelpDev",
      authorInitials: initialsFromTitle(item.title),
      readingMinutes: estimateMinutes(item.title),
      views: item.views ?? 0,
      publishedAt: item.createdAt,
      featured: index === 0,
    };
  });
}
