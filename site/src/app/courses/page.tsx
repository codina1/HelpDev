import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { CoursesGrid } from "@/components/courses/courses-grid";
import { COURSES } from "@/data/courses";

export const metadata: Metadata = {
  title: "دوره‌ها",
};

export default function CoursesPage() {
  return (
    <>
      <PageHeader
        title="دوره‌ها"
        description="دوره‌های منتخب در فرانت‌اند، بک‌اند، هوش مصنوعی و DevOps."
      />
      <CoursesGrid courses={COURSES} />
    </>
  );
}
