import type { Metadata } from "next";
import { ArticleList } from "@/components/admin/content/workspaces/article/article-list";

export const metadata: Metadata = { title: "مدیریت مقالات" };

export default function AdminContentArticlesPage() {
  return <ArticleList />;
}
