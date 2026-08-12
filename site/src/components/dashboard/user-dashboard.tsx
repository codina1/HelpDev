"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { CourseCard } from "@/components/learning/course-card";
import { ProgressCard } from "@/components/learning/progress-card";
import { RecommendationCard } from "@/components/learning/recommendation-card";
import { RoadmapCard } from "@/components/learning/roadmap-card";
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
import { getUserDisplayName } from "@/types/auth";

type DashboardData = {
  enrollments: EnrollmentListItemDto[];
  coursesById: Map<string, CourseSummaryDto>;
  recommendations: LearningRecommendationDto | null;
  roadmap: LearningRoadmapDto | null;
};

export function UserDashboard() {
  const { token, user, isReady } = useAuth();
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);

  const load = useCallback(async () => {
    if (!token) {
      setLoading(false);
      setData(null);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const [enrollments, courses, recommendations, roadmap] = await Promise.all([
        listMyEnrollments(token),
        listCourses().catch(() => [] as CourseSummaryDto[]),
        fetchLearningRecommendations(token).catch(() => null),
        fetchLearningRoadmap(token).catch(() => null),
      ]);

      const coursesById = new Map(courses.map((course) => [course.id, course]));
      setData({ enrollments, coursesById, recommendations, roadmap });
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
          title="برای مشاهده داشبورد وارد شوید"
          description="پس از ورود، پیشرفت یادگیری و پیشنهادهای شما اینجا نمایش داده می‌شود."
          action={
            <Link href="/" className="text-sm font-semibold text-violet-300">
              بازگشت به خانه و ورود
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

  const enrollments = data?.enrollments ?? [];
  const inProgress = enrollments
    .filter((item) => item.progressPercentage > 0 && item.progressPercentage < 100)
    .sort((a, b) => b.progressPercentage - a.progressPercentage);
  const continueItems = inProgress.length > 0 ? inProgress : enrollments.slice(0, 3);
  const recommendations = data?.recommendations?.recommendedItems.slice(0, 3) ?? [];

  return (
    <div className="mx-auto max-w-5xl space-y-8 px-4 py-10" dir="rtl">
      <section className="rounded-2xl border border-white/10 bg-gradient-to-l from-violet-500/10 to-transparent p-6">
        <h1 className="text-2xl font-extrabold text-white">
          سلام، {getUserDisplayName(user)}
        </h1>
        <p className="mt-2 text-sm text-slate-400">
          خلاصه یادگیری شما بر اساس ثبت‌نام‌ها و پیشنهادهای موجود.
        </p>
        <div className="mt-4 flex flex-wrap gap-2">
          <Link href="/learning" className="focus-ring rounded-xl bg-violet-500/20 px-3 py-2 text-xs font-semibold text-violet-200">
            خانه یادگیری
          </Link>
          <Link href="/learning/assistant" className="focus-ring rounded-xl bg-white/5 px-3 py-2 text-xs font-semibold text-slate-200">
            دستیار AI
          </Link>
          <Link href="/settings" className="focus-ring rounded-xl bg-white/5 px-3 py-2 text-xs font-semibold text-slate-200">
            تنظیمات
          </Link>
          <Link href="/profile" className="focus-ring rounded-xl bg-white/5 px-3 py-2 text-xs font-semibold text-slate-200">
            پروفایل
          </Link>
        </div>
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">پیشرفت یادگیری</h2>
        {enrollments.length === 0 ? (
          <PageEmptyState
            title="هنوز در دوره‌ای ثبت‌نام نکرده‌اید"
            description="از فهرست دوره‌ها یک دوره منتشرشده انتخاب کنید."
            action={
              <Link href="/courses" className="text-sm font-semibold text-violet-300">
                مشاهده دوره‌ها
              </Link>
            }
          />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2">
            {enrollments.slice(0, 4).map((enrollment) => {
              const course = data?.coursesById.get(enrollment.courseId);
              return (
                <ProgressCard
                  key={enrollment.id}
                  title={course?.title ?? `دوره ${enrollment.courseId.slice(0, 8)}`}
                  progressPercentage={enrollment.progressPercentage}
                  status={enrollment.status}
                />
              );
            })}
          </div>
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">ادامه یادگیری</h2>
        {continueItems.length === 0 ? (
          <PageEmptyState title="موردی برای ادامه نیست" description="پس از ثبت‌نام، دوره‌های فعال اینجا ظاهر می‌شوند." />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {continueItems.map((enrollment) => {
              const course = data?.coursesById.get(enrollment.courseId);
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
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-lg font-bold text-white">پیشنهادهای AI</h2>
          <Link href="/learning/assistant" className="text-xs font-semibold text-violet-300">
            همه پیشنهادها
          </Link>
        </div>
        {recommendations.length === 0 ? (
          <PageEmptyState
            title="پیشنهادی آماده نیست"
            description="پروفایل یادگیری را تکمیل کنید تا پیشنهادها دقیق‌تر شوند."
            action={
              <Link href="/learning/profile" className="text-sm font-semibold text-violet-300">
                پروفایل یادگیری
              </Link>
            }
          />
        ) : (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {recommendations.map((item) => (
              <RecommendationCard key={`${item.kind}-${item.courseId ?? item.title}`} item={item} />
            ))}
          </div>
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">نقشه راه</h2>
        {data?.roadmap ? (
          <RoadmapCard roadmap={data.roadmap} />
        ) : (
          <PageEmptyState
            title="نقشه راهی ندارید"
            description="از دستیار یادگیری یک نقشه راه پیشنهادی بسازید."
            action={
              <Link href="/learning/assistant" className="text-sm font-semibold text-violet-300">
                ساخت نقشه راه
              </Link>
            }
          />
        )}
      </section>

      <section className="space-y-3">
        <h2 className="text-lg font-bold text-white">فعالیت اخیر</h2>
        {enrollments.length === 0 ? (
          <PageEmptyState title="فعالیتی ثبت نشده" description="ثبت‌نام و پیشرفت دوره به‌عنوان فعالیت نمایش داده می‌شود." />
        ) : (
          <ul className="space-y-2 rounded-2xl border border-white/10 bg-white/[0.03] p-4">
            {enrollments
              .slice()
              .sort((a, b) => Date.parse(b.enrolledAt) - Date.parse(a.enrolledAt))
              .slice(0, 5)
              .map((enrollment) => {
                const course = data?.coursesById.get(enrollment.courseId);
                return (
                  <li key={enrollment.id} className="flex items-center justify-between gap-3 text-sm">
                    <span className="text-slate-200">
                      {course?.title ?? `دوره ${enrollment.courseId.slice(0, 8)}`}
                    </span>
                    <span className="text-[11px] text-slate-500" dir="ltr">
                      {new Date(enrollment.enrolledAt).toLocaleDateString("fa-IR")}
                    </span>
                  </li>
                );
              })}
          </ul>
        )}
      </section>
    </div>
  );
}
