import type { Metadata } from "next";
import { RoadmapEditor } from "@/components/admin/content/workspaces/roadmap/roadmap-editor";

export const metadata: Metadata = { title: "نقشه راه جدید" };

export default function AdminContentRoadmapsNewPage() {
  return <RoadmapEditor />;
}
