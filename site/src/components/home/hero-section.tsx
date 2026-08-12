"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { OutlineLinkButton } from "@/components/home/outline-link-button";
import { Badge, type BadgeVariant } from "@/components/ui/badge";
import { HERO_QUICK_LINKS } from "@/lib/constants";
import { DEV_DIGEST, HERO_STATS, HOT_NEWS } from "@/data/home";

function DigestBadge({ label }: { label: string }) {
  if (label === "NEW") return <Badge variant="new" pulse>NEW</Badge>;
  if (label === "Preview") return <Badge variant="pro">Preview</Badge>;
  return <Badge variant="updated">{label}</Badge>;
}

export function HeroSection() {
  const router = useRouter();
  const [query, setQuery] = useState("");

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const params = new URLSearchParams();
    if (query.trim()) params.set("q", query.trim());
    params.set("tab", "news");
    router.push(`/search?${params.toString()}`);
  }

  return (
    <section className="grid items-stretch gap-4 lg:grid-cols-[270px_1fr_270px]">
      <SidePanel title="Dev Digest" icon="🔥" panelBadge={{ label: "امروز", variant: "updated" }} className="order-3 animate-fade-up lg:order-1">
        <ul className="flex-1 space-y-2">
          {DEV_DIGEST.map((item) => (
            <li key={item.id}>
              <Link
                href="/news"
                className="focus-ring flex items-center gap-2.5 rounded-xl border border-white/[0.06] bg-white/[0.02] p-2.5 transition-all duration-300 hover:-translate-y-0.5 hover:border-violet-500/20 hover:bg-violet-500/[0.06] hover:shadow-[0_6px_20px_rgba(139,92,246,0.1)]"
              >
                <span
                  className={`flex h-9 w-9 shrink-0 items-center justify-center rounded-lg text-[10px] font-bold ${item.iconBg}`}
                >
                  {item.icon}
                </span>
                <span className="min-w-0 flex-1">
                  <span className="flex items-center gap-1.5">
                    <span className="block truncate text-[12px] font-bold text-white">
                      {item.title}
                    </span>
                    {item.badge && <DigestBadge label={item.badge} />}
                  </span>
                  <span className="text-[10px] text-slate-500">{item.time}</span>
                </span>
              </Link>
            </li>
          ))}
        </ul>
        <OutlineLinkButton href="/news">مشاهده همه خلاصه‌ها</OutlineLinkButton>
      </SidePanel>

      {/* Center hero — دو نیمه تمام‌قد */}
      <div className="hero-glow-ring order-1 h-full min-h-[360px] overflow-hidden rounded-2xl border border-indigo-500/20 bg-gradient-to-br from-[#1a1245]/90 via-[#12102a] to-[#0c1a3a]/95 animate-fade-up animate-fade-up-delay-1 lg:order-2">
        <div className="grid h-full grid-cols-1 lg:grid-cols-2">
          {/* نیمه محتوا */}
          <div className="order-2 flex h-full flex-col p-5 sm:p-6 lg:order-1 lg:p-7">
            <div className="shrink-0">
              <h1 className="bg-gradient-to-l from-white via-white to-violet-200 bg-clip-text text-[22px] font-black leading-tight text-transparent sm:text-[26px]">
                مرجع کامل برنامه‌نویسان
              </h1>
              <p className="mt-2 text-[14px] font-medium text-white/90">
                یاد بگیر، به‌روز باش، سریع‌تر توسعه بده
              </p>
              <div className="mt-2.5 flex flex-wrap gap-1.5">
                {HERO_STATS.map((stat) => (
                  <Badge
                    key={stat.label}
                    variant={stat.variant}
                    pulse={stat.variant === "live"}
                  >
                    {stat.label}
                  </Badge>
                ))}
              </div>
              <p className="mt-2 text-[12px] leading-6 text-slate-400">
                HelpDev همراه روزانه شما برای اخبار، آموزش، ابزارها و منابعی
                که توسعه‌دهنده‌ای باید داشته باشد.
              </p>
            </div>

            <HeroTechShowcase />

            <div className="shrink-0">
              <form
                onSubmit={handleSubmit}
                className="focus-within:ring-2 focus-within:ring-violet-500/30 flex overflow-hidden rounded-xl shadow-[0_8px_30px_rgba(0,0,0,0.3)] transition-shadow duration-300 focus-within:shadow-[0_12px_40px_rgba(99,102,241,0.25)]"
              >
                <input
                  type="search"
                  value={query}
                  onChange={(e) => setQuery(e.target.value)}
                  placeholder="دنبال چی هستی؟ (مثال: API, React, ...)"
                  className="focus-ring h-11 min-w-0 flex-1 border-0 bg-white px-4 text-[13px] text-slate-900 outline-none placeholder:text-slate-400"
                  aria-label="جستجو"
                />
                <button
                  type="submit"
                  className="focus-ring flex h-11 w-11 shrink-0 items-center justify-center bg-gradient-to-b from-blue-500 to-blue-600 text-white transition-all duration-200 hover:brightness-110 active:scale-95"
                  aria-label="جستجو"
                >
                  <SearchIcon />
                </button>
              </form>

              <div className="mt-3 flex flex-wrap gap-2">
                {HERO_QUICK_LINKS.map((link) => (
                  <Link
                    key={link.href}
                    href={link.href}
                    className="focus-ring flex items-center gap-1.5 rounded-full border border-violet-500/30 bg-white/[0.03] px-3 py-1.5 text-[11px] font-medium text-slate-300 shadow-[0_0_12px_rgba(139,92,246,0.08)] transition-all duration-200 hover:-translate-y-0.5 hover:border-violet-400/55 hover:bg-violet-500/10 hover:text-white hover:shadow-[0_4px_16px_rgba(139,92,246,0.2)]"
                  >
                    <QuickLinkIcon label={link.label} />
                    {link.label}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          {/* نیمه گرافیک — تمام ارتفاع */}
          <div className="relative order-1 h-40 bg-gradient-to-br from-indigo-950/60 via-[#151030]/80 to-blue-950/40 sm:h-44 lg:order-2 lg:h-full lg:min-h-0">
            <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_50%_80%,rgba(99,102,241,0.25),transparent_65%)]" />
            <div className="relative flex h-full items-center justify-center overflow-hidden p-2 lg:p-6">
              <div className="origin-center scale-[0.55] sm:scale-[0.65] lg:scale-100">
                <div className="illustration-glow">
                  <HeroIllustration />
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <SidePanel title="اخبار داغ" icon="🔥" panelBadge={{ label: "LIVE", variant: "live", pulse: true }} className="order-2 animate-fade-up animate-fade-up-delay-2 lg:order-3">
        <ul className="flex-1 space-y-1.5">
          {HOT_NEWS.map((item) => (
            <li key={item.id}>
              <Link
                href="/news"
                className="focus-ring flex items-start justify-between gap-2 rounded-xl border border-white/[0.06] bg-white/[0.02] px-2.5 py-2 transition-all duration-300 hover:-translate-y-0.5 hover:border-violet-500/20 hover:bg-violet-500/[0.06] hover:shadow-[0_6px_20px_rgba(139,92,246,0.1)]"
              >
                <span className="min-w-0 flex-1">
                  <span className="flex items-start gap-1">
                    <span className="block text-[11px] font-semibold leading-5 text-slate-200">
                      {item.title}
                    </span>
                    {item.hot && (
                      <Badge variant="hot" className="shrink-0 mt-0.5">
                        داغ
                      </Badge>
                    )}
                  </span>
                  <span className="text-[10px] text-slate-500">{item.time}</span>
                </span>
                <TagBadge tag={item.tag} tagColor={item.tagColor} />
              </Link>
            </li>
          ))}
        </ul>
        <OutlineLinkButton href="/news">مشاهده همه اخبار</OutlineLinkButton>
      </SidePanel>
    </section>
  );
}

function SidePanel({
  title,
  icon,
  panelBadge,
  className,
  children,
}: {
  title: string;
  icon: string;
  panelBadge?: { label: string; variant: BadgeVariant; pulse?: boolean };
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <aside
      className={[
        "flex min-h-[360px] flex-col rounded-2xl border border-white/[0.07] bg-[#111827]/90 p-4 shadow-[inset_0_1px_0_rgba(255,255,255,0.04)] transition-colors duration-300 hover:border-violet-500/12",
        className ?? "",
      ].join(" ")}
    >
      <div className="mb-3 flex items-center justify-between gap-2 border-b border-white/[0.05] pb-3">
        <div className="flex items-center gap-2">
          <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-violet-500/10 text-sm">
            {icon}
          </span>
          <h2 className="home-section-title text-[13px] font-extrabold text-white">
            {title}
          </h2>
        </div>
        {panelBadge && (
          <Badge variant={panelBadge.variant} pulse={panelBadge.pulse}>
            {panelBadge.label}
          </Badge>
        )}
      </div>
      {children}
    </aside>
  );
}

function TagBadge({ tag, tagColor }: { tag: string; tagColor: string }) {
  return (
    <span className={`shrink-0 rounded-md border px-1.5 py-0.5 text-[9px] font-bold ${tagColor}`}>
      {tag}
    </span>
  );
}

function HeroTechShowcase() {
  return (
    <div className="relative my-4 flex min-h-[100px] flex-1 flex-col justify-center py-2 sm:min-h-[120px]">
      <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(ellipse_at_50%_50%,rgba(99,102,241,0.08),transparent_70%)]" />

      {/* آیکون‌های شناور */}
      <TechFloatBadge
        label="React"
        icon="⚛"
        positionClassName="-top-1 end-0 z-10"
        badgeClassName="border-cyan-500/35 bg-cyan-500/10 text-cyan-300"
        animateClassName="hero-float hero-float-delay-1"
      />
      <TechFloatBadge
        label="TypeScript"
        icon="TS"
        positionClassName="top-6 start-0 z-10"
        badgeClassName="border-blue-500/35 bg-blue-500/10 text-blue-300"
        animateClassName="hero-float-slow hero-float-delay-2"
      />
      <TechFloatBadge
        label="Python"
        icon="🐍"
        positionClassName="bottom-2 end-2 z-10"
        badgeClassName="border-amber-500/35 bg-amber-500/10 text-amber-200"
        animateClassName="hero-float hero-float-delay-3"
      />
      <TechFloatBadge
        label=".NET"
        icon="◆"
        positionClassName="bottom-0 start-6 z-10 hidden sm:block"
        badgeClassName="border-violet-500/35 bg-violet-500/10 text-violet-300"
        animateClassName="hero-float-slow"
      />
      <TechFloatBadge
        label="Node"
        icon="⬡"
        positionClassName="top-1/2 -translate-y-1/2 start-1 z-10 hidden lg:block"
        badgeClassName="border-emerald-500/35 bg-emerald-500/10 text-emerald-300"
        animateClassName="hero-float hero-float-delay-2"
      />

      {/* اسنیپت کد */}
      <div className="relative z-0 mx-auto w-full max-w-[280px] overflow-hidden rounded-xl border border-white/[0.08] bg-black/40 shadow-[0_0_30px_rgba(99,102,241,0.12)] backdrop-blur-sm sm:max-w-none">
        <div className="flex items-center gap-1.5 border-b border-white/[0.06] bg-white/[0.03] px-3 py-2">
          <span className="h-2 w-2 rounded-full bg-red-400/70" />
          <span className="h-2 w-2 rounded-full bg-amber-400/70" />
          <span className="h-2 w-2 rounded-full bg-emerald-400/70" />
          <span className="ms-auto font-mono text-[9px] text-slate-500">helpdev.ts</span>
          <Badge variant="live" pulse className="ms-1">
            live
          </Badge>
        </div>
        <pre
          dir="ltr"
          className="overflow-x-auto p-3 font-mono text-[10px] leading-[1.65] text-left sm:text-[11px]"
        >
          <code>
            <span className="text-violet-400">const</span>{" "}
            <span className="text-sky-300">dev</span>{" "}
            <span className="text-slate-500">=</span>{" "}
            <span className="text-amber-300">{"{"}</span>
            {"\n"}
            <span className="text-slate-600">  </span>
            <span className="text-emerald-400">stack</span>
            <span className="text-slate-500">:</span>{" "}
            <span className="text-amber-300">[</span>
            <span className="text-orange-300">&apos;React&apos;</span>
            <span className="text-slate-500">, </span>
            <span className="text-orange-300">&apos;TS&apos;</span>
            <span className="text-amber-300">]</span>
            <span className="text-slate-500">,</span>
            {"\n"}
            <span className="text-slate-600">  </span>
            <span className="text-emerald-400">goal</span>
            <span className="text-slate-500">:</span>{" "}
            <span className="text-orange-300">&apos;ship faster&apos;</span>
            {"\n"}
            <span className="text-amber-300">{"}"}</span>
            <span className="text-slate-500">;</span>
            {"\n"}
            <span className="text-violet-400">await</span>{" "}
            <span className="text-sky-300">dev</span>
            <span className="text-slate-500">.</span>
            <span className="text-yellow-300">learn</span>
            <span className="text-slate-500">();</span>
            <span className="text-slate-600"> </span>
            <span className="text-slate-600">{"// 🚀"}</span>
          </code>
        </pre>
      </div>

      {/* نوار تکنولوژی */}
      <div className="relative z-0 mt-3 flex flex-wrap justify-center gap-1.5">
        {TECH_PILLS.map((pill) => (
          <span
            key={pill.label}
            className={`rounded-md border px-2 py-0.5 font-mono text-[9px] font-semibold sm:text-[10px] ${pill.className}`}
          >
            {pill.label}
          </span>
        ))}
      </div>
    </div>
  );
}

const TECH_PILLS = [
  { label: "JS", className: "border-yellow-500/25 bg-yellow-500/10 text-yellow-300" },
  { label: "Go", className: "border-cyan-500/25 bg-cyan-500/10 text-cyan-300" },
  { label: "Rust", className: "border-orange-500/25 bg-orange-500/10 text-orange-300" },
  { label: "Docker", className: "border-blue-500/25 bg-blue-500/10 text-blue-300" },
  { label: "SQL", className: "border-pink-500/25 bg-pink-500/10 text-pink-300" },
  { label: "Git", className: "border-red-500/25 bg-red-500/10 text-red-300" },
] as const;

function TechFloatBadge({
  label,
  icon,
  positionClassName,
  badgeClassName,
  animateClassName = "hero-float",
}: {
  label: string;
  icon: string;
  positionClassName: string;
  badgeClassName: string;
  animateClassName?: string;
}) {
  return (
    <span className={`absolute ${positionClassName}`} aria-hidden>
      <span
        className={`${animateClassName} flex items-center gap-1 rounded-lg border px-2 py-1 text-[10px] font-bold shadow-lg backdrop-blur-sm ${badgeClassName}`}
      >
        <span>{icon}</span>
        <span className="hidden sm:inline">{label}</span>
      </span>
    </span>
  );
}

function HeroIllustration() {
  return (
    <div className="relative h-[240px] w-[260px]">
      {/* Browser window */}
      <div className="absolute inset-x-3 top-2 rounded-2xl border border-white/10 bg-gradient-to-b from-indigo-900/50 to-slate-900/90 p-4 shadow-2xl">
        <div className="mb-3 flex gap-1.5">
          <span className="h-2.5 w-2.5 rounded-full bg-red-400/80" />
          <span className="h-2.5 w-2.5 rounded-full bg-amber-400/80" />
          <span className="h-2.5 w-2.5 rounded-full bg-emerald-400/80" />
        </div>
        <div className="space-y-2.5">
          <div className="h-2.5 w-4/5 rounded-full bg-indigo-400/35" />
          <div className="h-2.5 w-full rounded-full bg-white/10" />
          <div className="h-2.5 w-11/12 rounded-full bg-white/10" />
          <div className="mt-4 flex gap-2">
            <div className="h-10 flex-1 rounded-lg bg-blue-500/20" />
            <div className="h-10 w-10 rounded-lg bg-violet-500/25" />
          </div>
        </div>
      </div>

      {/* Code icon */}
      <div className="absolute bottom-4 start-6">
        <div className="absolute inset-0 rounded-2xl bg-blue-500/50 blur-2xl" />
        <div className="relative flex h-16 w-16 items-center justify-center rounded-2xl border border-blue-400/40 bg-gradient-to-br from-blue-500 to-indigo-600 text-xl font-bold text-white shadow-[0_0_40px_rgba(59,130,246,0.6)]">
          {"</>"}
        </div>
      </div>
    </div>
  );
}

function QuickLinkIcon({ label }: { label: string }) {
  const paths: Record<string, React.ReactNode> = {
    Roadmap: (
      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
        <path d="M3 6h18M3 12h12M3 18h6" />
      </svg>
    ),
    "Cheat Sheet": (
      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
        <rect x="4" y="4" width="16" height="16" rx="2" />
        <path d="M8 9h8M8 13h5" />
      </svg>
    ),
    "Prompt Lab": (
      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
        <path d="M12 3l1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5z" />
      </svg>
    ),
    Courses: (
      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" />
        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" />
      </svg>
    ),
  };

  return <span className="text-violet-400">{paths[label] ?? null}</span>;
}

function SearchIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}
