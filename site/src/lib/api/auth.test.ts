import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { sendOtp, verifyOtp } from "./auth";
import { ApiClientError } from "./errors";

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: new Headers({ "content-type": "application/json" }),
  });
}

async function captureError(promise: Promise<unknown>): Promise<ApiClientError> {
  try {
    await promise;
    throw new Error("Expected the request to reject, but it resolved.");
  } catch (error) {
    return error as ApiClientError;
  }
}

describe("auth api (canonical /api/v1)", () => {
  let fetchMock: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    fetchMock = vi.fn();
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("sends OTP to the canonical auth route", async () => {
    fetchMock.mockResolvedValueOnce(jsonResponse({ message: "ok", expiresInSeconds: 120 }));

    const result = await sendOtp("09120000000");

    const [url, init] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/auth\/send-otp$/);
    expect(init.method).toBe("POST");
    expect(result.expiresInSeconds).toBe(120);
  });

  it("verifies OTP and returns a session payload", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({
        accessToken: "jwt",
        expiresIn: 3600,
        user: { id: "1", mobile: "09120000000", role: "User" },
      }),
    );

    const result = await verifyOtp("09120000000", "123456");
    const [url] = fetchMock.mock.calls[0];
    expect(String(url)).toMatch(/\/api\/v1\/auth\/verify-otp$/);
    expect(result.accessToken).toBe("jwt");
    expect(result.user.role).toBe("User");
  });

  it("surfaces a validation error as ApiClientError with a preserved code", async () => {
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ message: "کد نامعتبر است", code: "invalid_otp" }, 400),
    );

    const error = await captureError(verifyOtp("09120000000", "000000"));
    expect(error).toBeInstanceOf(ApiClientError);
    expect(error.status).toBe(400);
    expect(error.code).toBe("invalid_otp");
  });
});
