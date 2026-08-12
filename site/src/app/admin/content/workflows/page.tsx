import type { Metadata } from "next";
import { ContentWorkflowsDashboard } from "@/components/admin/content/workflows/content-workflows-dashboard";

export const metadata: Metadata = { title: "گردش کار AI محتوا" };

export default function AdminContentWorkflowsPage() {
  return <ContentWorkflowsDashboard />;
}
