"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { SmartEmptyState } from "@/components/experience/smart-empty-state";
import { GlassCard } from "@/components/ui/public/v2/glass-card";
import { GlowButton } from "@/components/ui/public/v2/glow-button";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import {
  fetchLearningProfile,
  fetchLearningRecommendations,
  fetchLearningRoadmap,
  type LearningProfileDto,
  type LearningRecommendationDto,
  type LearningRoadmapDto,
} from "@/lib/api/learning-personalization";

type LoadState = "idle" | "loading" | "ready" | "error";

/**
 * Authenticated-only personalized strip — existing learning APIs only, no invented values.
 */
export function PersonalizedHero() {
  const { token, user, isReady } = useAuth();
  const [state, setState] = useState<LoadState>("idle");
  const [profile, setProfile] = useState<LearningProfileDto | null>(null);
  const [recs, setRecs] = useState<LearningRecommendationDto | null>(null);
  const [roadmap, setRoadmap] = useState<LearningRoadmapDto | null>(null);

  useEffect(() => {
    if (!isReady || !token || !user) {
      setState("idle");
      return;
    }

    const controller = new AbortController();
    setState("loading");

    void (async () => {
      try {
        const [p, r, map] = await Promise.all([
          fetchLearningProfile(token, controller.signal).catch(() => null),
          fetchLearningRecommendations(token, controller.signal).catch(() => null),
          fetchLearningRoadmap(token, controller.signal).catch(() => null),
        ]);
        if (controller.signal.aborted) return;
        setProfile(p);
        setRecs(r);
        setRoadmap(map);
        setState("ready");
      } catch {
        if (!controller.signal.aborted) setState("error");
      }
    })();

    return () => controller.abort();
  }, [isReady, token, user]);

  if (!isReady || !user || !token) return null;

  const recommendationItems = recs?.recommendedItems ?? [];
  const hasProfile = Boolean(profile?.experienceLevel || profile?.learningGoals);
  const hasRoadmap = Boolean(roadmap?.id);
  const hasRecs = recommendationItems.length > 0;

  return (
    <section className="pub-fade-up border-y border-[color:var(--pub-glass-border)] bg-[color:color-mix(in_srgb,var(--pub-bg-elevated)_70%,transparent)] py-8 sm:py-10" aria-labelledby="personalized-hero-title">
      <PublicContainer size="wide">
        <div className="mb-5 flex flex-wrap items-center justify-between gap-2">
          <div>
            <PremiumBadge variant="primary" className="mb-2">
              برای شما
            </PremiumBadge>
            <h2 id="personalized-hero-title" className="text-xl font-extrabold text-[color:var(--pub-fg)] sm:text-2xl">
              تجربه شخصی‌سازی‌شده
            </h2>
          </div>
          <GlowButton href="/learning" variant="secondary" className="!px-3 !py-2 text-[12px]">
            هاب یادگیری
          </GlowButton>
        </div>

        {state === "loading" ? (
          <p className="text-sm text-[color:var(--pub-muted)]" role="status">
            در حال بارگذاری پروفایل یادگیری...
          </p>
        ) : null}

        {state === "error" ? (
          <SmartEmptyState
            title="دریافت داده شخصی ممکن نشد"
            description="اتصال را بررسی کنید یا بعداً دوباره تلاش کنید. هیچ داده ساختگی نمایش داده نمی‌شود."
            ctaLabel="رفتن به یادگیری"
            ctaHref="/learning"
            badge="Sync"
          />
        ) : null}

        {state === "ready" ? (
          <div className="grid gap-4 lg:grid-cols-3">
            <GlassCard className="p-4">
              <p className="mb-2 text-[11px] font-bold text-[color:var(--pub-secondary)]">پروفایل یادگیری</p>
              {hasProfile ? (
                <dl className="space-y-2 text-[13px] text-[color:var(--pub-muted)]">
                  {profile?.experienceLevel ? (
                    <div>
                      <dt className="text-[11px] text-[color:var(--pub-muted)]">سطح</dt>
                      <dd className="font-semibold text-[color:var(--pub-fg)]">{profile.experienceLevel}</dd>
                    </div>
                  ) : null}
                  {profile?.learningGoals ? (
                    <div>
                      <dt className="text-[11px] text-[color:var(--pub-muted)]">هدف</dt>
                      <dd className="line-clamp-3 font-semibold text-[color:var(--pub-fg)]">
                        {profile.learningGoals}
                      </dd>
                    </div>
                  ) : null}
                </dl>
              ) : (
                <SmartEmptyState
                  className="!border-0 !bg-transparent !p-0 !shadow-none"
                  title="پروفایل هنوز کامل نیست"
                  description="با چند اطلاعات ساده، HelpDev مسیر مناسب شما را پیدا می‌کند"
                  ctaLabel="تکمیل پروفایل"
                  ctaHref="/learning/profile"
                  badge="Profile"
                />
              )}
            </GlassCard>

            <GlassCard className="p-4">
              <p className="mb-2 text-[11px] font-bold text-[color:var(--pub-secondary)]">پیشنهادها</p>
              {hasRecs ? (
                <ul className="space-y-2">
                  {recommendationItems.slice(0, 3).map((item, index) => (
                    <li key={`${item.kind}-${item.courseId ?? item.slug ?? index}`}>
                      <Link
                        href={item.slug ? `/courses?slug=${encodeURIComponent(item.slug)}` : "/learning"}
                        className="focus-ring block rounded-lg px-2 py-1.5 text-[13px] font-semibold text-[color:var(--pub-fg)] hover:bg-white/[0.04]"
                      >
                        {item.title}
                        {item.rationale ? (
                          <span className="mt-0.5 block text-[11px] font-normal text-[color:var(--pub-muted)] line-clamp-2">
                            {item.rationale}
                          </span>
                        ) : null}
                      </Link>
                    </li>
                  ))}
                </ul>
              ) : (
                <SmartEmptyState
                  className="!border-0 !bg-transparent !p-0 !shadow-none"
                  title="پیشنهادی آماده نیست"
                  description="با چند اطلاعات ساده، HelpDev مسیر مناسب شما را پیدا می‌کند"
                  ctaLabel="تنظیم ترجیحات"
                  ctaHref="/learning/profile"
                  badge="Recs"
                />
              )}
            </GlassCard>

            <GlassCard className="p-4">
              <p className="mb-2 text-[11px] font-bold text-[color:var(--pub-secondary)]">وضعیت نقشه راه</p>
              {hasRoadmap && roadmap ? (
                <div className="space-y-2 text-[13px]">
                  <p className="font-bold text-[color:var(--pub-fg)]">{roadmap.goal || "نقشه راه شما"}</p>
                  <p className="text-[color:var(--pub-muted)]">وضعیت: {roadmap.status}</p>
                  <p className="text-[color:var(--pub-muted)]">
                    {(roadmap.steps?.length ?? 0).toLocaleString("fa-IR")} گام
                  </p>
                  <GlowButton href="/learning/assistant" variant="secondary" className="!mt-2 !px-3 !py-1.5 text-[12px]">
                    ادامه مسیر
                  </GlowButton>
                </div>
              ) : (
                <SmartEmptyState
                  className="!border-0 !bg-transparent !p-0 !shadow-none"
                  title="مسیر یادگیری شما هنوز ساخته نشده"
                  description="با دستیار AI یک نقشه راه شخصی بسازید."
                  ctaLabel="ساخت مسیر با AI"
                  ctaHref="/learning/assistant"
                  badge="Roadmap"
                />
              )}
            </GlassCard>
          </div>
        ) : null}
      </PublicContainer>
    </section>
  );
}
