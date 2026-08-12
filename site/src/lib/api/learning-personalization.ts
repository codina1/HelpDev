import { apiRequest } from "@/lib/api/client";

export type LearningPreferenceDto = {
  topic: string;
  priority: number;
  interestLevel: number;
};

export type LearningProfileDto = {
  userId: string;
  experienceLevel: string;
  learningGoals: string;
  currentSkills: string;
  preferredTopics: LearningPreferenceDto[];
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type UpdateLearningProfileRequest = {
  experienceLevel: string;
  learningGoals?: string | null;
  currentSkills?: string | null;
  preferredTopics?: LearningPreferenceDto[];
};

export type RecommendedLearningItemDto = {
  kind: string;
  courseId: string | null;
  title: string;
  slug: string | null;
  rationale: string | null;
};

export type LearningRecommendationDto = {
  recommendedItems: RecommendedLearningItemDto[];
  reason: string;
  nextSteps: string[];
  generatedAtUtc: string;
};

export type LearningRoadmapStepDto = {
  stepOrder: number;
  title: string;
  description: string;
  relatedCourseId: string | null;
};

export type LearningRoadmapDto = {
  id: string;
  goal: string;
  status: string;
  steps: LearningRoadmapStepDto[];
  createdAtUtc: string;
  updatedAtUtc: string;
  approvedAtUtc: string | null;
};

export const LEARNING_TOPIC_OPTIONS = [".NET", "AI", "Frontend", "Architecture", "DevOps"] as const;

export async function fetchLearningProfile(token: string, signal?: AbortSignal) {
  return apiRequest<LearningProfileDto>({
    token,
    method: "GET",
    path: "/me/learning-profile",
    signal,
    cache: "no-store",
  });
}

export async function updateLearningProfile(
  token: string,
  body: UpdateLearningProfileRequest,
  signal?: AbortSignal,
) {
  return apiRequest<LearningProfileDto>({
    token,
    method: "PUT",
    path: "/me/learning-profile",
    body,
    signal,
  });
}

export async function fetchLearningRecommendations(token: string, signal?: AbortSignal) {
  return apiRequest<LearningRecommendationDto>({
    token,
    method: "GET",
    path: "/me/recommendations",
    signal,
    cache: "no-store",
  });
}

export async function fetchLearningRoadmap(token: string, signal?: AbortSignal) {
  return apiRequest<LearningRoadmapDto | null>({
    token,
    method: "GET",
    path: "/me/roadmap",
    signal,
    cache: "no-store",
  });
}

export async function generateLearningRoadmap(
  token: string,
  goal?: string | null,
  signal?: AbortSignal,
) {
  return apiRequest<LearningRoadmapDto>({
    token,
    method: "POST",
    path: "/me/roadmap/generate",
    body: { goal: goal ?? null },
    signal,
  });
}

export async function approveLearningRoadmap(token: string, signal?: AbortSignal) {
  return apiRequest<LearningRoadmapDto>({
    token,
    method: "POST",
    path: "/me/roadmap/approve",
    signal,
  });
}
