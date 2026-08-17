import { describe, expect, it } from "vitest";
import { HOME_COVERS } from "@/lib/public/home-covers";
import { coverForPromptLabCategory } from "@/lib/public/prompt-lab-covers";

describe("prompt lab local covers", () => {
  it("maps categories to existing HelpDev assets", () => {
    expect(coverForPromptLabCategory("image")).toBe(HOME_COVERS.frontend);
    expect(coverForPromptLabCategory("video")).toBe(HOME_COVERS.devops);
    expect(coverForPromptLabCategory("coding")).toBe(HOME_COVERS.architecture);
    expect(coverForPromptLabCategory("writing")).toBe(HOME_COVERS.article);
    expect(coverForPromptLabCategory("design")).toBe(HOME_COVERS.frontend);
    expect(coverForPromptLabCategory("marketing")).toBe(HOME_COVERS.ai);
    expect(coverForPromptLabCategory("education")).toBe(HOME_COVERS.backend);
    expect(coverForPromptLabCategory("coding").startsWith("/home/")).toBe(true);
  });
});
