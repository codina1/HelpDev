import type { Metadata } from "next";
import { ComparisonEditor } from "@/components/admin/content/workspaces/comparison/comparison-editor";

export const metadata: Metadata = { title: "مقایسه جدید" };

export default function AdminContentComparisonsNewPage() {
  return <ComparisonEditor />;
}
