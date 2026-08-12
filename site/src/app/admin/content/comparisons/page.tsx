import type { Metadata } from "next";
import { ComparisonList } from "@/components/admin/content/workspaces/comparison/comparison-editor";

export const metadata: Metadata = { title: "مقایسه‌ها" };

export default function AdminContentComparisonsPage() {
  return <ComparisonList />;
}
