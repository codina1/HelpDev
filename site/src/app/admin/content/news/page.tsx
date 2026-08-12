import type { Metadata } from "next";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";

export const metadata: Metadata = { title: "مدیریت اخبار" };

export default function AdminContentNewsPage() {
  return <ContentWorkspaceList workspace={getWorkspaceByKey("news")} />;
}
