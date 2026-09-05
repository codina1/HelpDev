import { isPromptLabCategorySlug, type PromptLabCategorySlug } from "@/lib/public/prompt-lab-covers";
import { PROMPT_LAB_PROMPTS, type PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import { PROMPT_LAB_SAMPLE_PROMPTS } from "@/data/prompt-lab";

export type PromptLabMediaType = "Text" | "Image" | "Audio" | "Video";

export type PromptLabAuthor = {
  id: string;
  name: string;
  role: string;
  bio: string;
  initials: string;
};

export type PromptLabDetail = PromptLabCardItem & {
  author: PromptLabAuthor;
  content: string;
  tags: readonly string[];
  mediaType: string;
};

export const PROMPT_LAB_AUTHORS = {
  nima: {
    id: "nima",
    name: "نیما رضایی",
    role: "معمار پلتفرم",
    bio: "روی مرز ماژول، قرارداد دامنه و انتشار دانش در HelpDev کار می‌کند.",
    initials: "نر",
  },
  sara: {
    id: "sara",
    name: "سارا محمدی",
    role: "طراح محصول AI",
    bio: "هویت تیره پریمیوم، کارت شیشه‌ای و اتمسفر بنفش/فیروزه‌ای را نگه می‌دارد.",
    initials: "سم",
  },
  kaveh: {
    id: "kaveh",
    name: "کاوه احمدی",
    role: "نویسنده فنی",
    bio: "تصمیم‌های معماری را به بریف و درس کوتاه قابل اشتراک تبدیل می‌کند.",
    initials: "کا",
  },
} as const;

const AUTHOR_BY_CATEGORY: Record<PromptLabCategorySlug, PromptLabAuthor> = {
  coding: PROMPT_LAB_AUTHORS.nima,
  education: PROMPT_LAB_AUTHORS.nima,
  image: PROMPT_LAB_AUTHORS.sara,
  design: PROMPT_LAB_AUTHORS.sara,
  video: PROMPT_LAB_AUTHORS.sara,
  writing: PROMPT_LAB_AUTHORS.kaveh,
  marketing: PROMPT_LAB_AUTHORS.kaveh,
};

/** Map UI/sample category slugs onto the canonical Prompt Lab taxonomy. */
function normalizeCategorySlug(value: string): PromptLabCategorySlug {
  if (isPromptLabCategorySlug(value)) return value;
  const key = value.trim().toLowerCase();
  if (key === "code" || key === "devops" || key === "data") return "coding";
  if (key === "content") return "writing";
  if (key === "design") return "design";
  if (key === "other") return "education";
  return "writing";
}

function localPromptCatalog(): readonly PromptLabCardItem[] {
  const seen = new Set<string>();
  const items: PromptLabCardItem[] = [];
  for (const item of [...PROMPT_LAB_SAMPLE_PROMPTS, ...PROMPT_LAB_PROMPTS]) {
    if (seen.has(item.slug)) continue;
    seen.add(item.slug);
    items.push(item);
  }
  return items;
}

const TAGS_BY_SLUG: Record<string, readonly string[]> = {
  "system-boundary-review": ["معماری", "ماژول", "قرارداد دامنه"],
  "product-ui-atmosphere": ["هیرو", "شیشه", "اتمسفر"],
  "release-walkthrough-video": ["انتشار", "سلامت سرویس", "ویدئو"],
  "engineering-brief": ["بریف", "تصمیم", "تیم"],
  "design-token-audit": ["توکن", "رنگ", "کارت"],
  "launch-narrative": ["کمپین", "مسیر یادگیری", "روایت"],
  "outbox-lesson": ["Outbox", "رویداد", "درس"],
  "gpt-image-cover": ["کاور", "مسیر", "فرانت‌اند"],
  "rag-query-rewrite": ["RAG", "بازیابی", "پرسش"],
  "onboarding-script": ["آشنایی", "اسکریپت", "Prompt Lab"],
  "docs-outline": ["مقاله", "سرفصل", "Outbox"],
  "card-hierarchy": ["کارت", "بج", "سلسله‌مراتب"],
};

const CONTENT_BY_SLUG: Record<string, string> = {
  "system-boundary-review": `You are a staff engineer reviewing a .NET modular monolith.

Goal
Review module boundaries for the described change.

Rules
- Name the owning module first
- Flag infrastructure leaking into the domain
- Do not propose a microservices split
- Keep contracts explicit and testable

Input
{{change}}

Output
1. Owning module
2. Boundary risks
3. Suggested contract
4. Tests to add`,
  "product-ui-atmosphere": `You are HelpDev's visual director.

Goal
Describe a premium dark AI interface atmosphere. No stock photography. No copied brand systems.

Palette
- Purple #8B5CF6
- Blue #6366F1
- Cyan #06B6D4
- Background #060816

Scene
{{scene}}

Output
- Lighting
- Glass surfaces
- What to avoid`,
  "release-walkthrough-video": `You write a 40-second release walkthrough.

Product
HelpDev API binary on Liara.

Must mention
- Health probe
- Proxy
- Release channel

Tone
Calm, precise, no invented metrics.

Script beats
{{beats}}`,
  "engineering-brief": `Turn an architecture discussion into a short team brief.

Audience
Engineer + writer.

Include
- Decision
- Trade-off
- What we will not do
- Next artifact

Discussion
{{notes}}`,
  "design-token-audit": `Audit HelpDev dark premium tokens.

Check
- Glass surface
- Border strength
- Radius on cards
- Purple / blue / cyan accents

Do not introduce a new palette.

Component
{{component}}`,
  "launch-narrative": `Write a launch narrative for a role-based learning path.

Rules
- No fake numbers
- Stay in HelpDev voice
- Focus on the path, not a course catalog

Path
{{path}}`,
  "outbox-lesson": `Create a short lesson on Transactional Outbox.

Students
Backend engineers new to domain events.

Explain
- Why publish after commit
- What fails without an outbox
- One HelpDev-shaped example

Avoid
Broker-specific tutorials.`,
  "gpt-image-cover": `Generate a cover for the Frontend Engineer learning path.

Identity
HelpDev. Dark. Glass. Purple/cyan light.

Do not
- Use generic stock developer photos
- Imitate another product's mascot

Format
Wide card cover, readable at small size.`,
  "rag-query-rewrite": `Rewrite a free-form question into a retrieval query.

Corpus
Published HelpDev knowledge only.

Rules
- Keep the original intent
- Add domain terms when missing
- Do not answer the question

Question
{{question}}`,
  "onboarding-script": `30-second onboarding script for Prompt Lab.

Show
- Search
- Category chips
- Prompt card

Voice
HelpDev, not a generic AI tool ad.`,
  "docs-outline": `Outline a technical article about Transactional Outbox.

Reader
HelpDev writer + engineer.

Sections
- Problem
- Contract
- Failure modes
- How we test it

Do not pad with generic event-driven filler.`,
  "card-hierarchy": `Design the hierarchy of a glass prompt card.

Order
Title, model badge, category badge, view/copy counts.

Constraints
RTL. Dark premium. Subtle glow on hover.

Card
{{card}}`,
  "advanced-react-component": `You are a senior React developer and UI/UX expert.

Create a reusable React component based on these requirements:

- Use TypeScript
- Use Tailwind CSS
- Follow best practices
- Make responsive components
- Prefer accessible, composition-friendly APIs

Requirements
{{requirements}}`,
};

function mediaTypeFor(categorySlug: PromptLabCategorySlug): PromptLabMediaType {
  if (categorySlug === "image") return "Image";
  if (categorySlug === "video") return "Video";
  return "Text";
}

function tagsFor(item: PromptLabCardItem): readonly string[] {
  return TAGS_BY_SLUG[item.slug] ?? [item.category, item.aiModel];
}

function contentFor(item: PromptLabCardItem): string {
  return (
    CONTENT_BY_SLUG[item.slug] ??
    `You are a HelpDev specialist.\n\nTask\n${item.title}\n\nContext\n${item.description}`
  );
}

export function toPromptLabDetail(item: PromptLabCardItem): PromptLabDetail {
  const categorySlug = normalizeCategorySlug(item.categorySlug);
  return {
    ...item,
    categorySlug,
    author: AUTHOR_BY_CATEGORY[categorySlug],
    content: contentFor(item),
    tags: tagsFor(item),
    mediaType: mediaTypeFor(categorySlug),
  };
}

/** Local detail catalog — covers listing samples + legacy library mocks. */
export function getPromptLabDetail(slug: string): PromptLabDetail | null {
  const item = localPromptCatalog().find((prompt) => prompt.slug === slug);
  return item ? toPromptLabDetail(item) : null;
}

export function relatedPromptLabPrompts(slug: string, limit = 3): PromptLabCardItem[] {
  const catalog = localPromptCatalog();
  const current = catalog.find((prompt) => prompt.slug === slug);
  if (!current) return [];
  const sameCategory = catalog.filter(
    (prompt) => prompt.slug !== slug && prompt.categorySlug === current.categorySlug,
  );
  const rest = catalog.filter(
    (prompt) => prompt.slug !== slug && prompt.categorySlug !== current.categorySlug,
  );
  return [...sameCategory, ...rest].slice(0, limit);
}

export function similarPromptLabPrompts(slug: string, limit = 4): PromptLabCardItem[] {
  const current = getPromptLabDetail(slug);
  if (!current) return [];
  const catalog = localPromptCatalog();
  const currentTags = new Set(current.tags);
  return catalog
    .filter((prompt) => prompt.slug !== slug)
    .slice()
    .sort((a, b) => {
      const detailA = toPromptLabDetail(a);
      const detailB = toPromptLabDetail(b);
      const tagScore = (item: PromptLabDetail) => item.tags.filter((tag) => currentTags.has(tag)).length;
      const modelScore = (item: PromptLabDetail) => (item.aiModel === current.aiModel ? 1 : 0);
      const categoryScore = (item: PromptLabDetail) => (item.categorySlug === current.categorySlug ? 2 : 0);
      const score = (item: PromptLabDetail) => tagScore(item) * 3 + categoryScore(item) + modelScore(item);
      return score(detailB) - score(detailA) || b.viewCount - a.viewCount;
    })
    .slice(0, limit);
}
