/** Public Prompt Lab homepage — never an admin path. */
export const PUBLIC_PROMPT_LAB_PATH = "/prompt-lab";

export const PROMPT_LAB_HERO_TITLE = "Prompt Lab";
export const PROMPT_LAB_HERO_SUBTITLE =
  "مجموعه‌ای از پرامپت‌های حرفه‌ای و تست‌شده برای ساخت، طراحی و توسعه با هوش مصنوعی";

export function publicPromptLabDetailPath(slug: string): string {
  return `${PUBLIC_PROMPT_LAB_PATH}/${encodeURIComponent(slug)}`;
}

export const PUBLIC_PROMPT_LAB_PACKS_PATH = `${PUBLIC_PROMPT_LAB_PATH}/packs`;

export function publicPromptLabPackPath(slug: string): string {
  return `${PUBLIC_PROMPT_LAB_PACKS_PATH}/${encodeURIComponent(slug)}`;
}
