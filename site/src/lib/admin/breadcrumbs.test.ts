import { describe, expect, it } from "vitest";
import { buildAdminBreadcrumbs } from "./breadcrumbs";

describe("buildAdminBreadcrumbs", () => {
  it("collapses the dashboard to a single, non-clickable crumb", () => {
    const crumbs = buildAdminBreadcrumbs("/admin");
    expect(crumbs).toHaveLength(1);
    expect(crumbs[0].current).toBe(true);
    expect(crumbs[0].href).toBeUndefined();
  });

  it("builds root + leaf when the leaf is the group landing (platform hub)", () => {
    const crumbs = buildAdminBreadcrumbs("/admin/content");
    expect(crumbs.map((c) => c.title)).toEqual(["مدیریت", "پلتفرم محتوا"]);
    expect(crumbs[0].href).toBe("/admin");
    expect(crumbs.at(-1)?.current).toBe(true);
  });

  it("includes the group crumb for a workspace nested route", () => {
    const crumbs = buildAdminBreadcrumbs("/admin/content/articles/new");
    expect(crumbs.map((c) => c.title)).toEqual([
      "مدیریت",
      "محتوا",
      "مقالات",
    ]);
    expect(crumbs[1].href).toBe("/admin/content");
    expect(crumbs[1].current).toBe(false);
    expect(crumbs.at(-1)?.current).toBe(true);
  });

  it("falls back gracefully for unknown routes", () => {
    const crumbs = buildAdminBreadcrumbs("/admin/does-not-exist");
    expect(crumbs[0].title).toBe("مدیریت");
    expect(crumbs.at(-1)?.current).toBe(true);
  });
});
