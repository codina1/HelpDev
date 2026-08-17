import { describe, expect, it } from "vitest";
import { isAdminPath } from "@/lib/admin/routes";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
  PUBLIC_PROMPT_LAB_PATH,
  publicPromptLabDetailPath,
} from "@/lib/public/prompt-lab-routes";

describe("public prompt lab route", () => {
  it("keeps the homepage on /prompt-lab outside admin", () => {
    expect(PUBLIC_PROMPT_LAB_PATH).toBe("/prompt-lab");
    expect(isAdminPath(PUBLIC_PROMPT_LAB_PATH)).toBe(false);
    expect(PROMPT_LAB_HERO_TITLE).toBe("Prompt Lab");
    expect(PROMPT_LAB_HERO_SUBTITLE).toContain("پرامپت‌های حرفه‌ای");
  });

  it("builds a public detail path that is not admin", () => {
    expect(publicPromptLabDetailPath("system-boundary-review")).toBe(
      "/prompt-lab/system-boundary-review",
    );
    expect(isAdminPath(publicPromptLabDetailPath("system-boundary-review"))).toBe(false);
  });
});
