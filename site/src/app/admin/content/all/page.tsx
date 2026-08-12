import type { Metadata } from "next";
import { ContentDashboard } from "@/components/admin/content/content-dashboard";

export const metadata: Metadata = { title: "همه محتواها" };

/** Legacy full CMS list — reachable from the content platform hub. */
export default function AdminContentAllPage() {
  return <ContentDashboard />;
}
