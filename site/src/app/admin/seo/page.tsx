import type { Metadata } from "next";
import { SeoDashboardWorkspace } from "@/components/admin/seo/seo-dashboard-workspace";

export const metadata: Metadata = { title: "تحلیل SEO" };

export default function AdminSeoPage() {
  return <SeoDashboardWorkspace />;
}
