import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import {
  HomeQuickAccessCard,
  type HomeQuickAccessItem,
} from "@/components/public/home/home-quick-access-card";

export const HOME_QUICK_ACCESS_ITEMS: readonly HomeQuickAccessItem[] = [
  {
    id: "news",
    title: "اخبار",
    description: "آخرین اخبار دنیای توسعه",
    href: "/news",
    icon: "news",
  },
  {
    id: "tools",
    title: "ابزارها",
    description: "ابزارهای کاربردی توسعه‌دهندگان",
    href: "/toolbox",
    icon: "tools",
  },
  {
    id: "prompt-lab",
    title: "Prompt Lab",
    description: "پرامپت‌های آماده هوش مصنوعی",
    href: "/prompt-lab",
    icon: "prompt",
  },
  {
    id: "roadmap",
    title: "Roadmap",
    description: "مسیرهای یادگیری",
    href: "/roadmap",
    icon: "roadmap",
  },
  {
    id: "learning",
    title: "یادگیری",
    description: "دوره‌ها و آموزش‌ها",
    href: "/learning",
    icon: "learning",
  },
] as const;

/** Five quick-access cards under the homepage hero. */
export function HomeQuickAccessSection() {
  return (
    <PublicSection
      className="home-quick-access home-reveal !pt-0 pb-6 sm:pb-7 lg:pb-8"
      bare
      aria-labelledby="home-quick-access-heading"
    >
      <PublicContainer size="wide">
        <div className="mb-5 text-center sm:mb-6 sm:text-start">
          <p className="text-[12px] font-bold tracking-wide text-[#06B6D4]">دسترسی سریع</p>
          <h2
            id="home-quick-access-heading"
            className="mt-1 text-[1.25rem] font-extrabold text-white sm:text-[1.4rem]"
          >
            مسیرهای اصلی HelpDev
          </h2>
        </div>

        <ul className="grid grid-cols-2 gap-3 sm:gap-4 lg:grid-cols-5 lg:gap-4">
          {HOME_QUICK_ACCESS_ITEMS.map((item) => (
            <HomeQuickAccessCard key={item.id} item={item} />
          ))}
        </ul>
      </PublicContainer>
    </PublicSection>
  );
}
