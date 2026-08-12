import { apiRequest } from "./client";

/** Mirrors backend SearchItemDto (camelCase JSON). */
export type SearchResultItemDto = {
  sourceType: string;
  sourceId: string;
  title: string;
  slug: string;
  summary?: string;
  url?: string;
  publishedAtUtc?: string | null;
  updatedAtUtc?: string;
  /** Convenience aliases used by UI helpers. */
  id?: string;
  type?: string;
};

export type SearchResultDto = {
  query: string;
  total: number;
  page: number;
  pageSize: number;
  items: SearchResultItemDto[];
};

export type SearchParams = {
  q?: string;
  type?: string;
  page?: number;
  pageSize?: number;
};

function normalizeItem(item: SearchResultItemDto): SearchResultItemDto {
  return {
    ...item,
    id: item.id ?? item.sourceId,
    type: item.type ?? item.sourceType,
  };
}

export function search(params: SearchParams, signal?: AbortSignal): Promise<SearchResultDto> {
  return apiRequest<SearchResultDto>({
    path: "/search",
    query: {
      q: params.q,
      type: params.type,
      page: params.page,
      pageSize: params.pageSize,
    },
    signal,
  }).then((result) => ({
    ...result,
    items: (result.items ?? []).map(normalizeItem),
  }));
}

export type SearchContextItemDto = {
  title: string;
  snippet: string;
  sourceUrl: string;
  sourceType: string;
  sourceId: string;
  similarity: number;
};

export type SearchContextDto = {
  query: string;
  items: SearchContextItemDto[];
};

export type KnowledgeSourceStatusDto = {
  sourceType: string;
  sourceId: string;
  status: string;
  chunkCount: number;
  updatedAtUtc: string;
  failureCode: string | null;
};

export type KnowledgeDashboardDto = {
  indexedDocuments: number;
  totalChunks: number;
  indexedSources: number;
  failedSources: number;
  sourceFilter?: string | null;
  recentFailures: KnowledgeSourceStatusDto[];
  recentIndexed: KnowledgeSourceStatusDto[];
};

export type SemanticSearchResultDto = {
  title: string;
  type: string;
  snippet: string;
  url: string;
  similarity: number;
};

export type SemanticSearchResponseDto = {
  query: string;
  results: SemanticSearchResultDto[];
};

export type RagAnswerDto = {
  answer: string;
  sources: Array<{
    title: string;
    sourceUrl: string;
    sourceType: string;
    sourceId: string;
    similarity: number;
  }>;
  generatedAtUtc: string;
};

/** GET /search/semantic — anonymous semantic retrieval (no vectors). */
export function searchSemantic(
  q: string,
  take = 8,
  signal?: AbortSignal,
): Promise<SemanticSearchResponseDto> {
  return apiRequest<SemanticSearchResponseDto>({
    path: "/search/semantic",
    query: { q, take },
    signal,
  });
}

/** POST /search/ask — grounded RAG answer. */
export function searchAsk(
  question: string,
  signal?: AbortSignal,
): Promise<RagAnswerDto> {
  return apiRequest<RagAnswerDto>({
    method: "POST",
    path: "/search/ask",
    body: { question },
    signal,
  });
}

/** GET /search/manage/knowledge — AdminOnly. */
export function fetchKnowledgeDashboard(
  token: string,
  sourceType?: string,
  signal?: AbortSignal,
): Promise<KnowledgeDashboardDto> {
  return apiRequest<KnowledgeDashboardDto>({
    path: "/search/manage/knowledge",
    token,
    query: sourceType ? { sourceType } : undefined,
    signal,
  });
}

/** GET /search/manage/related — AdminOnly related knowledge. */
export function fetchRelatedKnowledge(
  token: string,
  sourceType: string,
  sourceId: string,
  take = 6,
  signal?: AbortSignal,
): Promise<SearchContextDto> {
  return apiRequest<SearchContextDto>({
    path: "/search/manage/related",
    token,
    query: { sourceType, sourceId, take },
    signal,
  });
}
