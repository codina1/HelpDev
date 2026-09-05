"use client";

import { useEffect, useState } from "react";
import { ArticlesContainer } from "@/components/articles/articles-container";
import { CourseBreadcrumb } from "@/components/course/CourseBreadcrumb";
import { CourseDescription } from "@/components/course/CourseDescription";
import { CourseHero } from "@/components/course/CourseHero";
import { CourseTabs } from "@/components/course/CourseTabs";
import { CurriculumAccordion } from "@/components/course/CurriculumAccordion";
import { InstructorCard } from "@/components/course/InstructorCard";
import { LearningSection } from "@/components/course/LearningSection";
import { PurchaseCard } from "@/components/course/PurchaseCard";
import { RelatedCourses, Reviews, CourseProjects } from "@/components/course/RelatedCourses";
import { Requirements } from "@/components/course/Requirements";
import type { CourseDetailModel, CourseDetailTabId } from "@/data/course-detail";

type CourseDetailViewProps = {
  course: CourseDetailModel;
};

const TAB_TO_ID: Record<CourseDetailTabId, string> = {
  about: "about",
  curriculum: "curriculum",
  instructor: "instructor",
  reviews: "reviews",
  projects: "projects",
  requirements: "requirements",
};

export function CourseDetailView({ course }: CourseDetailViewProps) {
  const [activeTab, setActiveTab] = useState<CourseDetailTabId>("about");

  useEffect(() => {
    const ids = Object.values(TAB_TO_ID);
    const nodes = ids
      .map((id) => document.getElementById(id))
      .filter((node): node is HTMLElement => Boolean(node));
    if (nodes.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((entry) => entry.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio);
        const id = visible[0]?.target?.id;
        if (!id) return;
        const tab = (Object.entries(TAB_TO_ID).find(([, value]) => value === id)?.[0] ??
          "about") as CourseDetailTabId;
        setActiveTab(tab);
      },
      { rootMargin: "-25% 0px -55% 0px", threshold: [0.15, 0.4] },
    );

    nodes.forEach((node) => observer.observe(node));
    return () => observer.disconnect();
  }, []);

  function onTabChange(tab: CourseDetailTabId) {
    setActiveTab(tab);
    const el = document.getElementById(TAB_TO_ID[tab]);
    el?.scrollIntoView({ behavior: "smooth", block: "start" });
  }

  function onPreview() {
    document.getElementById("curriculum")?.scrollIntoView({ behavior: "smooth" });
  }

  return (
    <div className="bg-[#050816] pb-12 pt-4">
      <ArticlesContainer>
        <div className="mb-5">
          <CourseBreadcrumb items={course.breadcrumb} />
        </div>

        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[minmax(0,1fr)_300px] xl:gap-7"
        >
          <div className="min-w-0 space-y-6" dir="rtl">
            <CourseHero course={course} onPreview={onPreview} />
            <CourseTabs active={activeTab} onChange={onTabChange} />
            <CourseDescription course={course} />
            <LearningSection course={course} />
            <CurriculumAccordion sections={course.curriculum} />
            <InstructorCard course={course} />
            <Requirements course={course} />
            <CourseProjects projects={course.projects} />
            <Reviews reviews={course.reviews} rating={course.rating} />
          </div>

          <aside className="space-y-4 xl:sticky xl:top-20 xl:self-start" dir="rtl">
            <PurchaseCard course={course} />
            <div className="hidden xl:block">
              <RelatedCourses courses={course.related} />
            </div>
          </aside>
        </div>

        <div className="mt-8 xl:hidden" dir="rtl">
          <RelatedCourses courses={course.related} />
        </div>
      </ArticlesContainer>
    </div>
  );
}
