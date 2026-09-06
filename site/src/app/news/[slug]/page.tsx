import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import { PageErrorState } from "@/components/ui/page-error-state";
import { getNewsArticleBySlug } from "@/data/news-articles";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import { getContentBySlug, type ContentDetailDto } from "@/lib/api/content";
import { ApiClientError } from "@/lib/api/errors";
import { resolveArticleExcerpt } from "@/data/article-detail";

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
    views: 0,
    saves: 0,
    createdAt: new Date().toISOString(),
    coverImage: item.image,
    body: `## ${item.title}\n\n${item.summary}\n\nاین خبر از کاتالوگ HelpDev برای نمایش جزئیات بارگذاری شده است. برای محتوای کامل CMS، خبر را در پنل محتوا منتشر کنید.`,
    authorId: "helpdev",
    authorName: "تیم HelpDev",
    authorRole: "Editorial",
    readingTimeMinutes: minutes,
    contentFormat: "markdown",
  };
}

async function resolveNewsDetail(slug: string): Promise<ContentDetailDto | null> {
  try {
    const content = await getContentBySlug(slug);
    const type = content.type.toLowerCase();
    if (type === "news" || type === "article") return content;
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
