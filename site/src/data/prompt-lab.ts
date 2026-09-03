export const PROMPT_LAB_HERO_IMAGE_SRC = "/prompt-lab/hero-flask.png";

export const PROMPT_LAB_HERO_EYEBROW = "آزمایشگاه پرامپت";
export const PROMPT_LAB_HERO_TITLE = "Prompt Lab";
export const PROMPT_LAB_HERO_SUBTITLE =
  "مجموعه‌ای از بهترین پرامپت‌ها برای توسعه‌دهندگان، ابزارهای AI و ساخت محصول حرفه‌ای.";

export type PromptLabQuickFilterId =
  | "all"
  | "chatgpt"
  | "claude"
  | "gemini"
  | "copilot"
  | "code"
  | "design"
  | "devops"
  | "content"
  | "data"
  | "other";

export type PromptLabQuickFilter = {
  id: PromptLabQuickFilterId;
  label: string;
  icon: string;
  /** Maps to API `category` when set. */
  category?: string;
  /** Maps to API `aiModel` when set. */
  aiModel?: string;
};

export const PROMPT_LAB_QUICK_FILTERS: readonly PromptLabQuickFilter[] = [
  { id: "all", label: "همه", icon: "all" },
  { id: "chatgpt", label: "ChatGPT", icon: "chatgpt", aiModel: "chatgpt" },
  { id: "claude", label: "Claude", icon: "claude", aiModel: "claude" },
  { id: "gemini", label: "Gemini", icon: "gemini", aiModel: "gemini" },
  { id: "copilot", label: "Copilot", icon: "copilot", aiModel: "copilot" },
  { id: "code", label: "Code", icon: "code", category: "coding" },
  { id: "design", label: "طراحی", icon: "design", category: "design" },
  { id: "devops", label: "DevOps", icon: "devops", category: "video" },
  { id: "content", label: "تولید محتوا", icon: "content", category: "writing" },
  { id: "data", label: "تولید داده", icon: "data", category: "education" },
  { id: "other", label: "دیگر", icon: "other", category: "marketing" },
] as const;

export type PromptLabSidebarCategory = {
  id: string;
  label: string;
  slug: string;
  count: number;
};

export const PROMPT_LAB_SIDEBAR_CATEGORIES: readonly PromptLabSidebarCategory[] = [
  { id: "coding", label: "Code", slug: "coding", count: 68 },
  { id: "design", label: "طراحی", slug: "design", count: 42 },
  { id: "devops", label: "DevOps", slug: "video", count: 31 },
  { id: "writing", label: "تولید محتوا", slug: "writing", count: 28 },
  { id: "data", label: "تولید داده", slug: "education", count: 19 },
  { id: "other", label: "دیگر", slug: "marketing", count: 12 },
] as const;

export type PromptLabSidebarModel = {
  id: string;
  label: string;
  slug: string;
};

export const PROMPT_LAB_SIDEBAR_MODELS: readonly PromptLabSidebarModel[] = [
  { id: "chatgpt", label: "ChatGPT", slug: "chatgpt" },
  { id: "claude", label: "Claude", slug: "claude" },
  { id: "gemini", label: "Gemini", slug: "gemini" },
  { id: "copilot", label: "Copilot", slug: "copilot" },
  { id: "midjourney", label: "Midjourney", slug: "midjourney" },
] as const;

export type PromptLabLevelId = "all" | "beginner" | "intermediate" | "advanced";

export const PROMPT_LAB_LEVELS: readonly { id: PromptLabLevelId; label: string }[] = [
  { id: "all", label: "همه سطوح" },
  { id: "beginner", label: "مبتدی" },
  { id: "intermediate", label: "متوسط" },
  { id: "advanced", label: "پیشرفته" },
] as const;

export type PromptLabSortId = "newest" | "popular" | "views";

export const PROMPT_LAB_SORT_OPTIONS: readonly { id: PromptLabSortId; label: string }[] = [
  { id: "newest", label: "جدیدترین" },
  { id: "popular", label: "محبوب‌ترین" },
  { id: "views", label: "پربازدیدترین" },
] as const;

/** Display total used on the catalog header when unfiltered (reference). */
export const PROMPT_LAB_DISPLAY_TOTAL = 234;
