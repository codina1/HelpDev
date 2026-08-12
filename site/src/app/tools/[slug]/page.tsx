import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/ui/placeholder-page";

export const metadata: Metadata = { title: "ابزار" };

type PageProps = { params: Promise<{ slug: string }> };

/** Public tool page foundation — full catalog UI comes later; slug route reserved. */
export default async function PublicToolBySlugPage({ params }: PageProps) {
  const { slug } = await params;
  return (
    <PlaceholderPage
      title="کاتالوگ ابزار"
      description={`صفحهٔ عمومی ابزار «${decodeURIComponent(slug)}» — فاندیشن مسیر /tools/[slug].`}
    />
  );
}
