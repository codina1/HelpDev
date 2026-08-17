import { HOME_COVERS } from "@/lib/public/home-covers";

export const PROMPT_LAB_CATEGORY_SLUGS = [
  "image",
  "video",
  "coding",
  "writing",
  "design",
  "marketing",
  "education",
] as const;

export type PromptLabCategorySlug = (typeof PROMPT_LAB_CATEGORY_SLUGS)[number];

export function coverForPromptLabCategory(slug: PromptLabCategorySlug): string {
  if (slug === "image" || slug === "design") return HOME_COVERS.frontend;
  if (slug === "video") return HOME_COVERS.devops;
  if (slug === "coding") return HOME_COVERS.architecture;
  if (slug === "writing") return HOME_COVERS.article;
  if (slug === "marketing") return HOME_COVERS.ai;
  return HOME_COVERS.backend;
}
