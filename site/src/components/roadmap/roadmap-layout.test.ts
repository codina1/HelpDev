import { describe, expect, it } from "vitest";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { ROADMAP_STATS } from "@/data/roadmap-paths";

describe("roadmap listing layout", () => {
  it("uses a wide shared container (~95vw / 1400px)", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/roadmap/roadmap-container.tsx"),
      "utf8",
    );
    expect(source).toContain("w-[95%]");
    expect(source).toContain("max-w-[1400px]");
    expect(source).not.toContain("max-w-[1136px]");
  });

  it("keeps hero CTAs and single-line title highlight for توسعه‌دهنده", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/roadmap/roadmap-hero.tsx"),
      "utf8",
    );
    expect(source).toContain("نمایش همه مسیرها");
    expect(source).toContain("راهنمای استفاده");
    expect(source).toContain("md:h-[275px]");
    expect(source).toContain("توسعه‌دهنده");
    expect(source).toContain("حرفه‌ای");
    expect(source).toMatch(/توسعه‌دهنده[\s\S]*حرفه‌ای/);
  });

  it("orders stats RTL-first as 12+ · 120+ · 40+ · 24K+", () => {
    expect(ROADMAP_STATS.map((item) => item.value)).toEqual(["۱۲+", "۱۲۰+", "۴۰+", "۲۴K+"]);
  });

  it("uses full-width guide without narrow max-width override", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/roadmap/roadmap-guide.tsx"),
      "utf8",
    );
    expect(source).not.toContain("md:max-w-[880px]");
    expect(source).toContain("md:h-[155px]");
    expect(source).toContain("راهنمای کامل");
  });
});
