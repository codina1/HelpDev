"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { AiLearningCard } from "@/components/learning/ai-learning-card";
import { RecommendationList } from "@/components/learning/recommendation-list";
import { RoadmapView } from "@/components/learning/roadmap-view";
import {
  approveLearningRoadmap,
  fetchLearningRecommendations,
  fetchLearningRoadmap,
  generateLearningRoadmap,
  type LearningRecommendationDto,
  type LearningRoadmapDto,
} from "@/lib/api/learning-personalization";
import { ApiClientError } from "@/lib/api/errors";

export default function LearningAssistantPage() {
  const { token, user, isReady } = useAuth();
  const [recommendations, setRecommendations] = useState<LearningRecommendationDto | null>(null);
  const [roadmap, setRoadmap] = useState<LearningRoadmapDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const [recs, map] = await Promise.all([
        fetchLearningRecommendations(token),
        fetchLearningRoadmap(token).catch(() => null),
      ]);
      setRecommendations(recs);
      setRoadmap(map);
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "بارگذاری دستیار ناموفق بود.");
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void load();
  }, [load]);

  if (!isReady) {
    return <p className="p-8 text-center text-slate-400">در حال بارگذاری...</p>;
  }

  if (!user || !token) {
    return (
      <div className="mx-auto max-w-lg px-4 py-16 text-center" dir="rtl">
        <p className="text-white">برای استفاده از دستیار یادگیری وارد شوید.</p>
        <Link href="/" className="mt-4 inline-block text-emerald-300">
          بازگشت به خانه
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6 px-4 py-10" dir="rtl">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-extrabold text-white">دستیار یادگیری AI</h1>
          <p className="mt-1 text-sm text-slate-400">
            فقط پیشنهاد می‌دهد — ثبت‌نام، پیشرفت یا پروفایل را تغییر نمی‌دهد.
          </p>
        </div>
        <Link href="/learning/profile" className="text-sm text-emerald-300">
          پروفایل یادگیری
        </Link>
      </div>

      {error ? <p className="text-sm text-rose-300">{error}</p> : null}
      {loading ? <p className="text-slate-400">در حال بارگذاری...</p> : null}

      <AiLearningCard
        title="پیشنهاد مسیر"
        action={
          <button
            type="button"
            disabled={busy}
            onClick={() => void load()}
            className="rounded-lg bg-white/10 px-3 py-1.5 text-xs text-white"
          >
            بروزرسانی
          </button>
        }
      >
        <RecommendationList data={recommendations} />
      </AiLearningCard>

      <AiLearningCard
        title="نقشه راه شخصی"
        action={
          <button
            type="button"
            disabled={busy}
            onClick={async () => {
              setBusy(true);
              setError(null);
              try {
                setRoadmap(await generateLearningRoadmap(token));
              } catch (err) {
                setError(err instanceof ApiClientError ? err.message : "تولید نقشه راه ناموفق بود.");
              } finally {
                setBusy(false);
              }
            }}
            className="rounded-lg bg-emerald-600 px-3 py-1.5 text-xs font-bold text-white"
          >
            تولید پیشنهاد
          </button>
        }
      >
        <RoadmapView
          roadmap={roadmap}
          approving={busy}
          onApprove={async () => {
            setBusy(true);
            setError(null);
            try {
              setRoadmap(await approveLearningRoadmap(token));
            } catch (err) {
              setError(err instanceof ApiClientError ? err.message : "تأیید نقشه راه ناموفق بود.");
            } finally {
              setBusy(false);
            }
          }}
        />
      </AiLearningCard>
    </div>
  );
}
