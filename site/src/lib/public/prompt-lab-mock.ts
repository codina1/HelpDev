import { coverForPromptLabCategory, type PromptLabCategorySlug } from "@/lib/public/prompt-lab-covers";

export type PromptLabCategory = {
  slug: PromptLabCategorySlug;
  name: string;
};

export type PromptLabCardItem = {
  id: string;
  slug: string;
  title: string;
  description: string;
  coverImage: string;
  aiModel: string;
  category: string;
  categorySlug: string;
  copyCount: number;
  viewCount: number;
  featured: boolean;
  publishedAt: string;
};

export const PROMPT_LAB_CATEGORIES: readonly PromptLabCategory[] = [
  { slug: "image", name: "تصویر" },
  { slug: "video", name: "ویدئو" },
  { slug: "coding", name: "کدنویسی" },
  { slug: "writing", name: "تولید محتوا" },
  { slug: "design", name: "طراحی" },
  { slug: "marketing", name: "مارکتینگ" },
  { slug: "education", name: "آموزش" },
] as const;

const CATEGORY_NAME = Object.fromEntries(
  PROMPT_LAB_CATEGORIES.map((item) => [item.slug, item.name]),
) as Record<PromptLabCategorySlug, string>;

function item(
  partial: Omit<PromptLabCardItem, "coverImage" | "category"> & { categorySlug: PromptLabCategorySlug },
): PromptLabCardItem {
  return {
    ...partial,
    category: CATEGORY_NAME[partial.categorySlug],
    coverImage: coverForPromptLabCategory(partial.categorySlug),
  };
}

/** Local typed catalog for Sprint 01 — no API calls. */
export const PROMPT_LAB_PROMPTS: readonly PromptLabCardItem[] = [
  item({
    id: "pl-1",
    slug: "system-boundary-review",
    title: "بازبینی مرز ماژول در Modular Monolith",
    description: "پرامپت بررسی قرارداد دامنه، وابستگی بین ماژول‌ها و نشت زیرساخت.",
    aiModel: "Claude",
    categorySlug: "coding",
    copyCount: 186,
    viewCount: 1240,
    featured: true,
    publishedAt: "2026-08-16T08:00:00.000Z",
  }),
  item({
    id: "pl-2",
    slug: "product-ui-atmosphere",
    title: "اتمسفر رابط شیشه‌ای محصول AI",
    description: "تولید فضای بنفش/فیروزه‌ای برای هیرو و کارت‌های دانش، بدون استوک خارجی.",
    aiModel: "Midjourney",
    categorySlug: "image",
    copyCount: 242,
    viewCount: 1680,
    featured: true,
    publishedAt: "2026-08-15T11:00:00.000Z",
  }),
  item({
    id: "pl-3",
    slug: "release-walkthrough-video",
    title: "ویدئوی شرح انتشار باینری API",
    description: "سناریوی کوتاه برای توضیح health probe، پروکسی و کانال انتشار.",
    aiModel: "Veo",
    categorySlug: "video",
    copyCount: 94,
    viewCount: 720,
    featured: true,
    publishedAt: "2026-08-14T09:30:00.000Z",
  }),
  item({
    id: "pl-4",
    slug: "engineering-brief",
    title: "خلاصه تصمیم معماری برای تیم",
    description: "تبدیل بحث فنی به بریف کوتاه قابل اشتراک با نویسنده و مهندس.",
    aiModel: "ChatGPT",
    categorySlug: "writing",
    copyCount: 131,
    viewCount: 980,
    featured: true,
    publishedAt: "2026-08-13T16:00:00.000Z",
  }),
  item({
    id: "pl-5",
    slug: "design-token-audit",
    title: "ممیزی توکن رنگ و شعاع کارت",
    description: "چک‌لیست تیره پریمیوم: سطح شیشه‌ای، حاشیه، و تاکید بنفش/آبی/فیروزه‌ای.",
    aiModel: "Gemini",
    categorySlug: "design",
    copyCount: 77,
    viewCount: 540,
    featured: false,
    publishedAt: "2026-08-12T12:00:00.000Z",
  }),
  item({
    id: "pl-6",
    slug: "launch-narrative",
    title: "روایت معرفی مسیر یادگیری",
    description: "متن کمپین برای معرفی مسیر نقش‌محور بدون عدد ساختگی.",
    aiModel: "ChatGPT",
    categorySlug: "marketing",
    copyCount: 58,
    viewCount: 410,
    featured: false,
    publishedAt: "2026-08-11T10:00:00.000Z",
  }),
  item({
    id: "pl-7",
    slug: "outbox-lesson",
    title: "درس Transactional Outbox",
    description: "طرح درس کوتاه برای توضیح انتشار رویداد بعد از تراکنش.",
    aiModel: "Claude",
    categorySlug: "education",
    copyCount: 112,
    viewCount: 860,
    featured: false,
    publishedAt: "2026-08-10T08:00:00.000Z",
  }),
  item({
    id: "pl-8",
    slug: "gpt-image-cover",
    title: "کاور مسیر Frontend Engineer",
    description: "تصویر جلد برای کارت مسیر یادگیری با هویت HelpDev.",
    aiModel: "GPT Image",
    categorySlug: "image",
    copyCount: 63,
    viewCount: 390,
    featured: false,
    publishedAt: "2026-08-09T14:00:00.000Z",
  }),
  item({
    id: "pl-9",
    slug: "rag-query-rewrite",
    title: "بازنویسی پرسش برای RAG",
    description: "تبدیل سؤال آزاد به کوئری بازیابی روی دانش منتشرشده HelpDev.",
    aiModel: "Gemini",
    categorySlug: "coding",
    copyCount: 204,
    viewCount: 1510,
    featured: false,
    publishedAt: "2026-08-08T09:00:00.000Z",
  }),
  item({
    id: "pl-10",
    slug: "onboarding-script",
    title: "اسکریپت ویدئوی آشنایی با Prompt Lab",
    description: "متن ۳۰ ثانیه‌ای برای معرفی جستجو، دسته و کارت پرامپت.",
    aiModel: "Veo",
    categorySlug: "video",
    copyCount: 41,
    viewCount: 260,
    featured: false,
    publishedAt: "2026-08-17T07:00:00.000Z",
  }),
  item({
    id: "pl-11",
    slug: "docs-outline",
    title: "ساختار مقاله فنی Outbox",
    description: "خروجی سرفصل برای نویسنده محتوا با تمرکز بر قرارداد دامنه.",
    aiModel: "ChatGPT",
    categorySlug: "writing",
    copyCount: 88,
    viewCount: 610,
    featured: false,
    publishedAt: "2026-08-07T11:00:00.000Z",
  }),
  item({
    id: "pl-12",
    slug: "card-hierarchy",
    title: "سلسله‌مراتب کارت شیشه‌ای",
    description: "پرامپت چیدمان عنوان، بج مدل و شمارنده‌های مشاهده/کپی.",
    aiModel: "Claude",
    categorySlug: "design",
    copyCount: 99,
    viewCount: 705,
    featured: false,
    publishedAt: "2026-08-06T15:00:00.000Z",
  }),
];

function matchesFilter(
  prompt: PromptLabCardItem,
  query: string,
  categorySlug: string | null,
): boolean {
  if (categorySlug && prompt.categorySlug !== categorySlug) return false;
  const term = query.trim().toLowerCase();
  if (!term) return true;
  return (
    prompt.title.toLowerCase().includes(term) ||
    prompt.description.toLowerCase().includes(term) ||
    prompt.aiModel.toLowerCase().includes(term) ||
    prompt.category.includes(query.trim())
  );
}

export function filterPromptLabPrompts(
  items: readonly PromptLabCardItem[],
  query = "",
  categorySlug: string | null = null,
): PromptLabCardItem[] {
  return items.filter((prompt) => matchesFilter(prompt, query, categorySlug));
}

export function featuredPromptLabPrompts(
  items: readonly PromptLabCardItem[],
  query = "",
  categorySlug: string | null = null,
): PromptLabCardItem[] {
  return filterPromptLabPrompts(items, query, categorySlug)
    .filter((prompt) => prompt.featured)
    .slice(0, 4);
}

export function popularPromptLabPrompts(
  items: readonly PromptLabCardItem[],
  query = "",
  categorySlug: string | null = null,
): PromptLabCardItem[] {
  return filterPromptLabPrompts(items, query, categorySlug)
    .slice()
    .sort((a, b) => b.viewCount - a.viewCount || b.copyCount - a.copyCount)
    .slice(0, 4);
}

export function latestPromptLabPrompts(
  items: readonly PromptLabCardItem[],
  query = "",
  categorySlug: string | null = null,
): PromptLabCardItem[] {
  return filterPromptLabPrompts(items, query, categorySlug)
    .slice()
    .sort((a, b) => Date.parse(b.publishedAt) - Date.parse(a.publishedAt))
    .slice(0, 4);
}
