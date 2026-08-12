import { describe, expect, it } from "vitest";
import { assertValidApiBaseUrl } from "./config";

describe("assertValidApiBaseUrl", () => {
  it("accepts a canonical HTTPS base URL in Production", () => {
    expect(() =>
      assertValidApiBaseUrl("https://api.example.com/api/v1", true),
    ).not.toThrow();
  });

  it("accepts a trailing slash", () => {
    expect(() =>
      assertValidApiBaseUrl("https://api.example.com/api/v1/", true),
    ).not.toThrow();
  });

  it("rejects a non-URL value", () => {
    expect(() => assertValidApiBaseUrl("not a url", false)).toThrow(/not a valid/);
  });

  it("rejects an unversioned base URL", () => {
    expect(() => assertValidApiBaseUrl("https://api.example.com/api", true)).toThrow(
      /canonical/,
    );
  });

  it("rejects HTTP in Production", () => {
    expect(() => assertValidApiBaseUrl("http://api.example.com/api/v1", true)).toThrow(
      /HTTPS/,
    );
  });

  it("allows HTTP outside Production (local development)", () => {
    expect(() =>
      assertValidApiBaseUrl("http://localhost:5221/api/v1", false),
    ).not.toThrow();
  });
});
