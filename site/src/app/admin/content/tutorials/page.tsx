import type { Metadata } from "next";
import { TutorialList } from "@/components/admin/content/workspaces/tutorial/tutorial-editor";

export const metadata: Metadata = { title: "آموزش‌های کوتاه" };

export default function AdminContentTutorialsPage() {
  return <TutorialList />;
}
