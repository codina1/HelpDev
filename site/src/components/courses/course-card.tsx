"use client";

import { useState } from "react";
import type { Course } from "@/types";

type CourseCardProps = {
  course: Course;
};

export function CourseCard({ course }: CourseCardProps) {
  const [saved, setSaved] = useState(false);

  return (
    <article className="ui-card flex h-full flex-col p-5">
      <div className="mb-4 flex flex-wrap items-center gap-2.5">
        <span className="ui-badge">{course.category}</span>
        <span className="ui-meta">{course.level}</span>
      </div>

      <h2 className="ui-heading flex-1">{course.title}</h2>

      <div className="mt-4 flex items-center justify-between gap-3">
        <span className="ui-meta">{course.platform}</span>
        <span className="inline-flex items-center gap-1 text-sm font-semibold text-foreground">
          <StarIcon />
          {course.rating.toFixed(1)}
        </span>
      </div>

      <div className="mt-5 flex gap-2.5">
        <button type="button" className="ui-btn ui-btn-primary flex-1 px-3 py-2.5">
          مشاهده
        </button>
        <button
          type="button"
          onClick={() => setSaved((value) => !value)}
          aria-pressed={saved}
          className={[
            "ui-btn flex-1 px-3 py-2.5",
            saved ? "ui-btn-active" : "ui-btn-secondary",
          ].join(" ")}
        >
          {saved ? "ذخیره شد" : "ذخیره"}
        </button>
      </div>
    </article>
  );
}

function StarIcon() {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 24 24"
      fill="currentColor"
      aria-hidden
      className="text-accent drop-shadow-[0_0_8px_rgba(34,211,238,0.6)]"
    >
      <path d="M12 2.5l2.9 6.1 6.6.9-4.8 4.6 1.2 6.5L12 17.8 6.1 20.6l1.2-6.5L2.5 9.5l6.6-.9L12 2.5z" />
    </svg>
  );
}
