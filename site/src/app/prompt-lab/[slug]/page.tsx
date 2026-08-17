import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { getPromptBySlug } from "@/lib/api/promptlab";
import { ApiClientError } from "@/lib/api/errors";
import {
  EMPTY_PROMPT_LAB_CATALOG_PAGE,
  excludePromptSlug,
  fetchPromptLabCatalog,
} from "@/lib/public/prompt-lab-catalog";
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

    return (
      <PublicPromptLabDetailPage
        detail={detail}
        related={excludePromptSlug(relatedPage.items, detail.slug).slice(0, 3)}
        similar={excludePromptSlug(similarPage.items, detail.slug).slice(0, 4)}
      />
    );
  } catch (error) {
    if (error instanceof ApiClientError && error.isNotFound) {
      notFound();
    }
    return <PublicPromptLabDetailError error={error} />;
  }
}
