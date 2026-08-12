"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { LearningProfileForm } from "@/components/learning/learning-profile-form";
import {
  fetchLearningProfile,
  updateLearningProfile,
  type LearningProfileDto,
} from "@/lib/api/learning-personalization";
import { ApiClientError } from "@/lib/api/errors";

export default function LearningProfilePage() {
  const { token, user, isReady } = useAuth();
  const [profile, setProfile] = useState<LearningProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      setProfile(await fetchLearningProfile(token));
    } catch (err) {
      setError(err instanceof ApiClientError ? err.message : "بارگذاری پروفایل ناموفق بود.");
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
        <p className="text-white">برای مدیریت پروفایل یادگیری وارد شوید.</p>
        <Link href="/" className="mt-4 inline-block text-emerald-300">
          بازگشت به خانه
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10" dir="rtl">
      <div className="mb-6 flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-extrabold text-white">پروفایل یادگیری</h1>
          <p className="mt-1 text-sm text-slate-400">
            شما کنترل می‌کنید؛ AI هرگز این پروفایل را خودکار بازنویسی نمی‌کند.
          </p>
        </div>
        <Link href="/learning/assistant" className="text-sm text-emerald-300">
          دستیار یادگیری
        </Link>
      </div>

      {loading ? (
        <p className="text-slate-400">در حال بارگذاری...</p>
      ) : (
        <LearningProfileForm
          initial={profile}
          saving={saving}
          error={error}
          onSave={async (payload) => {
            setSaving(true);
            setError(null);
            try {
              const updated = await updateLearningProfile(token, payload);
              setProfile(updated);
            } catch (err) {
              setError(err instanceof ApiClientError ? err.message : "ذخیره ناموفق بود.");
            } finally {
              setSaving(false);
            }
          }}
        />
      )}
    </div>
  );
}
