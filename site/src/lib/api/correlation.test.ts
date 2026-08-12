import { describe, expect, it } from "vitest";
import {
  CORRELATION_ID_MAX_LENGTH,
  generateCorrelationId,
  normalizeCorrelationId,
} from "./correlation";

describe("correlation", () => {
  it("generates a valid, non-empty id using permitted characters", () => {
    const id = generateCorrelationId();
    expect(id.length).toBeGreaterThan(0);
    expect(id.length).toBeLessThanOrEqual(CORRELATION_ID_MAX_LENGTH);
    expect(id).toMatch(/^[A-Za-z0-9._-]+$/);
  });

  it("strips disallowed characters and truncates to the max length", () => {
    const dirty = `abc/def ghi<script>${"x".repeat(200)}`;
    const normalized = normalizeCorrelationId(dirty);
    expect(normalized).toMatch(/^[A-Za-z0-9._-]+$/);
    expect(normalized.length).toBeLessThanOrEqual(CORRELATION_ID_MAX_LENGTH);
  });

  it("falls back to a generated id when the input has no permitted characters", () => {
    const normalized = normalizeCorrelationId("///   ///");
    expect(normalized.length).toBeGreaterThan(0);
    expect(normalized).toMatch(/^[A-Za-z0-9._-]+$/);
  });
});
