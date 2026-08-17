"use client";

import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { adminContentTutorialRoute } from "@/lib/admin/routes";
import { ContentStudio } from "@/components/admin/content/editor/content-studio";
import { ContentWorkspaceList } from "@/components/admin/content/workspaces/content-workspace-list";

const workspace = getWorkspaceByKey("tutorial");

export function TutorialList() {
  return <ContentWorkspaceList workspace={workspace} />;
}

export function TutorialEditor() {
  return (
    <ContentStudio createType="Course" createSuccessPath={adminContentTutorialRoute} />
  );
}
