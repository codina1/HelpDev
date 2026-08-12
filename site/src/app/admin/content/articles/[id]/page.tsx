import type { Metadata } from "next";
import { ContentEditView } from "@/components/admin/content/editor/content-edit-view";

export const metadata: Metadata = { title: "ویرایش مقاله" };

type PageProps = { params: Promise<{ id: string }> };

/** Article workspace detail — Content Studio + Article Settings panel. */
export default async function AdminContentArticleDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <ContentEditView id={decodeURIComponent(id)} />;
}
