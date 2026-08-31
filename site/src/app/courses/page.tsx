import type { Metadata } from "next";
import { CoursesHero } from "@/components/courses/courses-hero";
import { CoursesGrid } from "@/components/courses/courses-grid";
import { COURSES } from "@/data/courses";

export const metadata: Metadata = {
  title: "دوره‌ها",
};

export default function CoursesPage() {
  return (
    <>
      <CoursesHero />
      <CoursesGrid courses={COURSES} />
    </>
  );
}
