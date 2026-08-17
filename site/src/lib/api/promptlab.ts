import { apiRequest } from "./client";

export type PromptCategoryDto = {
  id: string;
  name: string;
  slug: string;
  description?: string | null;
  icon?: string | null;
  displayOrder?: number;
};

export type PublicPromptCategoryRefDto = {
  id: string;
  name: string;
  slug: string;
};

export type PublicPromptAiModelRefDto = {
  id: string;
  name: string;
  slug: string;
  provider: string;
};

export type PublicPromptListItemDto = {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  coverImage: string | null;
  mediaType: string;
  category: PublicPromptCategoryRefDto;
  aiModel: PublicPromptAiModelRefDto;
  views: number;
  copyCount: number;
  publishedAt: string | null;
};

export type PublicPromptDetailsDto = PublicPromptListItemDto & {
  content: string;
};

export type PublicPromptPageDto = {
  page: number;
  pageSize: number;
  total: number;
  items: PublicPromptListItemDto[];
};

export type PublicPromptFilter = {
  category?: string;
  aiModel?: string;
  mediaType?: string;
  search?: string;
  popular?: boolean;
  page?: number;
  pageSize?: number;
};

export function listPromptCategories(signal?: AbortSignal): Promise<PromptCategoryDto[]> {
  return apiRequest<PromptCategoryDto[]>({ path: "/prompts/categories", signal });
}

export type PromptAiModelDto = {
  id: string;
  name: string;
  slug: string;
  provider: string;
};

export function listPromptAiModels(signal?: AbortSignal): Promise<PromptAiModelDto[]> {
  return apiRequest<PromptAiModelDto[]>({ path: "/prompts/ai-models", signal });
}

export function listPrompts(
  filter?: PublicPromptFilter,
  signal?: AbortSignal,
): Promise<PublicPromptPageDto> {
  return apiRequest<PublicPromptPageDto>({
    path: "/prompts",
    query: filter,
    signal,
  });
}

export function getPromptBySlug(slug: string, signal?: AbortSignal): Promise<PublicPromptDetailsDto> {
  return apiRequest<PublicPromptDetailsDto>({
    path: `/prompts/${encodeURIComponent(slug)}`,
    signal,
  });
}

/**
 * Renders a prompt. Variable values are sent in the request body only; they are
 * never placed in the URL, logged, or forwarded to analytics.
 */
export function renderPrompt(
  slug: string,
  variables: Record<string, unknown>,
  options?: { token?: string | null; signal?: AbortSignal },
): Promise<PromptRenderResponseDto> {
  return apiRequest<PromptRenderResponseDto>({
    method: "POST",
    path: `/prompts/${encodeURIComponent(slug)}/render`,
    body: variables,
    token: options?.token,
    signal: options?.signal,
  });
}

export type PromptRenderResponseDto = {
  renderedPrompt: string;
  correlationId?: string;
};
