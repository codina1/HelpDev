import { ADMIN_ROUTES } from "@/lib/admin/routes";
import {
  DEFAULT_WRITER_PROMPT_LIST_QUERY,
  WRITER_PROMPT_PAGE_SIZE_DEFAULT,
  WRITER_PROMPT_PAGE_SIZES,
  WRITER_PROMPT_STATUSES,
  type WriterPromptListQuery,
  type WriterPromptPageSize,
  type WriterPromptStatus,
} from "./writer-prompt-types";

const PAGE_SIZE_SET = new Set<number>(WRITER_PROMPT_PAGE_SIZES);

function parsePositiveInt(raw: string | null): number | null {
  if (raw == null || raw.trim() === "") return null;
  const n = Number.parseInt(raw, 10);
  return Number.isFinite(n) && n > 0 ? n : null;
}

function parsePageSize(raw: string | null): WriterPromptPageSize {
  const n = parsePositiveInt(raw);
  if (n != null && PAGE_SIZE_SET.has(n)) {
    return n as WriterPromptPageSize;
  }
  return WRITER_PROMPT_PAGE_SIZE_DEFAULT;
}

function parseStatus(raw: string | null): WriterPromptStatus | "all" {
  if (!raw) return "all";
  const match = WRITER_PROMPT_STATUSES.find((s) => s.toLowerCase() === raw.toLowerCase());
  return match ?? "all";
}

export function parseWriterPromptListQuery(
  params: URLSearchParams | { get(name: string): string | null } | null | undefined,
): WriterPromptListQuery {
  if (!params) return { ...DEFAULT_WRITER_PROMPT_LIST_QUERY };

  const page = parsePositiveInt(params.get("page")) ?? 1;
  const pageSize = parsePageSize(params.get("pageSize"));
  const status = parseStatus(params.get("status"));

  return { page, pageSize, status };
}

export function serializeWriterPromptListQuery(query: WriterPromptListQuery): URLSearchParams {
  const params = new URLSearchParams();

  if (query.page > 1) params.set("page", String(query.page));
  if (query.pageSize !== WRITER_PROMPT_PAGE_SIZE_DEFAULT) {
    params.set("pageSize", String(query.pageSize));
  }
  if (query.status !== "all") params.set("status", query.status);

  return params;
}

export function buildWriterPromptListHref(
  query: WriterPromptListQuery,
  basePath: string = ADMIN_ROUTES.promptLab,
): string {
  const params = serializeWriterPromptListQuery(query);
  const qs = params.toString();
  return qs ? `${basePath}?${qs}` : basePath;
}

export function mergeWriterPromptListQuery(
  current: WriterPromptListQuery,
  patch: Partial<WriterPromptListQuery>,
): WriterPromptListQuery {
  const next: WriterPromptListQuery = { ...current, ...patch };

  const filterChanged =
    (patch.status !== undefined && patch.status !== current.status) ||
    (patch.pageSize !== undefined && patch.pageSize !== current.pageSize);

  if (filterChanged && patch.page === undefined) {
    next.page = 1;
  }

  if (next.page < 1) next.page = 1;
  if (!PAGE_SIZE_SET.has(next.pageSize)) {
    next.pageSize = WRITER_PROMPT_PAGE_SIZE_DEFAULT;
  }

  return next;
}

export function isWriterPromptListFiltered(query: WriterPromptListQuery): boolean {
  return query.status !== "all";
}

export function writerPromptListQueryKey(query: WriterPromptListQuery): string {
  return `${query.page}|${query.pageSize}|${query.status}`;
}
