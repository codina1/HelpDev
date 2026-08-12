import { describe, expect, it } from "vitest";
import { readFileSync } from "node:fs";
import { join } from "node:path";

const CONTENT_STUDIO_FILE = join(
  process.cwd(),
  "src/components/admin/content/editor/content-studio.tsx",
);
const SEO_PANEL_FILE = join(process.cwd(), "src/components/admin/content/seo/seo-panel.tsx");

describe("Content Studio — Cover image picker integration", () => {
  const source = readFileSync(CONTENT_STUDIO_FILE, "utf8");

  it("renders the Media Library picker and wires it to the cover image field", () => {
    expect(source).toContain("MediaPickerDialog");
    expect(source).toContain('setPickerTarget("cover")');
    expect(source).toContain("onChange({ coverImage: selection.absoluteUrl })");
  });

  it("wires the picker to the OG image field via the SEO panel callback", () => {
    expect(source).toContain('setPickerTarget("og")');
    expect(source).toContain("onSeoChange({ ogImage: selection.absoluteUrl })");
    expect(source).toContain("onPickOgImage={() => setPickerTarget(\"og\")}");
  });

  it("selecting an asset marks the SEO analysis stale via the existing onChange/onSeoChange paths (no bypass)", () => {
    // handleMediaSelect must route through onChange/onSeoChange — the same
    // functions that already call seoAnalysis.markStale() — never mutate
    // `values`/`seo` state directly.
    const handlerMatch = source.match(
      /const handleMediaSelect = useCallback\(([\s\S]*?)\n\s*\},\s*\n\s*\[pickerTarget, onChange, onSeoChange\],\s*\n\s*\);/,
    );
    expect(handlerMatch).not.toBeNull();
    const handlerBody = handlerMatch?.[1] ?? "";
    expect(handlerBody).toContain("onChange(");
    expect(handlerBody).toContain("onSeoChange(");
    expect(handlerBody).not.toContain("setValues(");
    expect(handlerBody).not.toContain("setSeo(");
  });

  it("never auto-saves on selection (no update.run/seoMutation.run call inside the picker handler)", () => {
    const handlerMatch = source.match(
      /const handleMediaSelect = useCallback\(([\s\S]*?)\n\s*\},\s*\n\s*\[pickerTarget, onChange, onSeoChange\],\s*\n\s*\);/,
    );
    const handlerBody = handlerMatch?.[1] ?? "";
    expect(handlerBody).not.toContain("update.run(");
    expect(handlerBody).not.toContain("seoMutation.run(");
  });
});

describe("SEO panel — OG image picker integration", () => {
  const source = readFileSync(SEO_PANEL_FILE, "utf8");

  it("exposes an optional onPickOgImage action next to the OG image field", () => {
    expect(source).toContain("onPickOgImage?: () => void");
    expect(source).toContain("onClick={onPickOgImage}");
  });
});
