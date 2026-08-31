import type { Metadata } from "next";
import { RoadmapGuide } from "@/components/roadmap/roadmap-guide";
import { RoadmapHero } from "@/components/roadmap/roadmap-hero";
import { RoadmapPaths } from "@/components/roadmap/roadmap-paths";
import { RoadmapStats } from "@/components/roadmap/roadmap-stats";

export const metadata: Metadata = {
  title: "رودمپ",
};

export default function RoadmapPage() {
  return (
    <>
      <RoadmapHero />
      <RoadmapStats />
      <RoadmapPaths />
      <RoadmapGuide />
    </>
  );
}
