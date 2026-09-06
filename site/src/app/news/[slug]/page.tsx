import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import { PageErrorState } from "@/components/ui/page-error-state";
import {
  getNewsArticleBySlug,
  getNewsDetailBody,
  parseNewsViews,
} from "@/data/news-articles";
import { resolveArticleExcerpt } from "@/data/article-detail";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import { getContentBySlug, type ContentDetailDto } from "@/lib/api/content";
import { ApiClientError } from "@/lib/api/errors";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export const dynamic = "force-dynamic";

function catalogNewsToDetail(slug: string): ContentDetailDto | null {
  const item = getNewsArticleBySlug(slug);
  if (!item) return null;
  const minutes = Number.parseInt(item.readTime.replace(/[^\d]/g, ""), 10) || 5;
  return {
    id: item.id,
    title: item.title,
    slug: item.slug,
    type: "News",
    status: "Published",
    views: parseNewsViews(item.views),
    saves: 0,
    createdAt: new Date().toISOString(),
    coverImage: item.image,
    body: getNewsDetailBody(item),
    authorId: "helpdev",
    authorName: "تیم HelpDev",
    authorRole: "Editorial",
    readingTimeMinutes: minutes,
    contentFormat: "markdown",
  };
}

/** Prefer API news, but fill missing cover/body from catalog so UI never looks empty. */
function mergeWithCatalog(api: ContentDetailDto, slug: string): ContentDetailDto {
  const catalog = catalogNewsToDetail(slug);
  if (!catalog) return { ...api, type: api.type || "News" };

  const cover = (api.coverImage ?? "").trim() || catalog.coverImage;
  const body = (api.body ?? "").trim();
  const looksLikePlaceholder =
    !body ||
    body.includes("برای محتوای کامل CMS") ||
    body.includes("از کاتالوگ HelpDev برای نمایش جزئیات");

  return {
    ...api,
    type: "News",
    coverImage: cover,
    body: looksLikePlaceholder ? catalog.body : api.body,
    contentHtml: looksLikePlaceholder ? null : api.contentHtml,
    contentFormat: looksLikePlaceholder ? "markdown" : api.contentFormat,
    authorName: api.authorName?.trim() || catalog.authorName,
    authorRole: api.authorRole?.trim() || catalog.authorRole,
    readingTimeMinutes: api.readingTimeMinutes || catalog.readingTimeMinutes,
    views: api.views > 0 ? api.views : catalog.views,
  };
}

async function resolveNewsDetail(slug: string): Promise<ContentDetailDto | null> {
  try {
    const content = await getContentBySlug(slug);
    const type = content.type.toLowerCase();
    if (type === "news" || type === "article") {
      return mergeWithCatalog(content, slug);
    }
    return catalogNewsToDetail(slug);
  } catch (error) {
    if (error instanceof ApiClientError && error.status === 404) {
      return catalogNewsToDetail(slug);
    }
    const fallback = catalogNewsToDetail(slug);
    if (fallback) return fallback;
    throw error;
  }
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const news = await resolveNewsDetail(slug);
    if (!news) return { title: "خبر" };
    const cover = resolveContentCoverUrl(news.coverImage);
    return {
      title: news.title,
      description: resolveArticleExcerpt(news),
      alternates: { canonical: `/news/${news.slug}` },
      openGraph: cover
        ? {
            title: news.title,
            description: resolveArticleExcerpt(news),
            images: [{ url: cover }],
          }
        : {
            title: news.title,
            description: resolveArticleExcerpt(news),
          },
    };
  } catch {
    return { title: "خبر" };
  }
}

export default async function NewsDetailPage({ params }: PageProps) {
  const { slug } = await params;

  try {
    const news = await resolveNewsDetail(slug);
    if (!news) notFound();
    return <ArticleDetailView article={news} />;
  } catch (error) {
    if (error instanceof ApiClientError && error.status === 404) {
      notFound();
    }
    return (
      <div className="space-y-4 bg-[#050816] px-4 py-8">
        <Link href="/news" className="focus-ring text-[13px] text-violet-300">
          ← بازگشت به اخبار
        </Link>
        <PageErrorState error={error} />
      </div>
    );
  }
}
