import type { Metadata } from "next";
import { ArticleEditor } from "@/components/admin/content/workspaces/article/article-editor";

export const metadata: Metadata = { title: "مقاله جدید" };

export default function AdminContentArticlesNewPage() {
  return <ArticleEditor />;
}
