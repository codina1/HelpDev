/**
 * Standard frontend API error. Mirrors the backend error contract
 * `{ "message": "...", "code": "..." }` and augments it with transport metadata
 * that is safe to surface in the UI.
 *
 * Never carries stack traces or raw backend exception details.
 */

export type ApiErrorBody = {
  message?: string;
  code?: string;
  // Some legacy/framework responses use `title` instead of `message`.
  title?: string;
};

export const NETWORK_ERROR_STATUS = 0;

export type ApiClientErrorInit = {
  message: string;
  code?: string | null;
  status: number;
  correlationId?: string | null;
  retryAfterSeconds?: number | null;
};

export class ApiClientError extends Error {
  readonly code: string | null;
  readonly status: number;
  readonly correlationId: string | null;
  readonly retryAfterSeconds: number | null;

  constructor(init: ApiClientErrorInit) {
    super(init.message);
    this.name = "ApiClientError";
    this.code = init.code ?? null;
    this.status = init.status;
    this.correlationId = init.correlationId ?? null;
    this.retryAfterSeconds = init.retryAfterSeconds ?? null;

    // Preserve prototype chain for `instanceof` after transpilation.
    Object.setPrototypeOf(this, ApiClientError.prototype);
  }

  get isNetworkError(): boolean {
    return this.status === NETWORK_ERROR_STATUS;
  }

  get isUnauthorized(): boolean {
    return this.status === 401;
  }

  get isForbidden(): boolean {
    return this.status === 403;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }

  get isConflict(): boolean {
    return this.status === 409;
  }

  get isPayloadTooLarge(): boolean {
    return this.status === 413;
  }

  get isRateLimited(): boolean {
    return this.status === 429;
  }

  get isServerError(): boolean {
    return this.status >= 500;
  }
}

export function isApiClientError(value: unknown): value is ApiClientError {
  return value instanceof ApiClientError;
}
