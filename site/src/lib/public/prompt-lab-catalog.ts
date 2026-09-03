import { listPrompts, type PublicPromptFilter, type PublicPromptPageDto } from "@/lib/api/promptlab";
import { toPromptLabCardItem } from "@/lib/public/prompt-lab-mappers";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";

export type PromptLabCatalogPage = {
  items: PromptLabCardItem[];
  page: number;
  pageSize: number;
  total: number;
};

export const PROMPT_LAB_PAGE_SIZE = 8;
export const PROMPT_LAB_TEASER_SIZE = 4;

export const EMPTY_PROMPT_LAB_CATALOG_PAGE: PromptLabCatalogPage = {
  items: [],
  page: 1,
  pageSize: PROMPT_LAB_PAGE_SIZE,
  total: 0,
};

export type PromptLabCatalogQuery = {
  search?: string;
  category?: string | null;
  aiModel?: string | null;
  page?: number;
  pageSize?: number;
  popular?: boolean;
  signal?: AbortSignal;
};

export function toPromptLabCatalogPage(dto: PublicPromptPageDto): PromptLabCatalogPage {
  return {
    items: dto.items.map(toPromptLabCardItem),
    page: dto.page,
    pageSize: dto.pageSize,
    total: dto.total,
  };
}

export function fetchPromptLabCatalog(query: PromptLabCatalogQuery = {}): Promise<PromptLabCatalogPage> {
  const filter: PublicPromptFilter = {
    search: query.search?.trim() || undefined,
    category: query.category?.trim() || undefined,
    aiModel: query.aiModel?.trim() || undefined,
    popular: query.popular ? true : undefined,
    page: query.page ?? 1,
    pageSize: query.pageSize ?? PROMPT_LAB_PAGE_SIZE,
  };
  return listPrompts(filter, query.signal).then(toPromptLabCatalogPage);
}

export function excludePromptSlug(items: readonly PromptLabCardItem[], slug: string): PromptLabCardItem[] {
  return items.filter((item) => item.slug !== slug);
}
