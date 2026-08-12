import { apiRequest } from "./client";

export type CourseSummaryDto = {
  id: string;
  title: string;
  slug: string;
  status: string;
  instructorId?: string;
  createdAt?: string;
  publishedAt?: string | null;
  sectionCount?: number;
  lessonCount?: number;
  /** Optional display field when present in future API shapes. */
  level?: string;
};

export function listCourses(signal?: AbortSignal): Promise<CourseSummaryDto[]> {
  return apiRequest<CourseSummaryDto[]>({
    path: "/learning/courses",
    signal,
  });
}

export function getCourseBySlug(slug: string, signal?: AbortSignal): Promise<CourseSummaryDto> {
  return apiRequest<CourseSummaryDto>({
    path: `/learning/courses/${encodeURIComponent(slug)}`,
    signal,
  });
}
