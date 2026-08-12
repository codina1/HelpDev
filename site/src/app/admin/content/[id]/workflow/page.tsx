import type { Metadata } from "next";
import { ContentWorkflowWorkspace } from "@/components/admin/content/workflow/content-workflow-workspace";

export const metadata: Metadata = { title: "گردش کار محتوا" };

export default async function AdminContentWorkflowPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentWorkflowWorkspace contentId={decodeURIComponent(id)} />;
}
