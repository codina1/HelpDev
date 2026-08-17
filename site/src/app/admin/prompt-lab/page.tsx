import type { Metadata } from "next";
import { WriterPromptDashboard } from "@/components/admin/prompt-lab/writer-prompt-dashboard";

export const metadata: Metadata = { title: "Writer Prompt Studio" };

export default function AdminPromptLabPage() {
  return <WriterPromptDashboard />;
}
