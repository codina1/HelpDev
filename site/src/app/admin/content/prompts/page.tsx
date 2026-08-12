import type { Metadata } from "next";
import { PromptList } from "@/components/admin/content/workspaces/prompt/prompt-list";

export const metadata: Metadata = { title: "فضای کار Prompt" };

export default function AdminContentPromptsPage() {
  return <PromptList />;
}
