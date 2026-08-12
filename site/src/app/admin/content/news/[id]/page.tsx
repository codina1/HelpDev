import type { Metadata } from "next";
import { ContentEditView } from "@/components/admin/content/editor/content-edit-view";

export const metadata: Metadata = { title: "ویرایش خبر" };

type PageProps = { params: Promise<{ id: string }> };

export default async function AdminContentNewsDetailPage({ params }: PageProps) {
  const { id } = await params;
  return <ContentEditView id={decodeURIComponent(id)} />;
}
