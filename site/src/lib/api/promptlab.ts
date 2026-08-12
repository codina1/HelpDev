import { apiRequest } from "./client";

export type PromptCategoryDto = {
  id: string;
  name: string;
  slug: string;
};

export type PromptSummaryDto = {
  id: string;
  title: string;
  slug: string;
  categorySlug?: string;
  status: string;
};

export type PromptDetailDto = PromptSummaryDto & {
  description?: string;
  variables?: Array<{ name: string; label?: string; required?: boolean }>;
};

export type PromptRenderResponseDto = {
  renderedPrompt: string;
  correlationId?: string;
};

export function listPromptCategories(signal?: AbortSignal): Promise<PromptCategoryDto[]> {
  return apiRequest<PromptCategoryDto[]>({ path: "/prompts/categories", signal });
}

export function listPrompts(signal?: AbortSignal): Promise<PromptSummaryDto[]> {
  return apiRequest<PromptSummaryDto[]>({ path: "/prompts", signal });
}

export function getPromptBySlug(slug: string, signal?: AbortSignal): Promise<PromptDetailDto> {
  return apiRequest<PromptDetailDto>({
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
