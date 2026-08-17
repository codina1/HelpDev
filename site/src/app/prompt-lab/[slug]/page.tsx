import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicPromptLabDetailPage } from "@/components/public/prompt-lab/public-prompt-lab-detail-page";
import {
  getPromptLabDetail,
  relatedPromptLabPrompts,
  similarPromptLabPrompts,
} from "@/lib/public/prompt-lab-detail-mock";
import { PROMPT_LAB_PROMPTS } from "@/lib/public/prompt-lab-mock";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export function generateStaticParams() {
  return PROMPT_LAB_PROMPTS.map((prompt) => ({ slug: prompt.slug }));
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const detail = getPromptLabDetail(slug);
  if (!detail) {
    return { title: "پرامپت" };
  }
  return {
    title: detail.title,
    description: detail.description,
  };
}

export default async function PromptLabDetailPage({ params }: PageProps) {
  const { slug } = await params;
  const detail = getPromptLabDetail(slug);
  if (!detail) {
    notFound();
  }

  return (
    <PublicPromptLabDetailPage
      detail={detail}
      related={relatedPromptLabPrompts(detail.slug)}
      similar={similarPromptLabPrompts(detail.slug)}
    />
  );
}
