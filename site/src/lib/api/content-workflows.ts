import { apiRequest } from "./client";

export type AiContentWorkflowListItemDto = {
  id: string;
  ideaId: string;
  ideaTitle: string;
  ideaStatus: string;
  currentStep: string;
  createdByUserId: string;
  linkedContentId: string | null;
  updatedAtUtc: string;
};

export type ContentIdeaDto = {
  id: string;
  title: string;
  description: string;
  targetType: string;
  status: string;
  createdByUserId: string;
  createdAtUtc: string;
  updatedAtUtc: string;
};

export type AiContentWorkflowSessionDto = {
  id: string;
  ideaId: string;
  currentStep: string;
  createdByUserId: string;
  linkedContentId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  idea: ContentIdeaDto;
};

export type AiResearchResultDto = {
  summary: string;
  sources: Array<{
    title: string;
    url: string;
    sourceType: string;
    snippet: string;
  }>;
  model: string;
  provider: string;
  createdAtUtc: string;
};

export type ContentOutlineDto = {
  title: string;
  sections: Array<{ heading: string; subheadings: string[] }>;
  rawText: string;
  model: string;
  provider: string;
  createdAtUtc: string;
};

export type DraftSuggestionDto = {
  title: string;
  bodyMarkdown: string;
  model: string;
  provider: string;
  createdAtUtc: string;
};

export type SeoOptimizationSuggestionDto = {
  suggestedTitle: string | null;
  suggestedDescription: string | null;
  keywordSuggestions: string[];
  recommendations: string[];
  createdAtUtc: string;
};

export type ApplyDraftResultDto = {
  workflowId: string;
  contentId: string;
  revisionVersion: number;
  contentStatus: string;
};

export function createAiContentWorkflow(
  token: string,
  body: { title: string; description?: string; targetType?: string },
  signal?: AbortSignal,
): Promise<AiContentWorkflowSessionDto> {
  return apiRequest<AiContentWorkflowSessionDto>({
    method: "POST",
    path: "/admin/content/workflows",
    token,
    body,
    signal,
  });
}

export function listAiContentWorkflows(
  token: string,
  signal?: AbortSignal,
): Promise<AiContentWorkflowListItemDto[]> {
  return apiRequest<AiContentWorkflowListItemDto[]>({
    path: "/admin/content/workflows",
    token,
    signal,
  });
}

export function getAiContentWorkflow(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AiContentWorkflowSessionDto> {
  return apiRequest<AiContentWorkflowSessionDto>({
    path: `/admin/content/workflows/${encodeURIComponent(id)}`,
    token,
    signal,
  });
}

export function researchAiContentWorkflow(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<AiResearchResultDto> {
  return apiRequest<AiResearchResultDto>({
    method: "POST",
    path: `/admin/content/workflows/${encodeURIComponent(id)}/research`,
    token,
    signal,
  });
}

export function outlineAiContentWorkflow(
  token: string,
  id: string,
  researchSummary?: string,
  signal?: AbortSignal,
): Promise<ContentOutlineDto> {
  return apiRequest<ContentOutlineDto>({
    method: "POST",
    path: `/admin/content/workflows/${encodeURIComponent(id)}/outline`,
    token,
    body: { researchSummary: researchSummary ?? null },
    signal,
  });
}

export function draftAiContentWorkflow(
  token: string,
  id: string,
  body: { outlineTitle: string; outlineText: string },
  signal?: AbortSignal,
): Promise<DraftSuggestionDto> {
  return apiRequest<DraftSuggestionDto>({
    method: "POST",
    path: `/admin/content/workflows/${encodeURIComponent(id)}/draft`,
    token,
    body,
    signal,
  });
}

export function seoAiContentWorkflow(
  token: string,
  id: string,
  body: { title: string; body: string; slug?: string; focusKeyword?: string },
  signal?: AbortSignal,
): Promise<SeoOptimizationSuggestionDto> {
  return apiRequest<SeoOptimizationSuggestionDto>({
    method: "POST",
    path: `/admin/content/workflows/${encodeURIComponent(id)}/seo`,
    token,
    body,
    signal,
  });
}

export function applyAiContentWorkflowDraft(
  token: string,
  id: string,
  body: { title: string; body: string; slug?: string; targetType?: string },
  signal?: AbortSignal,
): Promise<ApplyDraftResultDto> {
  return apiRequest<ApplyDraftResultDto>({
    method: "POST",
    path: `/admin/content/workflows/${encodeURIComponent(id)}/apply-draft`,
    token,
    body,
    signal,
  });
}
