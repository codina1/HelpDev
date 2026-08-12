import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

const API_FILE = join(process.cwd(), "src/lib/admin/dashboard/dashboard-api.ts");
const DASHBOARD_DIRS = [
  join(process.cwd(), "src/lib/admin/dashboard"),
  join(process.cwd(), "src/components/admin/dashboard"),
];

function collect(dir: string, acc: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) {
      collect(full, acc);
      continue;
    }
    if (!/\.(ts|tsx)$/.test(entry)) continue;
    if (/\.test\.(ts|tsx)$/.test(entry)) continue;
    acc.push(full);
  }
  return acc;
}

describe("dashboard API sources", () => {
  it("targets only existing canonical admin endpoints", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain('"/admin/dashboard"');
    expect(content).toContain('"/admin/operations/health"');
    expect(content).toContain('"/admin/operations/status"');
    expect(content).toContain("/admin/audit?page=1&pageSize=");
  });

  it("does not duplicate the HTTP client (reuses apiRequest / content module)", () => {
    const content = readFileSync(API_FILE, "utf8");
    expect(content).toContain("apiRequest");
    expect(content).toContain("listPublishedContent");
    expect(content).not.toMatch(/\bfetch\s*\(/);
  });
});

describe("dashboard has no fabricated metrics", () => {
  // Distinctive example figures from the spec that must never be hardcoded.
  const FORBIDDEN = ["18520", "18,520", "1240", "8420", "99.9"];

  it("contains none of the sample numbers as literals", () => {
    const offenders: string[] = [];
    for (const dir of DASHBOARD_DIRS) {
      for (const file of collect(dir)) {
        const content = readFileSync(file, "utf8");
        for (const needle of FORBIDDEN) {
          if (content.includes(needle)) offenders.push(`${file} -> ${needle}`);
        }
      }
    }
    expect(offenders, `Hardcoded metrics found:\n${offenders.join("\n")}`).toHaveLength(0);
  });

  it("uses no unversioned /api/ literals in dashboard sources", () => {
    const offenders: string[] = [];
    for (const dir of DASHBOARD_DIRS) {
      for (const file of collect(dir)) {
        if (/["'`]\/api\/(?!v1)/.test(readFileSync(file, "utf8"))) offenders.push(file);
      }
    }
    expect(offenders).toHaveLength(0);
  });
});
