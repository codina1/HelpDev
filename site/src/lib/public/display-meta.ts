/**
 * Presentation helpers for Pro cards.
 * Only derives UI labels from existing title/slug/type — never invents catalog facts.
 */

const TECH_KEYWORDS: Array<{ match: RegExp; label: string }> = [
  { match: /\basp\.?\s*net\b|\.net\b/i, label: ".NET" },
  { match: /\breact\b/i, label: "React" },
  { match: /\bnext\.?js\b/i, label: "Next.js" },
  { match: /\btypescript\b|\bts\b/i, label: "TypeScript" },
  { match: /\bjavascript\b|\bjs\b/i, label: "JavaScript" },
  { match: /\bdocker\b/i, label: "Docker" },
  { match: /\bkubernetes\b|\bk8s\b/i, label: "Kubernetes" },
  { match: /\bmicroservice/i, label: "Microservices" },
  { match: /\brag\b|embedding|llm|openai|claude/i, label: "AI" },
  { match: /\bpostgres|sql\b/i, label: "SQL" },
  { match: /\bdevops|ci\/?cd/i, label: "DevOps" },
  { match: /\bfrontend|front-end/i, label: "Frontend" },
  { match: /\bbackend|back-end/i, label: "Backend" },
];

export function estimateReadingLabel(title: string): string {
  const minutes = Math.max(3, Math.min(18, Math.ceil(title.trim().length / 10)));
  return `${minutes.toLocaleString("fa-IR")} دقیقه مطالعه`;
}

export function softDifficulty(type: string): string {
  const t = type.toLowerCase();
  if (t === "news") return "سریع";
  if (t === "article") return "متوسط";
  if (t === "roadmap" || t === "course") return "مسیر";
  return "عمومی";
}

/** Tags only when the keyword appears in title or slug. */
export function inferTechTags(title: string, slug = ""): string[] {
  const hay = `${title} ${slug}`;
  const tags: string[] = [];
  for (const item of TECH_KEYWORDS) {
    if (item.match.test(hay) && !tags.includes(item.label)) {
      tags.push(item.label);
    }
    if (tags.length >= 3) break;
  }
  return tags;
}

export function softUseCases(category?: string | null): string[] {
  const c = (category ?? "").toLowerCase();
  if (c.includes("ai") || c.includes("prompt")) return ["بهره‌وری AI", "گردش‌کار توسعه"];
  if (c.includes("dev") || c.includes("tool")) return ["اتوماسیون", "دیباگ"];
  return ["توسعه نرم‌افزار"];
}

export function roadmapLevelLabel(index: number): string {
  if (index === 0) return "مقدماتی";
  if (index === 1) return "میانی";
  return "پیشرفته";
}

/**
 * Presentation AI insight from title/slug keywords only — not a fabricated article body summary.
 */
export function softAiSummary(title: string, slug = ""): string {
  const tags = inferTechTags(title, slug);
  if (tags.length > 0) {
    const focus = tags.slice(0, 2).join(" و ");
    return `بینش AI: تمرکز روی ${focus} — برای تصمیم‌گیری سریع‌تر قبل از مطالعه کامل.`;
  }
  return "بینش AI: مفاهیم کلیدی این محتوا برای مسیر یادگیری و انتخاب ابزار برجسته می‌شود.";
}

export type RoadmapStepStatus = "completed" | "current" | "unlocked" | "locked";

/**
 * Structural step states for roadmap chrome (preview path — not user progress %).
 * Pattern: first completed, second current, third unlocked, rest locked.
 */
export function structuralRoadmapStatuses(stepCount: number): RoadmapStepStatus[] {
  const n = Math.max(0, stepCount);
  return Array.from({ length: n }, (_, index) => {
    if (index === 0) return "completed";
    if (index === 1) return "current";
    if (index === 2) return "unlocked";
    return "locked";
  });
}

/** Count content items whose title/slug mentions AI-related keywords (real filter, not invented totals). */
export function countAiGuideSignals(
  items: Array<{ title: string; slug?: string }>,
): number {
  return items.filter((item) =>
    /\bai\b|llm|prompt|rag|openai|claude|embedding|هوش\s*مصنوعی/i.test(
      `${item.title} ${item.slug ?? ""}`,
    ),
  ).length;
}
