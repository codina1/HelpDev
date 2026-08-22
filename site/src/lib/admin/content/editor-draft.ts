/**
 * Local-only draft recovery for the Content Studio.
 *
 * Stores a MINIMAL snapshot in localStorage so an author can recover unsaved
 * edits after an accidental reload/navigation. It intentionally stores NO
 * secrets: no auth token, no JWT, no user identity — only the content id and
 * the text being edited. This is not server autosave; the server is never
 * contacted from here.
 */

export const CONTENT_DRAFT_STORAGE_KEY = "helpdev.content.editor.draft.v1";

export type EditorDraft = {
  contentId: string;
  title: string;
  body: string;
  excerpt: string;
  timestamp: number;
  /** Optional TipTap JSON for the article block editor. Absent on older drafts. */
  contentJson?: string;
};

function isBrowser(): boolean {
  return typeof window !== "undefined" && typeof window.localStorage !== "undefined";
}

function isValidDraft(value: unknown): value is EditorDraft {
  if (typeof value !== "object" || value === null) return false;
  const draft = value as Record<string, unknown>;
  const contentJsonOk =
    draft.contentJson === undefined || typeof draft.contentJson === "string";
  return (
    typeof draft.contentId === "string" &&
    typeof draft.title === "string" &&
    typeof draft.body === "string" &&
    typeof draft.excerpt === "string" &&
    typeof draft.timestamp === "number" &&
    Number.isFinite(draft.timestamp) &&
    contentJsonOk
  );
}

/** Persists a minimal draft snapshot. Failures (quota/SSR) are swallowed. */
export function saveDraft(draft: Omit<EditorDraft, "timestamp">): void {
  if (!isBrowser()) return;
  try {
    const payload: EditorDraft = {
      contentId: draft.contentId,
      title: draft.title,
      body: draft.body,
      excerpt: draft.excerpt,
      timestamp: Date.now(),
    };
    if (typeof draft.contentJson === "string") {
      payload.contentJson = draft.contentJson;
    }
    window.localStorage.setItem(CONTENT_DRAFT_STORAGE_KEY, JSON.stringify(payload));
  } catch {
    // Ignore write failures (private mode / quota exceeded).
  }
}

/**
 * Reads the stored draft for a given content id. Returns null when absent,
 * malformed, or belonging to a different content item. Malformed entries are
 * cleared so a corrupt value cannot wedge the editor.
 */
export function loadDraft(contentId: string): EditorDraft | null {
  if (!isBrowser()) return null;
  try {
    const raw = window.localStorage.getItem(CONTENT_DRAFT_STORAGE_KEY);
    if (!raw) return null;
    const parsed: unknown = JSON.parse(raw);
    if (!isValidDraft(parsed)) {
      window.localStorage.removeItem(CONTENT_DRAFT_STORAGE_KEY);
      return null;
    }
    if (parsed.contentId !== contentId) return null;
    return parsed;
  } catch {
    try {
      window.localStorage.removeItem(CONTENT_DRAFT_STORAGE_KEY);
    } catch {
      // Ignore.
    }
    return null;
  }
}

/** Removes the stored draft (e.g. after a successful save or discard). */
export function clearDraft(): void {
  if (!isBrowser()) return;
  try {
    window.localStorage.removeItem(CONTENT_DRAFT_STORAGE_KEY);
  } catch {
    // Ignore.
  }
}
