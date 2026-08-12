import type { Metadata } from "next";
import { ContentDetailsView } from "@/components/admin/content/details/content-details-view";

export const metadata: Metadata = { title: "جزئیات محتوا" };

export default async function AdminContentDetailsPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <ContentDetailsView id={decodeURIComponent(id)} />;
}
