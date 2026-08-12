import type { Metadata } from "next";
import { RoadmapTracker } from "@/components/roadmap/roadmap-tracker";
import { FRONTEND_ROADMAP } from "@/data/roadmap";

export const metadata: Metadata = {
  title: "رودمپ",
};

export default function RoadmapPage() {
  return (
    <RoadmapTracker
      title={FRONTEND_ROADMAP.title}
      description={FRONTEND_ROADMAP.description}
      steps={[...FRONTEND_ROADMAP.steps]}
    />
  );
}
