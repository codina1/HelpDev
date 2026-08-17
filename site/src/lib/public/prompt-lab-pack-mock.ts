import { coverForPromptLabCategory, type PromptLabCategorySlug } from "@/lib/public/prompt-lab-covers";
import { toPromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { PROMPT_LAB_CATEGORIES, PROMPT_LAB_PROMPTS, type PromptLabCardItem } from "@/lib/public/prompt-lab-mock";

export type PromptLabPackListItem = {
  order: number;
  prompt: PromptLabCardItem;
  content: string;
  preview: string;
};

export type PromptLabPack = {
  id: string;
  slug: string;
  title: string;
  description: string;
  coverImage: string;
  category: string;
  categorySlug: PromptLabCategorySlug;
  items: readonly PromptLabPackListItem[];
};

const CATEGORY_NAME = Object.fromEntries(
  PROMPT_LAB_CATEGORIES.map((item) => [item.slug, item.name]),
) as Record<PromptLabCategorySlug, string>;

type PackSeed = {
  id: string;
  slug: string;
  title: string;
  description: string;
  categorySlug: PromptLabCategorySlug;
  promptSlugs: readonly string[];
};

const PACK_SEEDS: readonly PackSeed[] = [
  {
    id: "pack-1",
    slug: "modular-monolith-studio",
    title: "استودیوی Modular Monolith",
    description:
      "چهار پرامپت پشت‌سرهم برای بازبینی مرز ماژول، بازنویسی پرسش RAG، درس Outbox و بریف تصمیم معماری.",
    categorySlug: "coding",
    promptSlugs: [
      "system-boundary-review",
      "rag-query-rewrite",
      "outbox-lesson",
      "engineering-brief",
    ],
  },
  {
    id: "pack-2",
    slug: "glass-ui-kit",
    title: "کیت رابط شیشه‌ای HelpDev",
    description:
      "پک طراحی برای اتمسفر هیرو، ممیزی توکن، سلسله‌مراتب کارت و کاور مسیر یادگیری — بدون استوک خارجی.",
    categorySlug: "design",
    promptSlugs: [
      "product-ui-atmosphere",
      "design-token-audit",
      "card-hierarchy",
      "gpt-image-cover",
    ],
  },
  {
    id: "pack-3",
    slug: "product-launch-story",
    title: "روایت انتشار محصول",
    description:
      "از کمپین مسیر یادگیری تا اسکریپت ویدئوی انتشار و سرفصل مقاله؛ پکی برای روایت محصول HelpDev.",
    categorySlug: "writing",
    promptSlugs: [
      "launch-narrative",
      "docs-outline",
      "release-walkthrough-video",
      "onboarding-script",
    ],
  },
];

export function previewPromptLabContent(content: string, lineCount = 4): string {
  return content
    .replace(/\r\n/g, "\n")
    .split("\n")
    .filter((line) => line.trim().length > 0)
    .slice(0, lineCount)
    .join("\n");
}

function resolveItems(promptSlugs: readonly string[]): PromptLabPackListItem[] {
  return promptSlugs.map((slug, index) => {
    const prompt = PROMPT_LAB_PROMPTS.find((item) => item.slug === slug);
    if (!prompt) {
      throw new Error(`Unknown prompt slug in pack mock: ${slug}`);
    }
    const content = toPromptLabDetail(prompt).content;
    return {
      order: index + 1,
      prompt,
      content,
      preview: previewPromptLabContent(content),
    };
  });
}

function toPack(seed: PackSeed): PromptLabPack {
  return {
    id: seed.id,
    slug: seed.slug,
    title: seed.title,
    description: seed.description,
    coverImage: coverForPromptLabCategory(seed.categorySlug),
    category: CATEGORY_NAME[seed.categorySlug],
    categorySlug: seed.categorySlug,
    items: resolveItems(seed.promptSlugs),
  };
}

/** Local Prompt Pack catalog — no API. */
export const PROMPT_LAB_PACKS: readonly PromptLabPack[] = PACK_SEEDS.map(toPack);

export function getPromptLabPack(slug: string): PromptLabPack | null {
  return PROMPT_LAB_PACKS.find((pack) => pack.slug === slug) ?? null;
}
