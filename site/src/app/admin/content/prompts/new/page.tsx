import type { Metadata } from "next";
import { PromptEditor } from "@/components/admin/content/workspaces/prompt/prompt-editor";

export const metadata: Metadata = { title: "پرامپت جدید" };

export default function AdminContentPromptsNewPage() {
  return <PromptEditor />;
}
