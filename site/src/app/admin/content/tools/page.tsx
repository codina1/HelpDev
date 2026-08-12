import type { Metadata } from "next";
import { ToolList } from "@/components/admin/content/workspaces/tool/tool-editor";

export const metadata: Metadata = { title: "مدیریت ابزارها" };

export default function AdminContentToolsPage() {
  return <ToolList />;
}
