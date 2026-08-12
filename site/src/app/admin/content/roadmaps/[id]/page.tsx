import type { Metadata } from "next";
import { RoadmapBuilderDetail } from "@/components/admin/content/workspaces/roadmap/roadmap-editor";

export const metadata: Metadata = { title: "ویرایش نقشه راه" };

type PageProps = { params: Promise<{ id: string }> };

export default async function AdminContentRoadmapDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <RoadmapBuilderDetail contentId={decodeURIComponent(id)} />;
}
