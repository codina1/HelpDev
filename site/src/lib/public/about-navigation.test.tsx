import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicFooter } from "@/components/public/public-footer";
import { ADMIN_NAVIGATION, flattenNavItems } from "@/lib/admin/navigation";
import { findActiveNavItemId, isRouteActive } from "@/lib/admin/route-matcher";
import { ADMIN_ROUTES, isAdminPath, isSafeAdminReturnUrl } from "@/lib/admin/routes";
import { PUBLIC_ABOUT_PATH } from "@/lib/public/about-routes";

describe("About navigation", () => {
  const appRoot = join(process.cwd(), "src", "app");

  it("exposes a public /about page and an admin /admin/about page", () => {
    expect(PUBLIC_ABOUT_PATH).toBe("/about");
    expect(ADMIN_ROUTES.about).toBe("/admin/about");
    expect(existsSync(join(appRoot, "about", "page.tsx"))).toBe(true);
    expect(existsSync(join(appRoot, "admin", "about", "page.tsx"))).toBe(true);
  });

  it("points public About / درباره ما links at /about, not settings", () => {
    const footer = renderToStaticMarkup(<PublicFooter />);
    expect(footer).toContain('href="/about"');
    expect(footer).toContain("About");

    const homeFooter = readFileSync(
      join(process.cwd(), "src/components/home/home-footer.tsx"),
      "utf8",
    );
    expect(homeFooter).toContain('{ href: "/about", label: "درباره ما" }');
    expect(homeFooter).not.toContain('href: "/settings"');
    expect(homeFooter).not.toContain('href: "/admin/settings"');
  });

  it("does not change Contact routing", () => {
    const footer = renderToStaticMarkup(<PublicFooter />);
    expect(footer).toContain('href="/settings"');
    expect(footer).toContain("Contact");
  });

  it("keeps public /about out of admin auth paths", () => {
    expect(isAdminPath(PUBLIC_ABOUT_PATH)).toBe(false);
    expect(isAdminPath("/admin/about")).toBe(true);
    expect(isSafeAdminReturnUrl(PUBLIC_ABOUT_PATH)).toBe(false);
    expect(isSafeAdminReturnUrl("/admin/about")).toBe(true);
    expect(findActiveNavItemId(ADMIN_NAVIGATION, PUBLIC_ABOUT_PATH)).toBeNull();
  });

  it("matches /admin/about to about management, never settings", () => {
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/about")).toBe("system-about");
    expect(findActiveNavItemId(ADMIN_NAVIGATION, "/admin/settings")).toBe("system-settings");
    expect(isRouteActive("/admin/about", ADMIN_ROUTES.settings)).toBe(false);
    expect(isRouteActive("/admin/about", ADMIN_ROUTES.about)).toBe(true);

    const aboutItem = flattenNavItems(ADMIN_NAVIGATION).find((item) => item.id === "system-about");
    expect(aboutItem?.href).toBe("/admin/about");
  });
});
