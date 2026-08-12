"use client";

import { useMemo, useState } from "react";
import { CourseCard } from "@/components/courses/course-card";
import { COURSE_CATEGORIES } from "@/data/courses";
import type { Course, CourseCategory } from "@/types";

type CoursesGridProps = {
  courses: Course[];
};

type FilterValue = "همه" | CourseCategory;

const FILTERS: FilterValue[] = ["همه", ...COURSE_CATEGORIES];

export function CoursesGrid({ courses }: CoursesGridProps) {
  const [filter, setFilter] = useState<FilterValue>("همه");

  const visible = useMemo(() => {
    if (filter === "همه") return courses;
    return courses.filter((course) => course.category === filter);
  }, [courses, filter]);

  return (
    <div className="space-y-5">
      <div
        className="flex flex-wrap gap-2"
        role="tablist"
        aria-label="Filter by category"
      >
        {FILTERS.map((item) => {
          const isActive = filter === item;

          return (
            <button
              key={item}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => setFilter(item)}
              className={[
                "ui-chip px-3.5 py-1.5",
                isActive ? "ui-chip-active" : "",
              ].join(" ")}
            >
              {item}
            </button>
          );
        })}
      </div>

      <p className="ui-meta">
        {visible.length} دوره
        {filter !== "همه" ? ` در ${filter}` : ""}
      </p>

      {visible.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
          {visible.map((course) => (
            <CourseCard key={course.id} course={course} />
          ))}
        </div>
      ) : (
        <div className="ui-panel border-dashed px-4 py-12 text-center">
          <p className="ui-body">دوره‌ای برای این دسته پیدا نشد.</p>
        </div>
      )}
    </div>
  );
}
