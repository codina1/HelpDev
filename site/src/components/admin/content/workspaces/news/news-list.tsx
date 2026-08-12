"use client";

import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";

export function NewsList() {
  return <ContentWorkspaceList workspace={getWorkspaceByKey("news")} />;
}
