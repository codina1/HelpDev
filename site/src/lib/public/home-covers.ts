/** Local homepage illustrations — never remote stock or invented covers. */

export const HOME_COVERS = {
  hero: "/home/hero-scene.svg",
  newsletter: "/home/newsletter.svg",
  architecture: "/home/cover-architecture.svg",
  ai: "/home/cover-ai.svg",
  backend: "/home/cover-backend.svg",
  frontend: "/home/cover-frontend.svg",
  devops: "/home/cover-devops.svg",
  article: "/home/cover-article.svg",
} as const;

export function coverForHomeCategory(category: string): string {
  if (category === "معماری") return HOME_COVERS.architecture;
  if (category === "هوش مصنوعی") return HOME_COVERS.ai;
  if (category === "بک‌اند") return HOME_COVERS.backend;
  if (category === "فرانت‌اند") return HOME_COVERS.frontend;
  if (category === "دواپس") return HOME_COVERS.devops;
  return HOME_COVERS.article;
}

export function coverForHomePath(visual: string): string {
  if (visual === "architect") return HOME_COVERS.architecture;
  if (visual === "frontend") return HOME_COVERS.frontend;
  if (visual === "devops") return HOME_COVERS.devops;
  if (visual === "ai") return HOME_COVERS.ai;
  if (visual === "backend") return HOME_COVERS.backend;
  return HOME_COVERS.article;
}

export function coverForHomeValue(id: string): string {
  if (id === "paths") return HOME_COVERS.architecture;
  if (id === "tools") return HOME_COVERS.devops;
  if (id === "ai") return HOME_COVERS.ai;
  return HOME_COVERS.article;
}
