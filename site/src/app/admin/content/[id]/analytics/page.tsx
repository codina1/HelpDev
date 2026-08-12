import type { Metadata } from "next";
import { ContentItemAnalyticsWorkspace } from "@/components/admin/analytics/content/content-item-analytics-workspace";

export const metadata: Metadata = { title: "تحلیل محتوا" };

export default async function AdminContentItemAnalyticsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentItemAnalyticsWorkspace contentId={decodeURIComponent(id)} />;
}
