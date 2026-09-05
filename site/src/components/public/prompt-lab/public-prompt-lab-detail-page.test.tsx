import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicPromptLabDetailError, PublicPromptLabDetailPage } from "./public-prompt-lab-detail-page";
import { toPromptLabDetail } from "@/lib/public/prompt-lab-mappers";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";
import type { PublicPromptDetailsDto } from "@/lib/api/promptlab";

const SAMPLE: PublicPromptDetailsDto = {
  id: "11111111-1111-1111-1111-111111111111",
  title: "بازبینی مرز ماژول",
  slug: "system-boundary-review",
  description: "پرامپت بررسی قرارداد دامنه.",
  content: "You are a staff engineer.",
  coverImage: "/home/cover-architecture.svg",
  mediaType: "Text",
  category: { id: "c1", name: "Coding", slug: "coding" },
  aiModel: { id: "m1", name: "Claude", slug: "claude", provider: "Anthropic" },
  views: 1240,
  copyCount: 186,
  publishedAt: "2026-08-16T08:00:00.000Z",
};

describe("public prompt lab detail page", () => {
  it("is mounted at /prompt-lab/[slug]", () => {
    expect(publicPromptLabDetailPath("system-boundary-review")).toBe(
      "/prompt-lab/system-boundary-review",
    );
    expect(existsSync(join(process.cwd(), "src", "app", "prompt-lab", "[slug]", "page.tsx"))).toBe(
      true,
    );
  });

  it("renders premium detail sections from API detail", () => {
    const detail = toPromptLabDetail(SAMPLE);
    const html = renderToStaticMarkup(
      <PublicPromptLabDetailPage detail={detail} related={[]} similar={[]} />,
    );
    expect(html).toContain(detail.title);
    expect(html).toContain(detail.category);
    expect(html).toContain(detail.aiModel);
    expect(html).toContain("کپی پرامپت");
    expect(html).toContain("متن کامل پرامپت");
    expect(html).toContain("نحوه استفاده از این پرامپت");
    expect(html).toContain("نمونه ورودی و خروجی");
    expect(html).toContain("نسخه‌ها و تاریخچه تغییرات");
    expect(html).toContain("اطلاعات کلی");
    expect(html).toContain("پرامپت‌های مشابه");
    expect(html).toContain("مقالات مرتبط");
    expect(html).toContain(detail.coverImage);
  });

  it("renders a recoverable error state", () => {
    const html = renderToStaticMarkup(<PublicPromptLabDetailError />);
    expect(html).toContain("پرامپت در دسترس نیست");
    expect(html).toContain("بازگشت به Prompt Lab");
  });

  it("loads prompt details through the API with mock fallback", () => {
    const page = readFileSync(join(process.cwd(), "src/app/prompt-lab/[slug]/page.tsx"), "utf8");
    expect(page).toContain("getPromptBySlug");
    expect(page).toContain("fetchPromptLabCatalog");
    expect(page).toContain("getPromptLabDetail");
    expect(page).not.toContain("@/components/admin");
  });
});
