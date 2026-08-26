/** Local homepage illustrations — never remote stock or invented covers. */

export const HOME_COVERS = {
  hero: "/home/hero-workspace.webp",
  newsletter: "/home/newsletter.svg",
  architecture: "/home/icon-dotnet.png",
  ai: "/home/icon-ai.png",
  backend: "/home/icon-backend.png",
  frontend: "/home/icon-frontend.png",
  devops: "/home/icon-devops.png",
  article: "/home/icon-learning.png",
  news: "/home/icon-news.png",
  tools: "/home/icon-tools.png",
  prompt: "/home/icon-prompt.png",
  roadmap: "/home/icon-roadmap.png",
} as const;

export function coverForHomeCategory(category: string): string {
  if (category === "معماری" || category === ".NET") return HOME_COVERS.architecture;
  if (category === "هوش مصنوعی" || category === "AI Coding" || category === "MCP") return HOME_COVERS.ai;
  if (category === "بک‌اند") return HOME_COVERS.backend;
  if (category === "فرانت‌اند") return HOME_COVERS.frontend;
  if (category === "دواپس" || category === "Tools") return HOME_COVERS.devops;
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
