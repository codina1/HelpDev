"use client";

import { useMemo, useState } from "react";
import { CourseCard } from "@/components/courses/course-card";
import { CoursesCategoryBar } from "@/components/courses/courses-category-bar";
import { CoursesContainer } from "@/components/courses/courses-container";
import {
  CoursesFiltersSidebar,
  DEFAULT_COURSES_FILTERS,
  type CoursesFiltersState,
} from "@/components/courses/courses-filters-sidebar";
import {
  COURSES_TOTAL_COUNT,
  COURSES_TOTAL_PAGES,
  type CourseCategoryFilter,
} from "@/data/courses";
import type { Course } from "@/types";

type CoursesCatalogProps = {
  courses: Course[];
};

const SORT_OPTIONS = [
  { id: "newest", label: "جدیدترین" },
  { id: "popular", label: "محبوب‌ترین" },
  { id: "cheapest", label: "ارزان‌ترین" },
  { id: "longest", label: "طولانی‌ترین" },
] as const;

type SortId = (typeof SORT_OPTIONS)[number]["id"];

function FilterIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M4 6.5h16M7 12h10M10 17.5h4" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function toPersianDigits(value: number): string {
  return value.toLocaleString("fa-IR", { useGrouping: false });
}

export function CoursesCatalog({ courses }: CoursesCatalogProps) {
  const [category, setCategory] = useState<CourseCategoryFilter>("همه");
  const [filters, setFilters] = useState<CoursesFiltersState>(DEFAULT_COURSES_FILTERS);
  const [sort, setSort] = useState<SortId>("newest");
  const [page, setPage] = useState(1);
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const visible = useMemo(() => {
    const query = filters.query.trim().toLowerCase();
    const next = courses.filter((course) => {
      if (category !== "همه" && !course.categories.includes(category)) return false;
      if (filters.level !== "all" && course.level !== filters.level) return false;
      if (filters.price === "free" && course.price !== 0) return false;
      if (filters.price === "paid" && course.price === 0) return false;
      if (filters.price === "featured" && !course.isNew) return false;
      if (course.durationHours > filters.maxHours) return false;
      if (query && !`${course.title} ${course.description}`.toLowerCase().includes(query)) {
        return false;
      }
      return true;
    });

    switch (sort) {
      case "popular":
        return next.slice().sort((a, b) => b.rating - a.rating);
      case "cheapest":
        return next.slice().sort((a, b) => a.price - b.price);
      case "longest":
        return next.slice().sort((a, b) => b.durationHours - a.durationHours);
      default:
        return next;
    }
  }, [category, courses, filters, sort]);

  const isPristine =
    category === "همه" &&
    filters.query === DEFAULT_COURSES_FILTERS.query &&
    filters.level === DEFAULT_COURSES_FILTERS.level &&
    filters.price === DEFAULT_COURSES_FILTERS.price &&
    filters.maxHours === DEFAULT_COURSES_FILTERS.maxHours;

  const totalLabel = isPristine ? COURSES_TOTAL_COUNT : visible.length;

  function reset() {
    setFilters(DEFAULT_COURSES_FILTERS);
    setCategory("همه");
    setPage(1);
  }

  return (
    <section id="courses-grid" className="bg-[#030712] pb-7 pt-2" dir="rtl">
      <CoursesContainer>
        <div className="mb-7">
          <CoursesCategoryBar
            active={category}
            onSelect={(value) => {
              setCategory(value);
              setPage(1);
            }}
          />
        </div>

        <div className="grid items-start gap-6 lg:grid-cols-[minmax(0,1fr)_240px] lg:gap-8">
          <div className="order-1 lg:order-2">
            <button
              type="button"
              onClick={() => setMobileFiltersOpen((open) => !open)}
              aria-expanded={mobileFiltersOpen}
              className="mb-3 inline-flex h-10 w-full items-center justify-center gap-2 rounded-xl border border-white/[0.1] bg-[#0F1626] text-[12px] font-bold text-[#E5E7EB] lg:hidden"
            >
              <FilterIcon className="h-4 w-4 text-[#A78BFA]" />
              فیلترها
            </button>
            <div className={[mobileFiltersOpen ? "block" : "hidden", "lg:sticky lg:top-24 lg:block"].join(" ")}>
              <CoursesFiltersSidebar
                value={filters}
                onChange={(next) => {
                  setFilters(next);
                  setPage(1);
                }}
                onReset={reset}
              />
            </div>
          </div>

          <div className="order-2 min-w-0 lg:order-1">
            <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
              <p className="text-[13.5px] font-semibold text-[#94A3B8]">
                تعداد دوره‌ها:{" "}
                <span className="bg-gradient-to-l from-[#C084FC] to-[#3B82F6] bg-clip-text font-extrabold text-transparent">
                  {toPersianDigits(totalLabel)} دوره
                </span>
              </p>

              <label className="inline-flex items-center gap-2 text-[13px] text-[#64748B]">
                مرتب‌سازی:
                <select
                  value={sort}
                  onChange={(event) => setSort(event.target.value as SortId)}
                  aria-label="مرتب‌سازی دوره‌ها"
                  className="h-10 rounded-lg border border-white/[0.1] bg-[#0F1626] px-3 text-[13px] font-semibold text-[#E5E7EB] outline-none transition focus:border-[rgba(168,85,247,0.45)]"
                >
                  {SORT_OPTIONS.map((option) => (
                    <option key={option.id} value={option.id}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </label>
            </div>

            {visible.length > 0 ? (
              <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-4">
                {visible.map((course) => (
                  <CourseCard key={course.id} course={course} />
                ))}
              </div>
            ) : (
              <div className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[13px] text-[#94A3B8]">
                دوره‌ای با این فیلترها پیدا نشد.
              </div>
            )}

            <CoursesPagination
              currentPage={page}
              totalPages={COURSES_TOTAL_PAGES}
              onPageChange={setPage}
            />
          </div>
        </div>
      </CoursesContainer>
    </section>
  );
}

function CoursesPagination({
  currentPage,
  totalPages,
  onPageChange,
}: {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  const pages = [1, 2, 3, 4];

  return (
    <nav
      className="flex flex-wrap items-center justify-center gap-1.5 pt-7"
      aria-label="صفحه‌بندی دوره‌ها"
      dir="ltr"
    >
      <button
        type="button"
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-white/[0.08] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
        onClick={() => onPageChange(Math.max(1, currentPage - 1))}
        disabled={currentPage === 1}
        aria-label="صفحه قبلی"
      >
        ‹
      </button>
      {pages.map((page) => (
        <button
          key={page}
          type="button"
          aria-current={page === currentPage ? "page" : undefined}
          onClick={() => onPageChange(page)}
          className={[
            "inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-2 text-[13px] font-semibold transition",
            page === currentPage
              ? "bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_16px_rgba(124,58,237,0.3)]"
              : "text-[#94A3B8] hover:text-white",
          ].join(" ")}
        >
          {page}
        </button>
      ))}
      <span className="px-1 text-[13px] text-[#64748B]">…</span>
      <button
        type="button"
        className="inline-flex h-9 min-w-9 items-center justify-center rounded-lg px-2 text-[13px] font-semibold text-[#94A3B8] transition hover:text-white"
        onClick={() => onPageChange(totalPages)}
      >
        {totalPages}
      </button>
      <button
        type="button"
        className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-white/[0.08] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
        onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
        disabled={currentPage >= totalPages}
        aria-label="صفحه بعدی"
      >
        ›
      </button>
    </nav>
  );
}
