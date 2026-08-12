import type { Metadata } from "next";
import { ContentEditView } from "@/components/admin/content/editor/content-edit-view";

export const metadata: Metadata = { title: "ویرایش محتوا" };

export default async function AdminContentEditPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentEditView id={decodeURIComponent(id)} />;
}
