import type { Metadata } from "next";
import { ToolWorkspaceDetail } from "@/components/admin/content/workspaces/tool/tool-editor";

export const metadata: Metadata = { title: "ویرایش ابزار" };

type PageProps = { params: Promise<{ id: string }> };

export default async function AdminContentToolDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <ToolWorkspaceDetail contentId={decodeURIComponent(id)} />;
}
