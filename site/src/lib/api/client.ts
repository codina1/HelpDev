/**
 * Centralized, typed HTTP client for the HelpDev API.
 *
 * Responsibilities:
 *   - Canonical `/api/v1` base URL (from config).
 *   - JSON content negotiation.
 *   - Bearer token injection (only when a token is provided).
 *   - X-Correlation-ID generation and response capture.
 *   - Bounded timeout with AbortSignal support.
 *   - Standard error mapping to {@link ApiClientError}.
 *
 * Privacy: this module never logs tokens, request bodies, query strings, or
 * response payloads. Do not add such logging.
 */

import { API_BASE_URL } from "@/lib/config";
import {
  CORRELATION_ID_HEADER,
  generateCorrelationId,
  normalizeCorrelationId,
} from "./correlation";
import { ApiClientError, NETWORK_ERROR_STATUS, type ApiErrorBody } from "./errors";

export const DEFAULT_TIMEOUT_MS = 20_000;

export type ApiRequestOptions = {
  path: string;
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  token?: string | null;
  query?: Record<string, string | number | boolean | null | undefined>;
  signal?: AbortSignal;
  timeoutMs?: number;
  correlationId?: string;
  cache?: RequestCache;
  headers?: Record<string, string>;
};

export function buildRequestUrl(
  path: string,
  query?: ApiRequestOptions["query"],
): string {
  if (!path.startsWith("/")) {
    throw new Error(`API path must start with "/": "${path}"`);
  }

  const url = `${API_BASE_URL}${path}`;
  if (!query) {
    return url;
  }

  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null) {
      params.append(key, String(value));
    }
  }

  const queryString = params.toString();
  return queryString ? `${url}?${queryString}` : url;
}

export function parseRetryAfter(header: string | null): number | null {
  if (!header) {
    return null;
  }

  const asSeconds = Number(header);
  if (Number.isFinite(asSeconds) && asSeconds >= 0) {
    return Math.round(asSeconds);
  }

  const asDate = Date.parse(header);
  if (!Number.isNaN(asDate)) {
    const deltaSeconds = Math.round((asDate - Date.now()) / 1000);
    return deltaSeconds > 0 ? deltaSeconds : 0;
  }

  return null;
}

const SAFE_STATUS_MESSAGES: Record<number, string> = {
  400: "درخواست نامعتبر است.",
  401: "برای ادامه وارد حساب کاربری شوید.",
  403: "شما به این بخش دسترسی ندارید.",
  404: "موردی یافت نشد.",
  409: "این عملیات با وضعیت فعلی تداخل دارد.",
  413: "حجم درخواست بیش از حد مجاز است.",
  429: "تعداد درخواست‌ها زیاد است. کمی بعد دوباره تلاش کنید.",
  500: "خطای سرور. لطفاً بعداً تلاش کنید.",
  503: "سرویس موقتاً در دسترس نیست.",
};

function safeUserMessage(status: number, parsed: ApiErrorBody | null): string {
  const provided = parsed?.message ?? parsed?.title;
  if (provided && provided.trim().length > 0) {
    return provided;
  }

  if (SAFE_STATUS_MESSAGES[status]) {
    return SAFE_STATUS_MESSAGES[status];
  }

  return status >= 500 ? "خطای سرور. لطفاً بعداً تلاش کنید." : "خطایی رخ داد.";
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException
    ? error.name === "AbortError"
    : error instanceof Error && error.name === "AbortError";
}

async function safeParseErrorBody(response: Response): Promise<ApiErrorBody | null> {
  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return null;
  }

  try {
    return (await response.json()) as ApiErrorBody;
  } catch {
    // Malformed error body — fall back to a safe status message.
    return null;
  }
}

async function parseSuccessBody<T>(response: Response): Promise<T> {
  if (response.status === 204 || response.status === 205) {
    return undefined as T;
  }

  const contentType = response.headers.get("content-type") ?? "";
  if (!contentType.includes("application/json")) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export async function apiRequest<T>(options: ApiRequestOptions): Promise<T> {
  const correlationId = options.correlationId
    ? normalizeCorrelationId(options.correlationId)
    : generateCorrelationId();

  const hasBody = options.body !== undefined;
  // Multipart uploads (e.g. media): the body is FormData and the browser must
  // set its own `Content-Type` (with the multipart boundary) — never set it
  // manually, or the server will fail to parse the multipart payload.
  const isFormData = typeof FormData !== "undefined" && options.body instanceof FormData;

  const headers: Record<string, string> = {
    Accept: "application/json",
    [CORRELATION_ID_HEADER]: correlationId,
    ...options.headers,
  };

  if (hasBody && !isFormData) {
    headers["Content-Type"] = "application/json";
  }

  if (options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }

  // Own timeout controller unless the caller supplies a signal.
  const timeoutMs = options.timeoutMs ?? DEFAULT_TIMEOUT_MS;
  const timeoutController = options.signal ? null : new AbortController();
  const signal = options.signal ?? timeoutController?.signal;
  const timeoutHandle =
    timeoutController != null
      ? setTimeout(() => timeoutController.abort(), timeoutMs)
      : null;

  let response: Response;
  try {
    response = await fetch(buildRequestUrl(options.path, options.query), {
      method: options.method ?? "GET",
      headers,
      body: hasBody ? (isFormData ? (options.body as FormData) : JSON.stringify(options.body)) : undefined,
      signal,
      cache: options.cache,
    });
  } catch (error) {
    if (isAbortError(error)) {
      throw new ApiClientError({
        message: "درخواست لغو یا زمان‌بندی آن منقضی شد.",
        code: "request_aborted",
        status: NETWORK_ERROR_STATUS,
        correlationId,
      });
    }

    throw new ApiClientError({
      message: "اتصال به سرور برقرار نشد.",
      code: "network_error",
      status: NETWORK_ERROR_STATUS,
      correlationId,
    });
  } finally {
    if (timeoutHandle != null) {
      clearTimeout(timeoutHandle);
    }
  }

  const responseCorrelationId =
    response.headers.get(CORRELATION_ID_HEADER) ?? correlationId;

  if (!response.ok) {
    const parsed = await safeParseErrorBody(response);
    throw new ApiClientError({
      message: safeUserMessage(response.status, parsed),
      code: parsed?.code ?? null,
      status: response.status,
      correlationId: responseCorrelationId,
      retryAfterSeconds: parseRetryAfter(response.headers.get("Retry-After")),
    });
  }

  return parseSuccessBody<T>(response);
}
