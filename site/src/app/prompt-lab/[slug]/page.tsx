import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getPromptBySlug } from "@/lib/api/promptlab";
import { ApiClientError } from "@/lib/api/errors";
import {
  EMPTY_PROMPT_LAB_CATALOG_PAGE,
  excludePromptSlug,
  fetchPromptLabCatalog,
} from "@/lib/public/prompt-lab-catalog";
import {
  getPromptLabDetail,
  relatedPromptLabPrompts,
  similarPromptLabPrompts,
} from "@/lib/public/prompt-lab-detail-mock";
import { toPromptLabDetail } from "@/lib/public/prompt-lab-mappers";
import {
  PublicPromptLabDetailError,
  PublicPromptLabDetailPage,
} from "@/components/public/prompt-lab/public-prompt-lab-detail-page";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  try {
    const prompt = await getPromptBySlug(slug);
    return {
      title: prompt.title,
      description: prompt.description ?? prompt.title,
    };
  } catch {
    const mock = getPromptLabDetail(slug);
    if (mock) {
      return { title: mock.title, description: mock.description };
    }
    return { title: "پرامپت" };
  }
}

export default async function PromptLabDetailPage({ params }: PageProps) {
  const { slug } = await params;

  try {
    const dto = await getPromptBySlug(slug);
    const detail = toPromptLabDetail(dto);
    const [relatedPage, similarPage] = await Promise.all([
      fetchPromptLabCatalog({ category: detail.categorySlug, pageSize: 4 }).catch(
        () => EMPTY_PROMPT_LAB_CATALOG_PAGE,
      ),
      fetchPromptLabCatalog({ popular: true, pageSize: 4 }).catch(() => EMPTY_PROMPT_LAB_CATALOG_PAGE),
    ]);

    const related = excludePromptSlug(relatedPage.items, detail.slug).slice(0, 3);
    const similar = excludePromptSlug(similarPage.items, detail.slug).slice(0, 4);

    return (
      <PublicPromptLabDetailPage
        detail={detail}
        related={related.length > 0 ? related : relatedPromptLabPrompts(detail.slug, 3)}
        similar={similar.length > 0 ? similar : similarPromptLabPrompts(detail.slug, 4)}
      />
    );
  } catch (error) {
    // Catalog UI still uses sample prompts while production API has none published.
    // Prefer mock detail over a hard 404 for those known slugs.
    if (error instanceof ApiClientError && error.isNotFound) {
      const mock = getPromptLabDetail(slug);
      if (mock) {
        return (
          <PublicPromptLabDetailPage
            detail={mock}
            related={relatedPromptLabPrompts(slug, 3)}
            similar={similarPromptLabPrompts(slug, 4)}
          />
        );
      }
      notFound();
    }

    const mock = getPromptLabDetail(slug);
    if (mock) {
      return (
        <PublicPromptLabDetailPage
          detail={mock}
          related={relatedPromptLabPrompts(slug, 3)}
          similar={similarPromptLabPrompts(slug, 4)}
        />
      );
    }

    return <PublicPromptLabDetailError error={error} />;
  }
}
