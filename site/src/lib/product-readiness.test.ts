import { describe, expect, it } from "vitest";
import { existsSync } from "node:fs";
import { join } from "node:path";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { HEADER_NAV } from "@/lib/constants";
import { emptyNotificationFeed } from "@/lib/notifications";
import { CONTENT_STATUSES } from "@/lib/admin/content/content-types";

describe("Sprint 45 — product readiness routes", () => {
  const appRoot = join(process.cwd(), "src", "app");

  it("exposes dashboard, learning home, and settings pages", () => {
    expect(existsSync(join(appRoot, "dashboard", "page.tsx"))).toBe(true);
    expect(existsSync(join(appRoot, "learning", "page.tsx"))).toBe(true);
    expect(existsSync(join(appRoot, "settings", "page.tsx"))).toBe(true);
  });

  it("keeps critical admin navigation routes", () => {
    expect(ADMIN_ROUTES.dashboard).toBe("/admin");
    expect(ADMIN_ROUTES.content).toBe("/admin/content");
    expect(ADMIN_ROUTES.contentWorkflows).toBe("/admin/content/workflows");
    expect(ADMIN_ROUTES.operations).toBe("/admin/operations");
  });

  it("includes learning and dashboard in header nav", () => {
    expect(HEADER_NAV.some((item) => item.href === "/learning")).toBe(true);
    expect(HEADER_NAV.some((item) => item.href === "/dashboard")).toBe(true);
  });
});

describe("Sprint 45 — notification foundation", () => {
  it("starts with an empty feed and zero unread (no fake items)", () => {
    const feed = emptyNotificationFeed();
    expect(feed.items).toEqual([]);
    expect(feed.unreadCount).toBe(0);
  });
});

describe("Sprint 45 — content status visualization", () => {
  it("covers draft through archived statuses", () => {
    expect(CONTENT_STATUSES).toEqual([
      "Draft",
      "ReviewPending",
      "Approved",
      "Published",
      "Archived",
    ]);
  });
});
