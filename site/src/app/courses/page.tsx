import type { Metadata } from "next";
import { CoursesCatalog } from "@/components/courses/courses-catalog";
import { CoursesHero } from "@/components/courses/courses-hero";
import { COURSES } from "@/data/courses";

export const metadata: Metadata = {
  title: "دوره‌ها",
};

export default function CoursesPage() {
  return (
    <>
      <CoursesHero />
      <CoursesCatalog courses={COURSES} />
    </>
  );
}
