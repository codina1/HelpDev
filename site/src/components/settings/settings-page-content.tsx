"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import { PageErrorState } from "@/components/ui/page-error-state";
import { PageLoadingState } from "@/components/ui/page-loading-state";
import {
  fetchLearningProfile,
  updateLearningProfile,
  LEARNING_TOPIC_OPTIONS,
  type LearningProfileDto,
} from "@/lib/api/learning-personalization";

type SectionKey = "profile" | "learning" | "ai" | "security";

export function SettingsPageContent() {
  const { token, user, isReady, refreshProfile, saveProfile } = useAuth();
  const [section, setSection] = useState<SectionKey>("profile");
  const [learningProfile, setLearningProfile] = useState<LearningProfileDto | null>(null);
  const [loadingLearning, setLoadingLearning] = useState(false);
  const [learningError, setLearningError] = useState<unknown>(null);
  const [profileForm, setProfileForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    expertise: "",
    interests: "",
  });
  const [profileSaving, setProfileSaving] = useState(false);
  const [profileMessage, setProfileMessage] = useState<string | null>(null);
  const [profileError, setProfileError] = useState<unknown>(null);
  const [learningSaving, setLearningSaving] = useState(false);
  const [learningMessage, setLearningMessage] = useState<string | null>(null);

  useEffect(() => {
    if (!user) return;
    setProfileForm({
      firstName: user.firstName ?? "",
      lastName: user.lastName ?? "",
      email: user.email ?? "",
      expertise: user.expertise ?? "",
      interests: user.interests ?? "",
    });
  }, [user]);

  const loadLearning = useCallback(async () => {
    if (!token) return;
    setLoadingLearning(true);
    setLearningError(null);
    try {
      setLearningProfile(await fetchLearningProfile(token));
    } catch (err) {
      setLearningError(err);
    } finally {
      setLoadingLearning(false);
    }
  }, [token]);

  useEffect(() => {
    if (section === "learning" && token) {
      void loadLearning();
    }
  }, [section, token, loadLearning]);

  if (!isReady) {
    return (
      <div className="mx-auto max-w-3xl px-4 py-10">
        <PageLoadingState rows={3} />
      </div>
    );
  }

  if (!user || !token) {
    return (
      <div className="mx-auto max-w-lg px-4 py-16">
        <PageEmptyState
          title="برای تنظیمات وارد شوید"
          action={
            <Link href="/" className="text-sm font-semibold text-violet-300">
              ورود
            </Link>
          }
        />
      </div>
    );
  }

  async function onSaveProfile(event: React.FormEvent) {
    event.preventDefault();
    setProfileSaving(true);
    setProfileError(null);
    setProfileMessage(null);
    try {
      await saveProfile({
        firstName: profileForm.firstName,
        lastName: profileForm.lastName,
        email: profileForm.email,
        profileImageUrl: user!.profileImageUrl ?? "",
        expertise: profileForm.expertise,
        interests: profileForm.interests,
      });
      await refreshProfile();
      setProfileMessage("پروفایل ذخیره شد.");
    } catch (err) {
      setProfileError(err);
    } finally {
      setProfileSaving(false);
    }
  }

  async function onSaveLearning(event: React.FormEvent) {
    event.preventDefault();
    if (!learningProfile || !token) return;
    setLearningSaving(true);
    setLearningMessage(null);
    setLearningError(null);
    try {
      const updated = await updateLearningProfile(token, {
        experienceLevel: learningProfile.experienceLevel || "Beginner",
        learningGoals: learningProfile.learningGoals,
        currentSkills: learningProfile.currentSkills,
        preferredTopics: learningProfile.preferredTopics,
      });
      setLearningProfile(updated);
      setLearningMessage("ترجیحات یادگیری ذخیره شد.");
    } catch (err) {
      setLearningError(err);
    } finally {
      setLearningSaving(false);
    }
  }

  const tabs: Array<{ key: SectionKey; label: string }> = [
    { key: "profile", label: "پروفایل" },
    { key: "learning", label: "ترجیحات یادگیری" },
    { key: "ai", label: "ترجیحات AI" },
    { key: "security", label: "امنیت" },
  ];

  return (
    <div className="mx-auto max-w-3xl space-y-6 px-4 py-10" dir="rtl">
      <header>
        <h1 className="text-2xl font-extrabold text-white">تنظیمات</h1>
        <p className="mt-1 text-sm text-slate-400">مدیریت پروفایل و ترجیحات — بدون ذخیره جعلی.</p>
      </header>

      <nav className="flex flex-wrap gap-2" aria-label="بخش‌های تنظیمات">
        {tabs.map((tab) => (
          <button
            key={tab.key}
            type="button"
            onClick={() => setSection(tab.key)}
            className={[
              "focus-ring rounded-xl px-3 py-2 text-xs font-semibold transition",
              section === tab.key
                ? "bg-violet-500/25 text-violet-100"
                : "bg-white/5 text-slate-300 hover:bg-white/10",
            ].join(" ")}
            aria-current={section === tab.key ? "page" : undefined}
          >
            {tab.label}
          </button>
        ))}
      </nav>

      {section === "profile" ? (
        <form
          onSubmit={onSaveProfile}
          className="space-y-4 rounded-2xl border border-white/10 bg-white/[0.03] p-5"
        >
          <Field
            label="نام"
            value={profileForm.firstName}
            onChange={(v) => setProfileForm((s) => ({ ...s, firstName: v }))}
          />
          <Field
            label="نام خانوادگی"
            value={profileForm.lastName}
            onChange={(v) => setProfileForm((s) => ({ ...s, lastName: v }))}
          />
          <Field
            label="ایمیل"
            value={profileForm.email}
            onChange={(v) => setProfileForm((s) => ({ ...s, email: v }))}
          />
          <Field
            label="تخصص"
            value={profileForm.expertise}
            onChange={(v) => setProfileForm((s) => ({ ...s, expertise: v }))}
          />
          <Field
            label="علایق"
            value={profileForm.interests}
            onChange={(v) => setProfileForm((s) => ({ ...s, interests: v }))}
          />
          {profileError ? <PageErrorState error={profileError} /> : null}
          {profileMessage ? <p className="text-sm text-emerald-300">{profileMessage}</p> : null}
          <button
            type="submit"
            disabled={profileSaving}
            className="focus-ring rounded-xl bg-violet-500/25 px-4 py-2 text-sm font-semibold text-violet-100 disabled:opacity-60"
          >
            {profileSaving ? "در حال ذخیره..." : "ذخیره پروفایل"}
          </button>
        </form>
      ) : null}

      {section === "learning" ? (
        loadingLearning ? (
          <PageLoadingState rows={3} />
        ) : learningError && !learningProfile ? (
          <PageErrorState error={learningError} onRetry={() => void loadLearning()} />
        ) : learningProfile ? (
          <form
            onSubmit={onSaveLearning}
            className="space-y-4 rounded-2xl border border-white/10 bg-white/[0.03] p-5"
          >
            <label className="block space-y-1 text-sm">
              <span className="text-slate-300">سطح تجربه</span>
              <select
                className="focus-ring w-full rounded-xl border border-white/10 bg-[#121826] px-3 py-2 text-white"
                value={learningProfile.experienceLevel}
                onChange={(e) =>
                  setLearningProfile((prev) =>
                    prev ? { ...prev, experienceLevel: e.target.value } : prev,
                  )
                }
              >
                {["Beginner", "Intermediate", "Advanced"].map((level) => (
                  <option key={level} value={level}>
                    {level}
                  </option>
                ))}
              </select>
            </label>
            <Field
              label="اهداف یادگیری"
              value={learningProfile.learningGoals ?? ""}
              onChange={(v) =>
                setLearningProfile((prev) => (prev ? { ...prev, learningGoals: v } : prev))
              }
            />
            <Field
              label="مهارت‌های فعلی"
              value={learningProfile.currentSkills ?? ""}
              onChange={(v) =>
                setLearningProfile((prev) => (prev ? { ...prev, currentSkills: v } : prev))
              }
            />
            <p className="text-[12px] text-slate-400">
              موضوعات پیشنهادی: {LEARNING_TOPIC_OPTIONS.join("، ")}
            </p>
            {learningError ? <PageErrorState error={learningError} /> : null}
            {learningMessage ? <p className="text-sm text-emerald-300">{learningMessage}</p> : null}
            <button
              type="submit"
              disabled={learningSaving}
              className="focus-ring rounded-xl bg-violet-500/25 px-4 py-2 text-sm font-semibold text-violet-100 disabled:opacity-60"
            >
              {learningSaving ? "در حال ذخیره..." : "ذخیره ترجیحات یادگیری"}
            </button>
          </form>
        ) : (
          <PageEmptyState title="پروفایل یادگیری یافت نشد" />
        )
      ) : null}

      {section === "ai" ? (
        <UnavailableCard
          title="ترجیحات AI هنوز در دسترس نیست"
          description="API اختصاصی برای ذخیره ترجیحات AI کاربر وجود ندارد. از دستیار یادگیری برای پیشنهادها استفاده کنید — بدون ذخیره جعلی."
          href="/learning/assistant"
          linkLabel="دستیار یادگیری"
        />
      ) : null}

      {section === "security" ? (
        <UnavailableCard
          title="تنظیمات امنیتی محصول هنوز آماده نیست"
          description="مدیریت نشست‌ها یا دستگاه‌ها از طریق API عمومی فعلی پشتیبانی نمی‌شود. ورود با OTP موبایل انجام می‌شود."
        />
      ) : null}
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
}) {
  return (
    <label className="block space-y-1 text-sm">
      <span className="text-slate-300">{label}</span>
      <input
        className="focus-ring w-full rounded-xl border border-white/10 bg-[#121826] px-3 py-2 text-white"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
    </label>
  );
}

function UnavailableCard({
  title,
  description,
  href,
  linkLabel,
}: {
  title: string;
  description: string;
  href?: string;
  linkLabel?: string;
}) {
  return (
    <div className="rounded-2xl border border-amber-500/20 bg-amber-500/5 p-5" role="status">
      <h2 className="text-[15px] font-bold text-amber-100">{title}</h2>
      <p className="mt-2 text-[13px] leading-6 text-slate-300">{description}</p>
      {href && linkLabel ? (
        <Link href={href} className="mt-3 inline-flex text-sm font-semibold text-violet-300">
          {linkLabel}
        </Link>
      ) : null}
    </div>
  );
}
