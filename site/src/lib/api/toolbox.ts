import { apiRequest } from "./client";

export type ToolCategoryDto = {
  id: string;
  name: string;
  slug: string;
};

export type ToolSummaryDto = {
  id: string;
  title: string;
  slug: string;
  categorySlug?: string;
  status: string;
};

export type ToolDetailDto = ToolSummaryDto & {
  description?: string;
  inputSchema?: unknown;
};

export type ToolExecutionResponseDto = {
  output: string;
  correlationId?: string;
};

export function listToolCategories(signal?: AbortSignal): Promise<ToolCategoryDto[]> {
  return apiRequest<ToolCategoryDto[]>({ path: "/tools/categories", signal });
}

export function listTools(signal?: AbortSignal): Promise<ToolSummaryDto[]> {
  return apiRequest<ToolSummaryDto[]>({ path: "/tools", signal });
}

export function getToolBySlug(slug: string, signal?: AbortSignal): Promise<ToolDetailDto> {
  return apiRequest<ToolDetailDto>({
    path: `/tools/${encodeURIComponent(slug)}`,
    signal,
  });
}

/**
 * Executes a tool. Input values are sent in the request body only; they are
 * never placed in the URL, logged, or forwarded to analytics.
 */
export function executeTool(
  slug: string,
  input: Record<string, unknown>,
  options?: { token?: string | null; signal?: AbortSignal },
): Promise<ToolExecutionResponseDto> {
  return apiRequest<ToolExecutionResponseDto>({
    method: "POST",
    path: `/tools/${encodeURIComponent(slug)}/execute`,
    body: input,
    token: options?.token,
    signal: options?.signal,
  });
}
