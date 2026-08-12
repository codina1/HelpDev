import type { Metadata } from "next";
import { RoadmapList } from "@/components/admin/content/workspaces/roadmap/roadmap-editor";

export const metadata: Metadata = { title: "سازندهٔ نقشه راه" };

export default function AdminContentRoadmapsPage() {
  return <RoadmapList />;
}
