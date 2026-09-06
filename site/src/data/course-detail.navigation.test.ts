import { describe, expect, it } from "vitest";
import { getCourseDetailBySlug } from "@/data/course-detail";
import { COURSES, publicCoursePath } from "@/data/courses";

describe("course catalog → detail navigation", () => {
  it("resolves every catalog course to its own detail title", () => {
    for (const course of COURSES) {
      const detail = getCourseDetailBySlug(course.slug);
      expect(detail).not.toBeNull();
      expect(detail?.slug).toBe(course.slug);
      expect(detail?.title).toBe(
        course.slug === "react-19" ? "دوره جامع React" : course.title,
      );
      expect(publicCoursePath(course.slug)).toBe(`/courses/${course.slug}`);
    }
  });

  it("returns null for unknown slugs", () => {
    expect(getCourseDetailBySlug("does-not-exist")).toBeNull();
  });

  it("keeps react-19 rich detail aliases", () => {
    expect(getCourseDetailBySlug("react-19-complete")?.slug).toBe("react-19");
  });
});
