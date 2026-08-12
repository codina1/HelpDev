import type { Metadata } from "next";
import { ContentWorkflowWizard } from "@/components/admin/content/workflows/content-workflow-wizard";

export const metadata: Metadata = { title: "جزئیات گردش کار AI" };

type PageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminContentWorkflowDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <ContentWorkflowWizard workflowId={id} />;
}
