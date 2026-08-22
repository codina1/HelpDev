import { afterEach, describe, expect, it } from "vitest";
import {
  CONTENT_DRAFT_STORAGE_KEY,
  clearDraft,
  loadDraft,
  saveDraft,
} from "./editor-draft";

afterEach(() => {
  window.localStorage.clear();
});

describe("editor draft recovery", () => {
  it("saves and recovers a draft for the same content id", () => {
    saveDraft({ contentId: "c1", title: "T", body: "B", excerpt: "E" });
    const draft = loadDraft("c1");
    expect(draft).not.toBeNull();
    expect(draft?.title).toBe("T");
    expect(draft?.body).toBe("B");
    expect(draft?.excerpt).toBe("E");
    expect(typeof draft?.timestamp).toBe("number");
  });

  it("recovers optional block JSON without wiping a legacy draft", () => {
    saveDraft({
      contentId: "c1",
      title: "T",
      body: "B",
      excerpt: "E",
      contentJson: '{"type":"doc","content":[]}',
    });
    const draft = loadDraft("c1");
    expect(draft?.contentJson).toContain('"type":"doc"');
  });

  it("does not return a draft for a different content id", () => {
    saveDraft({ contentId: "c1", title: "T", body: "B", excerpt: "E" });
    expect(loadDraft("other")).toBeNull();
  });

  it("clears a draft", () => {
    saveDraft({ contentId: "c1", title: "T", body: "B", excerpt: "E" });
    clearDraft();
    expect(loadDraft("c1")).toBeNull();
  });

  it("handles malformed storage gracefully and clears it", () => {
    window.localStorage.setItem(CONTENT_DRAFT_STORAGE_KEY, "{not valid json");
    expect(loadDraft("c1")).toBeNull();
    expect(window.localStorage.getItem(CONTENT_DRAFT_STORAGE_KEY)).toBeNull();
  });

  it("rejects structurally invalid drafts", () => {
    window.localStorage.setItem(
      CONTENT_DRAFT_STORAGE_KEY,
      JSON.stringify({ contentId: "c1", title: 5 }),
    );
    expect(loadDraft("c1")).toBeNull();
  });
});

describe("draft storage security", () => {
  it("stores only the whitelisted, non-sensitive fields", () => {
    saveDraft({ contentId: "c1", title: "T", body: "B", excerpt: "E" });
    const raw = window.localStorage.getItem(CONTENT_DRAFT_STORAGE_KEY) ?? "";
    const parsed = JSON.parse(raw) as Record<string, unknown>;
    expect(Object.keys(parsed).sort()).toEqual(
      ["body", "contentId", "excerpt", "timestamp", "title"].sort(),
    );
  });

  it("never persists tokens, JWTs or user identity", () => {
    saveDraft({ contentId: "c1", title: "T", body: "B", excerpt: "E" });
    const raw = (window.localStorage.getItem(CONTENT_DRAFT_STORAGE_KEY) ?? "").toLowerCase();
    expect(raw).not.toContain("token");
    expect(raw).not.toContain("jwt");
    expect(raw).not.toContain("bearer");
    expect(raw).not.toContain("authorization");
    expect(raw).not.toContain("userid");
    expect(raw).not.toContain("email");
  });
});
