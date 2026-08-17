import type { Metadata } from "next";
import { AdminPromptReviewDashboard } from "@/components/admin/prompt-lab/admin-prompt-review-dashboard";

export const metadata: Metadata = { title: "بازبینی پرامپت‌ها" };

export default function AdminPromptsPage() {
  return <AdminPromptReviewDashboard />;
}
