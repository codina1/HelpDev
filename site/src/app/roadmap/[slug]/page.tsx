import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { RoadmapDetailView } from "@/components/roadmap-detail/RoadmapDetailView";
import { getRoadmapDetail } from "@/data/roadmap-detail";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const model = getRoadmapDetail(slug);
  if (!model) return { title: "نقشه راه" };
  return {
    title: model.title,
    description: model.description,
  };
}

export default async function RoadmapDetailPage({ params }: PageProps) {
  const { slug } = await params;
  const model = getRoadmapDetail(slug);
  if (!model) notFound();
  return <RoadmapDetailView model={model} />;
}
