"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { CourseCard } from "@/components/learning/course-card";
import { RecommendationCard } from "@/components/learning/recommendation-card";
import { RoadmapCard } from "@/components/learning/roadmap-card";
import { ProgressCard } from "@/components/learning/progress-card";
import { PageEmptyState } from "@/components/ui/page-empty-state";
import { PageErrorState } from "@/components/ui/page-error-state";
import { PageLoadingState } from "@/components/ui/page-loading-state";
import { listMyEnrollments, type EnrollmentListItemDto } from "@/lib/api/enrollments";
import { listCourses, type CourseSummaryDto } from "@/lib/api/learning";
import {
  fetchLearningRecommendations,
  fetchLearningRoadmap,
  type LearningRecommendationDto,
  type LearningRoadmapDto,
} from "@/lib/api/learning-personalization";

export default function LearningPage() {
  return <LearningHome />;
}

function LearningHome() {
  const { token, user, isReady } = useAuth();
  const [enrollments, setEnrollments] = useState<EnrollmentListItemDto[]>([]);
  const [coursesById, setCoursesById] = useState<Map<string, CourseSummaryDto>>(new Map());
  const [recommendations, setRecommendations] = useState<LearningRecommendationDto | null>(null);
  const [roadmap, setRoadmap] = useState<LearningRoadmapDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const load = useCallback(async () => {
    if (!token) {
      setLoading(false);
      return;
    }
    setLoading(true);
    setError(null);
    try {
      const [enrollmentRows, courses, recs, map] = await Promise.all([
        listMyEnrollments(token),
        listCourses().catch(() => [] as CourseSummaryDto[]),
        fetchLearningRecommendations(token).catch(() => null),
        fetchLearningRoadmap(token).catch(() => null),
      ]);
      setEnrollments(enrollmentRows);
      setCoursesById(new Map(courses.map((c) => [c.id, c])));
      setRecommendations(recs);
      setRoadmap(map);
    } catch (err) {
      setError(err);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void load();
  }, [load]);

  if (!isReady || loading) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10">
        <PageLoadingState />
      </div>
    );
  }

  if (!user || !token) {
    return (
      <div className="mx-auto max-w-lg px-4 py-16">
        <PageEmptyState
          title="برای یادگیری وارد شوید"
          description="ثبت‌نام دوره‌ها، نقشه راه و پیشنهادها پس از ورود در دسترس است."
          action={
            <Link href="/" className="text-sm font-semibold text-violet-300">
              ورود از صفحه اصلی
            </Link>
          }
        />
      </div>
    );
  }

  if (error) {
    return (
      <div className="mx-auto max-w-5xl px-4 py-10">
        <PageErrorState error={error} onRetry={() => void load()} />
      </div>
    );
  }

  const recent = enrollments
    .slice()
    .sort((a, b) => Date.parse(b.enrolledAt) - Date.parse(a.enrolledAt))
    .slice(0, 4);

  return (
    <div className="mx-auto max-w-5xl space-y-8 px-4 py-10" dir="rtl">
      <header className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-2xl font-extrabold text-white">یادگیری</h1>
          <p className="mt-1 text-sm text-slate-400">
            دوره‌های ثبت‌نام‌شده، نقشه راه و پیشنهادهای شخصی‌سازی‌شده.
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link href="/learning/profile" className="focus-ring rounded-xl bg-white/5 px-3 py-2 text-xs font-semibold text-slate-200">
            پروفایل یادگیری
          </Link>
          <Link href="/learning/assistant" className="focus-ring rounded-xl bg-violet-500/20 px-3 py-2 text-xs font-semibold text-violet-200">
            دستیار AI
          </Link>
        </div>
      </header>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">دوره‌های من</h2>
        {enrollments.length === 0 ? (
          <PageEmptyState
            title="هنوز ثبت‌نامی ندارید"
            description="از کاتالوگ دوره‌های منتشرشده شروع کنید."
            action={<Link href="/courses" className="text-sm font-semibold text-violet-300">کاتالوگ دوره‌ها</Link>}
          />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {enrollments.map((enrollment) => {
              const course = coursesById.get(enrollment.courseId);
              return course ? (
                <CourseCard
                  key={enrollment.id}
                  course={course}
                  progressPercentage={enrollment.progressPercentage}
                  meta={`وضعیت: ${enrollment.status}`}
                />
              ) : (
                <ProgressCard
                  key={enrollment.id}
                  title={`دوره ${enrollment.courseId.slice(0, 8)}`}
                  progressPercentage={enrollment.progressPercentage}
                  status={enrollment.status}
                />
              );
            })}
          </div>
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">نقشه راه</h2>
        {roadmap ? (
          <RoadmapCard roadmap={roadmap} />
        ) : (
          <PageEmptyState
            title="نقشه راهی ثبت نشده"
            description="از دستیار یادگیری یک پیشنهاد بسازید و تأیید کنید."
            action={<Link href="/learning/assistant" className="text-sm font-semibold text-violet-300">دستیار یادگیری</Link>}
          />
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">پیشنهادها</h2>
        {!recommendations || recommendations.recommendedItems.length === 0 ? (
          <PageEmptyState title="پیشنهادی نیست" description={recommendations?.reason ?? "پروفایل یادگیری را تکمیل کنید."} />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2">
            {recommendations.recommendedItems.slice(0, 4).map((item) => (
              <RecommendationCard key={`${item.kind}-${item.courseId ?? item.title}`} item={item} />
            ))}
          </div>
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">آخرین دوره‌ها</h2>
        {recent.length === 0 ? (
          <PageEmptyState title="درس اخیری نیست" description="پس از شروع درس‌ها، اینجا نمایش داده می‌شوند." />
        ) : (
          <ul className="space-y-2 rounded-2xl border border-white/10 bg-white/[0.03] p-4">
            {recent.map((enrollment) => {
              const course = coursesById.get(enrollment.courseId);
              return (
                <li key={enrollment.id} className="flex items-center justify-between gap-3 text-sm">
                  <span className="text-slate-200">{course?.title ?? enrollment.courseId.slice(0, 8)}</span>
                  <span className="text-[11px] text-emerald-300">{enrollment.progressPercentage}٪</span>
                </li>
              );
            })}
          </ul>
        )}
      </section>
    </div>
  );
}
