import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ToolDetailView } from "@/components/tool-detail/ToolDetailView";
import { getToolBySlug } from "@/lib/api/toolbox";
import { getToolDetailBySlug, mergeToolApiDetail } from "@/data/tool-detail";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export const dynamic = "force-dynamic";

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const model = getToolDetailBySlug(slug);
  if (!model) return { title: "ابزار" };
  return {
    title: model.name,
    description: model.description,
    openGraph: {
      title: model.name,
      description: model.description,
    },
    alternates: {
      canonical: `/tools/${model.slug}`,
    },
  };
}

export default async function PublicToolBySlugPage({ params }: PageProps) {
  const { slug } = await params;
  const catalog = getToolDetailBySlug(slug);
  if (!catalog) notFound();

  let model = catalog;
  try {
    const api = await getToolBySlug(catalog.slug);
    model = mergeToolApiDetail(catalog, api);
  } catch {
    // Catalog detail remains the source of truth when API tool library is empty.
  }

  return (
    <>
      <script
        type="application/ld+json"
        dangerouslySetInnerHTML={{
          __html: JSON.stringify({
            "@context": "https://schema.org",
            "@type": "SoftwareApplication",
            name: model.name,
            applicationCategory: model.toolType,
            operatingSystem: model.platforms.join(", "),
            url: model.websiteUrl,
            aggregateRating: {
              "@type": "AggregateRating",
              ratingValue: model.rating,
              ratingCount: model.ratingCount,
            },
          }),
        }}
      />
      <ToolDetailView model={model} />
    </>
  );
}
