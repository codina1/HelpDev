"use client";

import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";

export function ArticleList() {
  return <ContentWorkspaceList workspace={getWorkspaceByKey("article")} />;
}
