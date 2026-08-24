import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_PLATFORM_STATS = [
  { id: "articles", value: "+۱۲۰۰", label: "مقاله آموزشی" },
  { id: "prompts", value: "+۵۰۰", label: "Prompt آماده" },
  { id: "tools", value: "+۸۰", label: "ابزار کاربردی" },
  { id: "devs", value: "+۲۵K", label: "توسعه‌دهنده" },
] as const;

/** Four marketing stats under search. */
export function HomeStatsSection() {
  return (
    <PublicSection className="home-platform-stats home-reveal" bare aria-label="آمار پلتفرم">
      <PublicContainer size="wide">
        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-4 sm:gap-4">
          {HOME_PLATFORM_STATS.map((stat) => (
            <div
              key={stat.id}
              className="rounded-2xl border border-white/[0.08] bg-[#0B1224] px-3 py-4 text-center transition hover:border-[rgba(124,58,237,0.35)] hover:shadow-[0_0_28px_rgba(124,58,237,0.18)] sm:px-4 sm:py-5"
            >
              <dt className="sr-only">{stat.label}</dt>
              <dd className="text-[1.2rem] font-extrabold tracking-tight text-white sm:text-2xl">
                {stat.value}
              </dd>
              <p className="mt-1.5 text-[11px] font-semibold text-[#94A3B8] sm:text-[12px]">
                {stat.label}
              </p>
            </div>
          ))}
        </dl>
      </PublicContainer>
    </PublicSection>
  );
}
