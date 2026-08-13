import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeValueCard, type HomeValueItem } from "@/components/public/home/home-value-card";

export const HOME_VALUE_ITEMS: readonly HomeValueItem[] = [
  {
    id: "paths",
    title: "مسیرهای یادگیری هدفمند",
    description: "مسیر ساخت‌یافته برای رشد مهارت مهندسی — از مبانی تا معماری و اجرا.",
    href: "/learning",
    icon: "paths",
    accent: "purple",
  },
  {
    id: "tools",
    title: "ابزارهای قدرتمند",
    description: "ابزارهای مهندسی را در یک کتابخانه پیدا کنید و در کار واقعی به کار ببرید.",
    href: "/toolbox",
    icon: "tools",
    accent: "cyan",
  },
  {
    id: "ai",
    title: "هوش مصنوعی پیشرفته",
    description: "از دانش منتشرشده HelpDev بپرسید و مسیر تحلیل تا راهکار را کوتاه‌تر کنید.",
    href: "/learning/assistant",
    icon: "ai",
    accent: "ai",
  },
  {
    id: "knowledge",
    title: "دانش مهندسی عمیق",
    description: "مقالات فنی و تجربه‌های معماری برای تصمیم‌گیری دقیق‌تر در پروژه‌های واقعی.",
    href: "/articles",
    icon: "knowledge",
    accent: "blue",
  },
];

/**
 * Homepage value proposition — four glass cards under the workflow.
 */
export function HomeValueSection() {
  return (
    <PublicSection
      className="home-value home-reveal"
      containerSize="wide"
      aria-labelledby="home-value-heading"
    >
      <div className="mx-auto max-w-3xl text-center">
        <h2
          id="home-value-heading"
          className="font-extrabold tracking-tight text-[color:var(--home-text)]"
          style={{
            fontSize: "clamp(1.35rem, 2.4vw, var(--home-title-size))",
            lineHeight: 1.45,
          }}
        >
          چرا HelpDev؟
        </h2>
        <p
          className="mx-auto mt-3 max-w-xl text-[color:var(--home-text-muted)]"
          style={{
            fontSize: "var(--home-body-size)",
            lineHeight: "var(--home-body-leading)",
          }}
        >
          چهار ستون پلتفرم برای یادگیری هدفمند، ابزار واقعی، هوش و دانش مهندسی.
        </p>
      </div>

      <ul className="home-value-grid mt-8 sm:mt-10">
        {HOME_VALUE_ITEMS.map((item) => (
          <HomeValueCard key={item.id} item={item} />
        ))}
      </ul>
    </PublicSection>
  );
}
