import type { Metadata } from "next";
import { ToolEditor } from "@/components/admin/content/workspaces/tool/tool-editor";

export const metadata: Metadata = { title: "ابزار جدید" };

export default function AdminContentToolsNewPage() {
  return <ToolEditor />;
}
