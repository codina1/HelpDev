import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";
import { isAdminPath } from "@/lib/admin/routes";
import { PublicPromptLabPage } from "./public-prompt-lab-page";

describe("public prompt lab homepage", () => {
  it("is mounted at /prompt-lab and is not an admin route", () => {
    expect(PUBLIC_PROMPT_LAB_PATH).toBe("/prompt-lab");
    expect(isAdminPath(PUBLIC_PROMPT_LAB_PATH)).toBe(false);
    expect(existsSync(join(process.cwd(), "src", "app", "prompt-lab", "page.tsx"))).toBe(true);
  });

  it("renders hero, categories, and loading catalog shells", () => {
    const html = renderToStaticMarkup(<PublicPromptLabPage />);
    expect(html).toContain("Prompt Lab");
    expect(html).toContain("کدنویسی");
    expect(html).toContain("پرامپت‌های منتخب");
    expect(html).toContain("پرامپت‌های محبوب");
    expect(html).toContain("تازه‌ترین پرامپت‌ها");
    expect(html).toContain("aria-busy");
  });

  it("loads the public catalog through the existing API client", () => {
    const page = readFileSync(join(process.cwd(), "src/app/prompt-lab/page.tsx"), "utf8");
    const shell = readFileSync(
      join(process.cwd(), "src/components/public/prompt-lab/public-prompt-lab-page.tsx"),
      "utf8",
    );
    expect(page).toContain("PublicPromptLabPage");
    expect(shell).toContain("fetchPromptLabCatalog");
    expect(shell).not.toContain("PROMPT_LAB_PROMPTS");
    expect(shell).not.toContain("@/components/admin");
    expect(shell).not.toContain("@/components/public/home");
  });
});
