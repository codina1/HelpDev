import { describe, expect, it } from "vitest";
import {
  getAdminEnvironmentMeta,
  resolveAdminEnvironment,
} from "./environment";

describe("resolveAdminEnvironment", () => {
  it("maps production values", () => {
    expect(resolveAdminEnvironment("production")).toBe("production");
    expect(resolveAdminEnvironment("Production")).toBe("production");
  });

  it("maps staging values", () => {
    expect(resolveAdminEnvironment("staging")).toBe("staging");
  });

  it("defaults to development for anything else", () => {
    expect(resolveAdminEnvironment("development")).toBe("development");
    expect(resolveAdminEnvironment("")).toBe("development");
    expect(resolveAdminEnvironment(undefined)).toBe("development");
  });
});

describe("getAdminEnvironmentMeta", () => {
  it("uses a high-attention tone for production", () => {
    expect(getAdminEnvironmentMeta("production").tone).toBe("danger");
  });

  it("uses a warning tone for staging", () => {
    expect(getAdminEnvironmentMeta("staging").tone).toBe("warning");
  });
});
