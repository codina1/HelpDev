/**
 * Frontend API base configuration.
 *
 * The canonical contract is a full base URL that already includes the versioned
 * `/api/v1` segment, e.g. `https://api.example.com/api/v1`. Provide it through
 * `NEXT_PUBLIC_HELPDEV_API_BASE_URL`. This value is public (it ships to the
 * browser) and must never contain secrets.
 *
 * For local development we fall back to deriving the canonical base from the
 * legacy `NEXT_PUBLIC_API_URL` origin so existing setups keep working.
 */

const CANONICAL_SUFFIX = "/api/v1";
const DEV_FALLBACK_ORIGIN = "http://localhost:5221";

function stripTrailingSlashes(value: string): string {
  return value.replace(/\/+$/, "");
}

function resolveApiBaseUrl(): string {
  const explicit = process.env.NEXT_PUBLIC_HELPDEV_API_BASE_URL?.trim();
  if (explicit) {
    return stripTrailingSlashes(explicit);
  }

  const legacyOrigin = process.env.NEXT_PUBLIC_API_URL?.trim() || DEV_FALLBACK_ORIGIN;
  return `${stripTrailingSlashes(legacyOrigin)}${CANONICAL_SUFFIX}`;
}

export const API_BASE_URL = resolveApiBaseUrl();

/**
 * Validates the configured API base URL. Throws a descriptive error when the
 * value is unusable. In Production the URL must be HTTPS and must target the
 * canonical `/api/v1` route. Intended to run at startup/build.
 */
export function assertValidApiBaseUrl(
  baseUrl: string = API_BASE_URL,
  isProduction: boolean = process.env.NODE_ENV === "production",
): void {
  let parsed: URL;
  try {
    parsed = new URL(baseUrl);
  } catch {
    throw new Error(
      `Invalid NEXT_PUBLIC_HELPDEV_API_BASE_URL: "${baseUrl}" is not a valid absolute URL.`,
    );
  }

  if (!parsed.pathname.replace(/\/+$/, "").endsWith(CANONICAL_SUFFIX)) {
    throw new Error(
      `API base URL must target the canonical "${CANONICAL_SUFFIX}" route. Received path "${parsed.pathname}".`,
    );
  }

  if (isProduction && parsed.protocol !== "https:") {
    throw new Error("API base URL must use HTTPS in Production.");
  }
}
