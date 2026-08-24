import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArticleDetailView } from "@/components/public/articles/article-detail-view";
import { PageErrorState } from "@/components/ui/page-error-state";
import { resolveContentCoverUrl } from "@/lib/admin/content/content-mappers";
import { getContentBySlug } from "@/lib/api/content";
import { ApiClientError } from "@/lib/api/errors";

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
      description: article.body?.slice(0, 160) || article.title,
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
    return (
      <div>
        <div className="mx-auto w-full max-w-[1200px] px-4 pt-6 sm:px-5 lg:px-6">
          <nav aria-label="مسیر صفحه" className="text-[12px] text-[color:var(--pub-muted)]">
            <Link href="/articles" className="focus-ring rounded hover:text-[color:var(--pub-ai-from)]">
              مقالات
            </Link>
            <span className="mx-2" aria-hidden>
              /
            </span>
            <span className="text-[color:var(--pub-fg)]">{article.title}</span>
          </nav>
        </div>
        <ArticleDetailView article={article} />
      </div>
    );
  } catch (error) {
    if (error instanceof ApiClientError && error.status === 404) {
      notFound();
    }
    return (
      <div className="space-y-4">
        <Link href="/articles" className="focus-ring text-[13px] text-violet-300">
          ← بازگشت به مقالات
        </Link>
        <PageErrorState error={error} />
      </div>
    );
  }
}
