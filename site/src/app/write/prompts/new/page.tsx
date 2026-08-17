import type { Metadata } from "next";
import { WriterPromptEditor } from "@/components/write/writer-prompt-editor";

export const metadata: Metadata = { title: "پرامپت جدید" };

export default function WritePromptNewPage() {
  return <WriterPromptEditor variant="public" />;
}
