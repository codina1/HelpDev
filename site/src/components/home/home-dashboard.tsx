import Link from "next/link";
import { CATEGORY_LINKS } from "@/lib/constants";
import { HeroSection } from "@/components/home/hero-section";
import { HomeFooter } from "@/components/home/home-footer";
import { OutlineLinkButton } from "@/components/home/outline-link-button";
import { Badge, type BadgeVariant } from "@/components/ui/badge";
import {
  CHEAT_SHEETS,
  GITHUB_TRENDING,
  LATEST_NEWS_GRID,
  POPULAR_ROADMAPS,
  PROMPT_LAB_ITEMS,
  RECOMMENDED_COURSES,
  STARTER_KITS,
} from "@/data/home";

const HOME_CARD =
  "flex h-full flex-col rounded-2xl border border-white/[0.07] bg-[#111827]/90 p-4 shadow-[inset_0_1px_0_rgba(255,255,255,0.04)] transition-all duration-300 hover:border-violet-500/18 hover:shadow-[0_12px_40px_rgba(49,46,129,0.15)]";
const HOME_ITEM =
  "group focus-ring flex items-center gap-3 rounded-xl border border-white/[0.06] bg-white/[0.02] p-2.5 transition-all duration-300 hover:-translate-y-0.5 hover:border-violet-500/25 hover:bg-violet-500/[0.06] hover:shadow-[0_8px_24px_rgba(139,92,246,0.12)]";

const CHEAT_ICON_BG: Record<string, string> = {
  Git: "bg-orange-500/20 text-orange-300",
  SQL: "bg-blue-500/20 text-blue-300",
  JavaScript: "bg-amber-500/20 text-amber-300",
  Linux: "bg-emerald-500/20 text-emerald-300",
};

const NEWS_THUMB_BG = [
  "bg-violet-500/15",
  "bg-blue-500/15",
  "bg-cyan-500/15",
] as const;

const TAG_VARIANTS: Record<string, BadgeVariant> = {
  React: "tag",
  ".NET": "level",
  AI: "ai",
};

const CATEGORY_BADGE_STYLE: Record<string, string> = {
  داغ: "border-orange-400/50 bg-orange-500/30 text-orange-100",
  جدید: "border-emerald-400/50 bg-emerald-500/30 text-emerald-100",
  AI: "border-fuchsia-400/50 bg-fuchsia-500/30 text-fuchsia-100",
  ترند: "border-amber-400/50 bg-amber-500/30 text-amber-100",
};

function categoryBadgeVariant(badge: string): BadgeVariant {
  if (badge === "داغ") return "hot";
  if (badge === "جدید") return "new";
  if (badge === "AI") return "ai";
  return "trending";
}

function courseBadgeVariant(badge: string): BadgeVariant {
  if (badge === "محبوب") return "popular";
  return "pro";
}

export function HomeDashboard() {
  return (
    <div className="space-y-8">
      <HeroSection />

      <section className="animate-fade-up animate-fade-up-delay-1">
        <div className="mb-3 flex items-center gap-2">
          <p className="text-[11px] font-bold tracking-wide text-slate-500">
            دسترسی سریع
          </p>
          <Badge variant="updated">۸ بخش</Badge>
        </div>
        <div className="grid grid-cols-4 gap-3 sm:grid-cols-8 sm:gap-4">
          {CATEGORY_LINKS.map((item) => (
            <Link
              key={item.href}
              href={item.href}
              className="focus-ring group flex flex-col items-center gap-1.5 rounded-xl p-2 transition-all duration-300 hover:-translate-y-1 sm:gap-2"
            >
              <span
                className={`flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br text-xl shadow-[0_8px_24px_rgba(0,0,0,0.35)] transition-all duration-300 group-hover:scale-110 group-hover:shadow-[0_12px_32px_rgba(139,92,246,0.25)] sm:h-14 sm:w-14 sm:text-2xl ${item.color}`}
              >
                {item.icon}
              </span>
              <span className="text-center text-[10px] font-bold leading-4 text-slate-400 transition-colors group-hover:text-slate-200 sm:text-[11px]">
                {item.label}
              </span>
              {"badge" in item && item.badge && (
                <Badge
                  variant={categoryBadgeVariant(item.badge)}
                  size="md"
                  dot={false}
                  className={[
                    "min-w-[2.75rem] shadow-sm",
                    CATEGORY_BADGE_STYLE[item.badge] ?? "",
                  ].join(" ")}
                >
                  {item.badge}
                </Badge>
              )}
            </Link>
          ))}
        </div>
      </section>

      <section className="animate-fade-up animate-fade-up-delay-2 grid items-stretch gap-4 lg:grid-cols-3">
        <SectionCard title="نقشه راه‌های محبوب" href="/roadmap" buttonLabel="مشاهده همه نقشه‌راه‌ها" badge={{ label: "محبوب", variant: "popular" }}>
          <ul className="flex-1 space-y-2">
            {POPULAR_ROADMAPS.map((item) => (
              <li key={item.id}>
                <Link href="/roadmap" className={HOME_ITEM}>
                  <span
                    className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gradient-to-br text-lg shadow-inner ${item.color}`}
                  >
                    {item.icon}
                  </span>
                  <div className="min-w-0 flex-1">
                    <div className="mb-1.5 flex items-center justify-between gap-2">
                      <span className="flex min-w-0 items-center gap-1.5">
                        <span className="truncate text-[12px] font-bold text-white sm:text-[13px]">
                          {item.title}
                        </span>
                        <Badge variant={item.level === "جدید" ? "new" : item.level === "محبوب" ? "popular" : "level"}>
                          {item.level}
                        </Badge>
                      </span>
                      <Badge variant="pro">{item.progress}%</Badge>
                    </div>
                    <div className="h-1.5 overflow-hidden rounded-full bg-white/[0.06]">
                      <div
                        className={`progress-shine h-full rounded-full bg-gradient-to-l transition-all duration-700 ${item.color}`}
                        style={{ width: `${item.progress}%` }}
                      />
                    </div>
                  </div>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="آخرین اخبار" href="/news" buttonLabel="مشاهده همه اخبار" badge={{ label: "داغ", variant: "hot", pulse: true }}>
          <ul className="flex-1 space-y-2">
            {LATEST_NEWS_GRID.map((item, index) => (
              <li key={item.id}>
                <Link href="/news" className={HOME_ITEM}>
                  <span className="relative shrink-0">
                    <span
                      className={`flex h-11 w-14 items-center justify-center rounded-lg text-lg ${NEWS_THUMB_BG[index % NEWS_THUMB_BG.length]}`}
                    >
                      {item.thumb}
                    </span>
                    {item.isNew && (
                      <span className="absolute -top-1 -end-1">
                        <Badge variant="new" pulse>
                          جدید
                        </Badge>
                      </span>
                    )}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="block text-[12px] font-bold leading-5 text-white sm:text-[13px]">
                      {item.title}
                    </span>
                    <span className="mt-1 flex flex-wrap items-center gap-2">
                      <Badge variant={TAG_VARIANTS[item.tag] ?? "tag"}>{item.tag}</Badge>
                      <span className="text-[10px] text-slate-500">{item.time}</span>
                    </span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="دوره‌های پیشنهادی" href="/courses" buttonLabel="مشاهده همه دوره‌ها" badge={{ label: "پیشنهاد ویژه", variant: "pro" }}>
          <ul className="flex-1 space-y-2">
            {RECOMMENDED_COURSES.map((item) => (
              <li key={item.id}>
                <Link href="/courses" className={HOME_ITEM}>
                  <span className="relative shrink-0">
                    <span className="flex h-11 w-11 items-center justify-center rounded-xl border border-white/[0.06] bg-gradient-to-br from-indigo-500/20 to-violet-500/10 text-lg">
                      {item.thumb}
                    </span>
                    {item.badge && (
                      <span className="absolute -top-1 -end-1">
                        <Badge variant={courseBadgeVariant(item.badge)}>{item.badge}</Badge>
                      </span>
                    )}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="block truncate text-[12px] font-bold text-white sm:text-[13px]">
                      {item.title}
                    </span>
                    <span className="mt-1 flex items-center justify-between gap-2">
                      <span className="text-[10px] text-slate-500">{item.platform}</span>
                      <span className="flex shrink-0 items-center gap-1.5">
                        {item.free && <Badge variant="free">رایگان</Badge>}
                        <Badge variant="trending">★ {item.rating}</Badge>
                      </span>
                    </span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>
      </section>

      <section className="animate-fade-up animate-fade-up-delay-3 grid items-stretch gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <SectionCard title="چیت‌شیت" href="/cheat-sheets" buttonLabel="مشاهده همه چیت‌شیت‌ها" badge={{ label: "مرجع", variant: "updated" }}>
          <ul className="flex-1 space-y-2">
            {CHEAT_SHEETS.map((item) => (
              <li key={item.id}>
                <Link href={item.href} className={HOME_ITEM}>
                  <span
                    className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-base ${CHEAT_ICON_BG[item.title] ?? "bg-white/10 text-slate-300"}`}
                  >
                    {item.icon}
                  </span>
                  <span className="flex min-w-0 flex-1 items-center justify-between gap-2">
                    <span className="truncate text-[12px] font-semibold text-slate-200">
                      {item.title}
                    </span>
                    {item.updated && <Badge variant="updated">به‌روز</Badge>}
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="Prompt Lab" href="/prompt-lab" buttonLabel="باز کردن Prompt Lab" badge={{ label: "AI", variant: "ai" }}>
          <ul className="flex-1 space-y-2">
            {PROMPT_LAB_ITEMS.map((item) => (
              <li key={item.id}>
                <Link href={item.href} className={HOME_ITEM}>
                  <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-pink-500/15 text-base text-pink-300">
                    ✨
                  </span>
                  <span className="flex min-w-0 flex-1 items-center justify-between gap-2">
                    <span className="truncate text-[12px] font-semibold text-slate-200">
                      {item.title}
                    </span>
                    {item.badge && (
                      <Badge variant={item.badge === "AI" ? "ai" : "pro"}>{item.badge}</Badge>
                    )}
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="گیت‌هاب ترندینگ" href="/github-trending" buttonLabel="مشاهده ترندینگ" badge={{ label: "ترند", variant: "trending" }}>
          <ul className="flex-1 space-y-2">
            {GITHUB_TRENDING.map((item) => (
              <li key={item.id}>
                <Link href="/github-trending" className={HOME_ITEM}>
                  <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border border-white/[0.06] bg-[#0a0b14] text-base">
                    {item.icon}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="flex items-center gap-1.5">
                      <span className="block truncate text-[12px] font-semibold text-slate-200">
                        {item.name}
                      </span>
                      {item.trending && <Badge variant="trending">🔥</Badge>}
                    </span>
                    <Badge variant="trending" className="mt-1">
                      ⭐ {item.stars}
                    </Badge>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>

        <SectionCard title="Dev Starter Kit" href="/starter-kit" buttonLabel="مشاهده همه قالب‌ها" badge={{ label: "قالب", variant: "new" }}>
          <ul className="flex-1 space-y-2">
            {STARTER_KITS.map((item) => (
              <li key={item.id}>
                <Link href="/starter-kit" className={HOME_ITEM}>
                  <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg border border-white/[0.08] bg-white/[0.04] text-base text-white">
                    {item.icon}
                  </span>
                  <span className="min-w-0 flex-1">
                    <span className="flex items-center gap-1.5">
                      <span className="truncate text-[12px] font-semibold text-slate-200">
                        {item.title}
                      </span>
                      {item.badge && (
                        <Badge variant={item.badge === "جدید" ? "new" : "tag"}>{item.badge}</Badge>
                      )}
                    </span>
                    <span className="text-[10px] text-slate-500">{item.stack}</span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        </SectionCard>
      </section>

      <HomeFooter />
    </div>
  );
}

function SectionCard({
  title,
  href,
  buttonLabel,
  badge,
  children,
}: {
  title: string;
  href: string;
  buttonLabel: string;
  badge?: { label: string; variant: BadgeVariant; pulse?: boolean };
  children: React.ReactNode;
}) {
  return (
    <div className={HOME_CARD}>
      <div className="mb-3 flex items-center justify-between gap-2">
        <h2 className="home-section-title text-[13px] font-extrabold text-white">
          {title}
        </h2>
        {badge && (
          <Badge variant={badge.variant} pulse={badge.pulse}>
            {badge.label}
          </Badge>
        )}
      </div>
      <div className="flex flex-1 flex-col">{children}</div>
      <OutlineLinkButton href={href}>{buttonLabel}</OutlineLinkButton>
    </div>
  );
}
