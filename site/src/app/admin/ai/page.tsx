import type { Metadata } from "next";
import { AiDashboard } from "@/components/admin/ai/ai-dashboard";

export const metadata: Metadata = { title: "عملیات AI" };

export default function AdminAiPage() {
  return <AiDashboard />;
}
