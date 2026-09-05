import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { CourseDetailView } from "@/components/course/CourseDetailView";
import { getCourseDetailBySlug } from "@/data/course-detail";

type PageProps = {
  params: Promise<{ slug: string }>;
};

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const course = getCourseDetailBySlug(slug);
  if (!course) return { title: "دوره" };
  return {
    title: `${course.title}${course.titleAccent ? ` ${course.titleAccent}` : ""}`,
    description: course.description,
  };
}

export default async function CourseDetailPage({ params }: PageProps) {
  const { slug } = await params;
  const course = getCourseDetailBySlug(slug);
  if (!course) {
    notFound();
  }

  return <CourseDetailView course={course} />;
}
