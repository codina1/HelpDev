import { apiRequest } from "./client";

export type WriterPromptStatusDto = "Draft" | "Submitted" | "Approved" | "Rejected";

export type WriterPromptListItemDto = {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  coverImage: string | null;
  mediaType: string;
  categoryId: string;
  aiModelId: string;
  status: WriterPromptStatusDto;
  views: number;
  copyCount: number;
  createdAt: string;
  updatedAt: string;
  publishedAt: string | null;
};

export type WriterPromptPageDto = {
  page: number;
  pageSize: number;
  total: number;
  items: WriterPromptListItemDto[];
};

export type WriterPromptListFilter = {
  status?: WriterPromptStatusDto;
  page?: number;
  pageSize?: number;
};

export type WriterPromptDetailsDto = {
  id: string;
  title: string;
  slug: string;
  description: string | null;
  content: string;
  coverImage: string | null;
  mediaType: string;
  categoryId: string;
  aiModelId: string;
  status: WriterPromptStatusDto;
  views: number;
  copyCount: number;
  createdAt: string;
  updatedAt: string;
  publishedAt: string | null;
};

export type CreateWriterPromptRequest = {
  title: string;
  slug: string;
  description?: string | null;
  content: string;
  coverImage?: string | null;
  mediaType: string;
  categoryId: string;
  aiModelId: string;
};

export type UpdateWriterPromptRequest = CreateWriterPromptRequest;

export function listWriterPrompts(
  token: string,
  filter?: WriterPromptListFilter,
  signal?: AbortSignal,
): Promise<WriterPromptPageDto> {
  return apiRequest<WriterPromptPageDto>({
    path: "/writer/prompts",
    token,
    query: filter,
    signal,
  });
}

export function createWriterPrompt(
  token: string,
  body: CreateWriterPromptRequest,
  signal?: AbortSignal,
): Promise<WriterPromptDetailsDto> {
  return apiRequest<WriterPromptDetailsDto>({
    method: "POST",
    path: "/writer/prompts",
    token,
    body,
    signal,
  });
}

export function updateWriterPrompt(
  token: string,
  id: string,
  body: UpdateWriterPromptRequest,
  signal?: AbortSignal,
): Promise<WriterPromptDetailsDto> {
  return apiRequest<WriterPromptDetailsDto>({
    method: "PUT",
    path: `/writer/prompts/${encodeURIComponent(id)}`,
    token,
    body,
    signal,
  });
}

export function submitWriterPrompt(
  token: string,
  id: string,
  signal?: AbortSignal,
): Promise<WriterPromptDetailsDto> {
  return apiRequest<WriterPromptDetailsDto>({
    method: "POST",
    path: `/writer/prompts/${encodeURIComponent(id)}/submit`,
    token,
    signal,
  });
}
