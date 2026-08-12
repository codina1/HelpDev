import { apiRequest } from "./client";

export type EnrollmentListItemDto = {
  id: string;
  courseId: string;
  userId: string;
  enrolledAt: string;
  status: string;
  progressPercentage: number;
};

export type LessonProgressDto = {
  lessonId: string;
  startedAt: string | null;
  completedAt: string | null;
  isCompleted: boolean;
};

export type EnrollmentDto = {
  id: string;
  courseId: string;
  userId: string;
  enrolledAt: string;
  status: string;
  progressPercentage: number;
  lessonProgress: LessonProgressDto[];
};

/** GET /learning/me/enrollments — authenticated user's enrollments. */
export function listMyEnrollments(
  token: string,
  signal?: AbortSignal,
): Promise<EnrollmentListItemDto[]> {
  return apiRequest<EnrollmentListItemDto[]>({
    path: "/learning/me/enrollments",
    token,
    signal,
    cache: "no-store",
  });
}

/** GET /learning/me/enrollments/{id} */
export function getMyEnrollment(
  token: string,
  enrollmentId: string,
  signal?: AbortSignal,
): Promise<EnrollmentDto> {
  return apiRequest<EnrollmentDto>({
    path: `/learning/me/enrollments/${encodeURIComponent(enrollmentId)}`,
    token,
    signal,
    cache: "no-store",
  });
}
