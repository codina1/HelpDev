import { describe, expect, it } from "vitest";
import { isAdminPath } from "@/lib/admin/routes";
import {
  PROMPT_LAB_HERO_SUBTITLE,
  PROMPT_LAB_HERO_TITLE,
  PUBLIC_PROMPT_LAB_PATH,
} from "@/lib/public/prompt-lab-routes";

describe("public prompt lab route", () => {
  it("keeps the homepage on /prompt-lab outside admin", () => {
    expect(PUBLIC_PROMPT_LAB_PATH).toBe("/prompt-lab");
    expect(isAdminPath(PUBLIC_PROMPT_LAB_PATH)).toBe(false);
    expect(PROMPT_LAB_HERO_TITLE).toBe("Prompt Lab");
    expect(PROMPT_LAB_HERO_SUBTITLE).toContain("پرامپت‌های حرفه‌ای");
  });
});
