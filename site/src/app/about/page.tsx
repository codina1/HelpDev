import type { Metadata } from "next";
import { AboutHero, ABOUT_HERO_SUBTITLE } from "./about-hero";
import { AboutMission } from "./about-mission";
import { AboutStory } from "./about-story";

export const metadata: Metadata = {
  title: "درباره ما",
  description: ABOUT_HERO_SUBTITLE,
};

export default function AboutPage() {
  return (
    <>
      <AboutHero />
      <AboutMission />
      <AboutStory />
    </>
  );
}
