import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

describe("user dashboard theme", () => {
  const source = readFileSync(join(process.cwd(), "src/components/dashboard/user-dashboard.tsx"), "utf8");

  it("uses design tokens so light and dark themes stay readable", () => {
    expect(source).toContain("var(--ds-fg)");
    expect(source).toContain("var(--ds-muted)");
    expect(source).toContain("var(--ds-primary-strong)");
    expect(source).toContain("var(--ds-surface)");
    expect(source).not.toContain("text-white");
    expect(source).not.toContain("text-slate-");
    expect(source).not.toContain("border-white/10");
    expect(source).not.toContain("text-violet-");
  });
});
