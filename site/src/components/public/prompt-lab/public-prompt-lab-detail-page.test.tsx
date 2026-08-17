import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicPromptLabDetailPage } from "./public-prompt-lab-detail-page";
import {
  getPromptLabDetail,
  relatedPromptLabPrompts,
  similarPromptLabPrompts,
} from "@/lib/public/prompt-lab-detail-mock";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";

describe("public prompt lab detail page", () => {
  it("is mounted at /prompt-lab/[slug]", () => {
    expect(publicPromptLabDetailPath("system-boundary-review")).toBe(
      "/prompt-lab/system-boundary-review",
    );
    expect(existsSync(join(process.cwd(), "src", "app", "prompt-lab", "[slug]", "page.tsx"))).toBe(
      true,
    );
  });

  it("renders hero, description, viewer, sidebar, similar, and author from mock data", () => {
    const detail = getPromptLabDetail("system-boundary-review");
    expect(detail).not.toBeNull();
    const html = renderToStaticMarkup(
      <PublicPromptLabDetailPage
        detail={detail!}
        related={relatedPromptLabPrompts(detail!.slug)}
        similar={similarPromptLabPrompts(detail!.slug)}
      />,
    );
    expect(html).toContain(detail!.title);
    expect(html).toContain(detail!.category);
    expect(html).toContain(detail!.aiModel);
    expect(html).toContain(detail!.author.name);
    expect(html).toContain("شرح پرامپت");
    expect(html).toContain("متن پرامپت");
    expect(html).toContain('aria-label="کپی پرامپت"');
    expect(html).toContain("پرامپت‌های مرتبط");
    expect(html).toContain("برچسب‌ها");
    expect(html).toContain("پرامپت‌های مشابه");
    expect(html).toContain("درباره نویسنده");
    expect(html).toContain(detail!.coverImage);
    expect(html).toContain('dir="rtl"');
  });

  it("keeps the detail route free of API imports", () => {
    const page = readFileSync(join(process.cwd(), "src/app/prompt-lab/[slug]/page.tsx"), "utf8");
    expect(page).toContain("getPromptLabDetail");
    expect(page).not.toContain("promptLabApi");
    expect(page).not.toContain("@/lib/api");
    expect(page).not.toContain("@/components/admin");
  });
});
