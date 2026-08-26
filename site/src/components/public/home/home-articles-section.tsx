import Link from "next/link";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import {
  HomeArticleCard,
  HOME_ARTICLE_TONES,
  type HomeArticleItem,
  type HomeArticleTone,
} from "@/components/public/home/home-article-card";
import { formatDateFa, labelForContentType } from "@/lib/admin/content/content-mappers";
import type { ContentSummaryDto } from "@/lib/api/content";
import { publicHrefForContent } from "@/lib/public/content-helpers";
import { estimateReadingLabel, inferTechTags } from "@/lib/public/display-meta";
import { HOME_COVERS, coverForHomeCategory } from "@/lib/public/home-covers";

/** HelpDev technical topics shown only when the published catalog is empty. */
export const HOME_ARTICLE_TOPICS: readonly HomeArticleItem[] = [
  {
    id: "modular-monolith",
    title: "رشد Modular Monolith در ASP.NET Core بدون شکستن مرز ماژول‌ها",
    excerpt:
      "چگونه Identity، Content و Learning در یک استقرار می‌مانند و قرارداد ماژول‌ها حفظ می‌شود.",
    href: "/search?q=modular%20monolith",
    category: "معماری",
    readingTime: estimateReadingLabel("رشد Modular Monolith در ASP.NET Core بدون شکستن مرز ماژول‌ها"),
    date: "",
    tone: "purple",
    image: HOME_COVERS.architecture,
  },
  {
    id: "rag-knowledge",
    title: "RAG روی دانش منتشرشده HelpDev؛ پاسخ زمینه‌دار بدون عدد ساختگی",
    excerpt: "بازیابی از مقالات و مسیرهای واقعی پلتفرم، نه تولید آمار یا کاتالوگ جعلی.",
    href: "/search?q=RAG%20HelpDev",
    category: "هوش مصنوعی",
    readingTime: estimateReadingLabel("RAG روی دانش منتشرشده HelpDev؛ پاسخ زمینه‌دار بدون عدد ساختگی"),
    date: "",
    tone: "cyan",
    image: HOME_COVERS.ai,
  },
  {
    id: "outbox",
    title: "Transactional Outbox برای رویدادهای Content و Learning",
    excerpt: "انتشار مطمئن رویداد دامنه بعد از تراکنش، بدون دوگانگی وضعیت در صف.",
    href: "/search?q=transactional%20outbox",
    category: "بک‌اند",
    readingTime: estimateReadingLabel("Transactional Outbox برای رویدادهای Content و Learning"),
    date: "",
    tone: "blue",
    image: HOME_COVERS.backend,
  },
  {
    id: "proxy-health",
    title: "قرارداد Reverse Proxy، Health Probe و انتشار API",
    excerpt: "مرز HTTPS تا Kestrel، پروب‌های زنده/آماده، و مسیر انتشار قطعی باینری.",
    href: "/search?q=health%20probe%20reverse%20proxy",
    category: "دواپس",
    readingTime: estimateReadingLabel("قرارداد Reverse Proxy، Health Probe و انتشار API"),
    date: "",
    tone: "purple",
    image: HOME_COVERS.devops,
  },
  {
    id: "rtl-home-tokens",
    title: "رابط RTL شیشه‌ای برای پلتفرم دانش HelpDev",
    excerpt: "هدر فشرده، هیرو و کارت‌های شیشه‌ای با سلسله‌مراتب بنفش، آبی و فیروزه‌ای.",
    href: "/search?q=RTL%20glass%20UI",
    category: "فرانت‌اند",
    readingTime: estimateReadingLabel("رابط RTL شیشه‌ای برای پلتفرم دانش HelpDev"),
    date: "",
    tone: "cyan",
    image: HOME_COVERS.frontend,
  },
];

type HomeArticlesSectionProps = {
  articles?: ContentSummaryDto[];
};

export function categoryForHomeArticle(title: string, slug = "", type = "Article"): string {
  const hay = `${title} ${slug}`;
  if (/microservice|architect|modular|monolith|معمار|مرز ماژول/i.test(hay)) return "معماری";
  if (/\brag\b|llm|openai|embedding|هوش مصنوعی|\bai\b/i.test(hay)) return "هوش مصنوعی";
  if (/devops|docker|kubernetes|\bk8s\b|ci\/?cd|proxy|health/i.test(hay)) return "دواپس";
  if (/frontend|front-end|react|next\.?js|rtl|فرانت/i.test(hay)) return "فرانت‌اند";
  if (/backend|back-end|asp\.?\s*net|\.net|\bapi\b|outbox|بک/i.test(hay)) return "بک‌اند";
  return labelForContentType(type);
}

export function excerptForHomeArticle(title: string, slug = ""): string {
  const tags = inferTechTags(title, slug);
  if (tags.length > 0) {
    return `نگاهی فنی به ${tags.slice(0, 2).join(" و ")} در پایگاه دانش HelpDev.`;
  }
  return "مقاله فنی از دانش HelpDev برای تصمیم‌گیری سریع‌تر پیش از مطالعه کامل.";
}

export function mapPublishedHomeArticle(
  item: ContentSummaryDto,
  index: number,
): HomeArticleItem {
  const tone = HOME_ARTICLE_TONES[index % HOME_ARTICLE_TONES.length] as HomeArticleTone;
  const category = categoryForHomeArticle(item.title, item.slug, item.type);
  return {
    id: item.id,
    title: item.title,
    excerpt: excerptForHomeArticle(item.title, item.slug),
    href: publicHrefForContent(item),
    category,
    readingTime: estimateReadingLabel(item.title),
    date: formatDateFa(item.createdAt),
    tone,
    image: coverForHomeCategory(category),
  };
}

export function buildHomeArticles(articles: ContentSummaryDto[]): HomeArticleItem[] {
  if (articles.length === 0) return HOME_ARTICLE_TOPICS.slice(0, 5);
  return articles.slice(0, 5).map(mapPublishedHomeArticle);
}

/**
 * Large latest-articles block — image, category, title, reading time.
 */
export function HomeArticlesSection({ articles = [] }: HomeArticlesSectionProps) {
  const items = buildHomeArticles(articles);

  return (
    <PublicSection
      className="home-articles home-reveal py-10 sm:py-12 lg:py-14"
      containerSize="wide"
      aria-labelledby="home-articles-heading"
    >
      <div className="mb-8 flex flex-wrap items-end justify-between gap-3 sm:mb-10">
        <div className="max-w-xl text-start">
          <p className="text-[12px] font-bold tracking-wide text-[#06B6D4]">مقالات</p>
          <h2 id="home-articles-heading" className="mt-1 text-[1.45rem] font-extrabold text-white sm:text-[1.7rem]">
            جدیدترین مقالات
          </h2>
          <p className="mt-3 text-[14px] leading-7 text-[#94A3B8]">
            تازه‌ترین دانش فنی HelpDev — معماری، هوش مصنوعی، بک‌اند و مسیر اجرا.
          </p>
        </div>
        <Link href="/articles" className="focus-ring text-[12px] font-semibold text-[#94A3B8] no-underline hover:text-white">
          همه مقالات
        </Link>
      </div>

      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-3 min-[1440px]:grid-cols-5 min-[1440px]:gap-4">
        {items.map((item) => (
          <HomeArticleCard key={item.id} item={item} />
        ))}
      </ul>
    </PublicSection>
  );
}
