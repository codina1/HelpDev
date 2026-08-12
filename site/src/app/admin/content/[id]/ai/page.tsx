import type { Metadata } from "next";
import { ContentAiWorkspace } from "@/components/admin/content/ai/content-ai-workspace";

export const metadata: Metadata = { title: "دستیار هوش مصنوعی" };

export default async function AdminContentAiPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentAiWorkspace contentId={decodeURIComponent(id)} />;
}
