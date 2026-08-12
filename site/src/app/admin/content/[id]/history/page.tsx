import type { Metadata } from "next";
import { ContentHistoryWorkspace } from "@/components/admin/content/history/content-history-workspace";

export const metadata: Metadata = { title: "تاریخچه محتوا" };

export default async function AdminContentHistoryPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentHistoryWorkspace contentId={decodeURIComponent(id)} />;
}
