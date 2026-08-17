import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { PublicPromptLabPackPage } from "@/components/public/prompt-lab/public-prompt-lab-pack-page";
import { getPromptLabPack, PROMPT_LAB_PACKS } from "@/lib/public/prompt-lab-pack-mock";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export function generateStaticParams() {
  return PROMPT_LAB_PACKS.map((pack) => ({ slug: pack.slug }));
}

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const pack = getPromptLabPack(slug);
  if (!pack) {
    return { title: "پک پرامپت" };
  }
  return {
    title: pack.title,
    description: pack.description,
  };
}

export default async function PromptLabPackPage({ params }: PageProps) {
  const { slug } = await params;
  const pack = getPromptLabPack(slug);
  if (!pack) {
    notFound();
  }

  return <PublicPromptLabPackPage pack={pack} />;
}
