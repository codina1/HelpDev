import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import { PageErrorState } from "@/components/ui/page-error-state";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import { getContentBySlug } from "@/lib/api/content";
import { ApiClientError } from "@/lib/api/errors";
import { resolveArticleExcerpt } from "@/data/article-detail";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const article = await getContentBySlug(slug);
    const cover = resolveContentCoverUrl(article.coverImage);
    return {
      title: article.title,
      description: resolveArticleExcerpt(article),
      openGraph: cover
        ? {
            images: [{ url: cover }],
          }
        : undefined,
    };
  } catch {
    return { title: "مقاله" };
  }
}

export default async function ArticleDetailPage({ params }: PageProps) {
  const { slug } = await params;

  try {
    const article = await getContentBySlug(slug);
    return <ArticleDetailView article={article} />;
  } catch (error) {
    if (error instanceof ApiClientError && error.status === 404) {
      notFound();
    }
    return (
      <div className="space-y-4 bg-[#050816] px-4 py-8">
        <Link href="/articles" className="focus-ring text-[13px] text-violet-300">
          ← بازگشت به مقالات
        </Link>
        <PageErrorState error={error} />
      </div>
    );
  }
}
