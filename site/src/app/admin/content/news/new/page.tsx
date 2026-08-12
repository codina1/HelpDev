import type { Metadata } from "next";
import { NewsEditor } from "@/components/admin/content/workspaces/news/news-editor";

export const metadata: Metadata = { title: "خبر جدید" };

export default function AdminContentNewsNewPage() {
  return <NewsEditor />;
}
