import type { Metadata } from "next";
import { TutorialEditor } from "@/components/admin/content/workspaces/tutorial/tutorial-editor";

export const metadata: Metadata = { title: "آموزش جدید" };

export default function AdminContentTutorialsNewPage() {
  return <TutorialEditor />;
}
