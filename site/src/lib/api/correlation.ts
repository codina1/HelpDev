/**
 * Correlation ID helpers. Correlation IDs are used to link a frontend request
 * to backend logs for support/debugging only. They are never used for
 * authentication and are not persisted indefinitely.
 */

export const CORRELATION_ID_HEADER = "X-Correlation-ID";

export const CORRELATION_ID_MAX_LENGTH = 100;

// Permitted characters for the correlation header value.
const DISALLOWED_CHARACTERS = /[^A-Za-z0-9._-]/g;

/**
 * Generates a valid correlation ID, preferring the browser crypto UUID and
 * normalizing it to the permitted character set and length.
 */
export function generateCorrelationId(): string {
  const candidate =
    typeof globalThis.crypto?.randomUUID === "function"
      ? globalThis.crypto.randomUUID()
      : fallbackCorrelationId();

  return normalizeCorrelationId(candidate);
}

/**
 * Normalizes an arbitrary string into a safe correlation ID: allowed characters
 * only, at most {@link CORRELATION_ID_MAX_LENGTH} characters, never empty.
 */
export function normalizeCorrelationId(value: string): string {
  const cleaned = value
    .replace(DISALLOWED_CHARACTERS, "")
    .slice(0, CORRELATION_ID_MAX_LENGTH);

  return cleaned.length > 0 ? cleaned : fallbackCorrelationId();
}

function fallbackCorrelationId(): string {
  const random = Math.random().toString(36).slice(2, 12);
  return `cid-${Date.now().toString(36)}-${random}`.slice(0, CORRELATION_ID_MAX_LENGTH);
}
