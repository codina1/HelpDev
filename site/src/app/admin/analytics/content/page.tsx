import type { Metadata } from "next";
import { ContentAnalyticsDashboard } from "@/components/admin/analytics/content/content-analytics-dashboard";

export const metadata: Metadata = { title: "تحلیل محتوا" };

export default function AdminContentAnalyticsPage() {
  return <ContentAnalyticsDashboard />;
}
