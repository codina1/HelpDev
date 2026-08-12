import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";

const SRC_ROOT = join(process.cwd(), "src");

// Files exempt from the canonical-route rule.
const EXEMPT_FILES = new Set(["config.ts"]);

// A string/template literal that begins with an unversioned "/api/..." path.
// Canonical "/api/v1..." routes are allowed.
const FORBIDDEN_LITERAL = /["'`]\/api\/(?!v1)/;

function collectSourceFiles(dir: string, acc: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) {
      collectSourceFiles(full, acc);
      continue;
    }

    if (!/\.(ts|tsx)$/.test(entry)) continue;
    if (/\.test\.(ts|tsx)$/.test(entry)) continue;
    if (EXEMPT_FILES.has(entry)) continue;

    acc.push(full);
  }

  return acc;
}

describe("frontend canonical-route enforcement", () => {
  it("contains no unversioned /api/... string literals in source", () => {
    const offenders: string[] = [];

    for (const file of collectSourceFiles(SRC_ROOT)) {
      const content = readFileSync(file, "utf8");
      if (FORBIDDEN_LITERAL.test(content)) {
        offenders.push(file);
      }
    }

    expect(offenders, `Unversioned /api/ routes found in:\n${offenders.join("\n")}`).toHaveLength(
      0,
    );
  });
});
