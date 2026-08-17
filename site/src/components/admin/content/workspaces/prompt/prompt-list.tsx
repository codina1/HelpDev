"use client";

import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { WriterPromptDashboard } from "@/components/admin/prompt-lab/writer-prompt-dashboard";

const workspace = getWorkspaceByKey("prompt");

/** Prompt workspace list — writer dashboard backed by Prompt Lab API. */
export function PromptList() {
  return (
    <WriterPromptDashboard workspace={workspace} basePath={ADMIN_ROUTES.contentPrompts} />
  );
}
