import type { Metadata } from "next";
import { KnowledgeDashboardWorkspace } from "@/components/admin/search/knowledge-dashboard-workspace";

export const metadata: Metadata = { title: "دانش جستجو" };

export default function AdminSearchKnowledgePage() {
  return <KnowledgeDashboardWorkspace />;
}
