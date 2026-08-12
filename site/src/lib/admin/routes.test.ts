import { describe, expect, it } from "vitest";
import {
  ADMIN_ROUTES,
  buildAdminLoginUrl,
  isAdminPath,
  isSafeAdminReturnUrl,
} from "./routes";

describe("isAdminPath", () => {
  it("recognizes admin paths", () => {
    expect(isAdminPath("/admin")).toBe(true);
    expect(isAdminPath("/admin/content")).toBe(true);
  });

  it("rejects non-admin paths", () => {
    expect(isAdminPath("/profile")).toBe(false);
    expect(isAdminPath("/administrator")).toBe(false);
  });
});

describe("isSafeAdminReturnUrl", () => {
  it("accepts in-app admin paths", () => {
    expect(isSafeAdminReturnUrl("/admin/users")).toBe(true);
  });

  it("rejects open-redirect / injection attempts", () => {
    expect(isSafeAdminReturnUrl("//evil.com")).toBe(false);
    expect(isSafeAdminReturnUrl("https://evil.com/admin")).toBe(false);
    expect(isSafeAdminReturnUrl("/profile")).toBe(false);
    expect(isSafeAdminReturnUrl("/admin\\..\\x")).toBe(false);
    expect(isSafeAdminReturnUrl(null)).toBe(false);
  });
});

describe("buildAdminLoginUrl", () => {
  it("carries a safe return url", () => {
    expect(buildAdminLoginUrl("/admin/users")).toBe(
      "/?next=%2Fadmin%2Fusers",
    );
  });

  it("falls back to the dashboard for unsafe return urls", () => {
    expect(buildAdminLoginUrl("https://evil.com")).toBe(
      `/?next=${encodeURIComponent(ADMIN_ROUTES.dashboard)}`,
    );
  });
});
