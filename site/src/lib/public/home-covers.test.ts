import { describe, expect, it } from "vitest";
import {
  HOME_COVERS,
  coverForHomeCategory,
  coverForHomePath,
  coverForHomeValue,
} from "@/lib/public/home-covers";

describe("homepage local covers", () => {
  it("maps categories, paths, and value tiles to local assets", () => {
    expect(coverForHomeCategory("معماری")).toBe(HOME_COVERS.architecture);
    expect(coverForHomeCategory("هوش مصنوعی")).toBe(HOME_COVERS.ai);
    expect(coverForHomeCategory("بک‌اند")).toBe(HOME_COVERS.backend);
    expect(coverForHomeCategory("فرانت‌اند")).toBe(HOME_COVERS.frontend);
    expect(coverForHomeCategory("دواپس")).toBe(HOME_COVERS.devops);
    expect(coverForHomeCategory("سایر")).toBe(HOME_COVERS.article);
    expect(coverForHomePath("architect")).toBe(HOME_COVERS.architecture);
    expect(coverForHomeValue("ai")).toBe(HOME_COVERS.ai);
    expect(HOME_COVERS.hero.startsWith("/home/")).toBe(true);
  });
});
