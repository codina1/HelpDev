import { readFileSync } from "node:fs";
import { join } from "node:path";
import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ApiClientError } from "@/lib/api/errors";
import type { SeoAnalysisReport } from "@/lib/admin/content/content-types";
import { SeoAnalysisPanel } from "./seo-analysis-panel";

const noop = () => {};

const sampleReport: SeoAnalysisReport = {
  contentId: "content-1",
  analyzedAtUtc: "2026-07-01T10:15:00Z",
  summary: { errorCount: 1, warningCount: 2, infoCount: 1 },
  findings: [
    {
      ruleId: "meta-description-missing",
      category: "Metadata",
      severity: "Error",
      passed: false,
      message: "توضیحات متا خالی است.",
      suggestion: "یک توضیح ۵۰ تا ۱۶۰ نویسه‌ای اضافه کنید.",
      field: "seoDescription",
      categoryLabel: "متادیتا",
      severityLabel: "خطا",
    },
    {
      ruleId: "seo.canonical.missing",
      category: "Technical",
      severity: "Warning",
      passed: false,
      message: "نشانی کانونیکال تنظیم نشده است.",
      suggestion: "در صورت نیاز یک URL مطلق http(s) تنظیم کنید.",
      field: "canonicalUrl",
      categoryLabel: "فنی",
      severityLabel: "هشدار",
    },
    {
      ruleId: "heading-structure-ok",
      category: "ContentStructure",
      severity: "Info",
      passed: true,
      message: "ساختار عنوان‌بندی مناسب است.",
      suggestion: null,
      field: "body",
      categoryLabel: "ساختار محتوا",
      severityLabel: "اطلاعاتی",
    },
  ],
};

// Text that must NEVER appear in the analyzer's rendered output.
const FORBIDDEN_UI_TEXT = [
  "امتیاز سئو",
  "درصد",
  "%",
  "پیش‌بینی رتبه",
  "رتبه‌بندی",
  "هوش مصنوعی",
];

describe("SeoAnalysisPanel", () => {
  it("idle: prompts the user to run the analysis, with no report/summary shown", () => {
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="idle" report={null} onAnalyze={noop} />,
    );
    expect(html).toContain("تحلیل");
    expect(html).not.toContain("موفق");
    expect(html).not.toContain("زمان تحلیل");
  });

  it("analyzing: announces the loading state and disables the Analyze button", () => {
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="analyzing" report={null} onAnalyze={noop} />,
    );
    expect(html).toContain("در حال تحلیل");
    expect(html).toMatch(/role="status"/);
    expect(html).toMatch(/aria-live="polite"/);
    expect(html).toMatch(/<button[^>]*disabled[^>]*>/);
  });

  it("success: shows error/warning/info counts and checklist", () => {
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="success" report={sampleReport} onAnalyze={noop} />,
    );
    expect(html).toContain("تحلیل براساس آخرین نسخهٔ ذخیره‌شده");
    expect(html).toContain("هشدار");
    expect(html).toContain("خطا");
    expect(html).toContain("اطلاعاتی");
    expect(html).toContain("چک‌لیست متادیتا");
    expect(html).toContain("توضیحات متا خالی است.");
    expect(html).not.toContain("موفق");
    expect(html).not.toContain("امتیاز");
  });

  it("success: renders no fabricated score, percentage, ranking, or AI wording", () => {
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="success" report={sampleReport} onAnalyze={noop} />,
    );
    for (const forbidden of FORBIDDEN_UI_TEXT) {
      expect(html).not.toContain(forbidden);
    }
  });

  it("stale: keeps the previous report visible but clearly labels it as out of date", () => {
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="stale" report={sampleReport} onAnalyze={noop} />,
    );
    expect(html).toContain("نسخهٔ قبلی");
    expect(html).toContain("تحلیل مجدد");
  });

  it("error: surfaces a safe message and a keyboard-accessible retry control (no raw error text)", () => {
    const error = new ApiClientError({ message: "raw-secret-detail", status: 500 });
    const html = renderToStaticMarkup(
      <SeoAnalysisPanel status="error" report={null} error={error} onAnalyze={noop} />,
    );
    expect(html).not.toContain("raw-secret-detail");
    expect(html).toContain("تلاش مجدد");
    expect(html).toMatch(/<button[^>]*>[\s\S]*تلاش مجدد/);
  });

  it("labels the Analyze/Rerun action accessibly, and switches its wording once a report exists", () => {
    const idleHtml = renderToStaticMarkup(
      <SeoAnalysisPanel status="idle" report={null} onAnalyze={noop} />,
    );
    expect(idleHtml).toMatch(/aria-label="اجرای تحلیل سئو"/);

    const successHtml = renderToStaticMarkup(
      <SeoAnalysisPanel status="success" report={sampleReport} onAnalyze={noop} />,
    );
    expect(successHtml).toMatch(/aria-label="اجرای دوباره تحلیل سئو"/);
  });
});

describe("SEO analyzer UI guardrails (static source scan)", () => {
  const files = [
    join(process.cwd(), "src/components/admin/content/seo/seo-analysis-panel.tsx"),
    join(process.cwd(), "src/components/admin/content/seo/seo-panel.tsx"),
    join(process.cwd(), "src/lib/admin/content/content-types.ts"),
    join(process.cwd(), "src/lib/admin/content/content-mappers.ts"),
    join(process.cwd(), "src/lib/admin/content/content-hooks.ts"),
    join(process.cwd(), "src/lib/admin/content/content-api.ts"),
    join(process.cwd(), "src/lib/api/content.ts"),
  ];

  it("never introduces a fabricated SEO score/percentage/ranking prediction identifier", () => {
    // Matched against identifiers/tokens only (no spaces) so honest doc
    // comments explaining what is deliberately NOT implemented never trip
    // this guardrail — it should only fail on actual implementation code.
    const forbidden = [
      /\bseoscore\b/i,
      /\bseo_score\b/i,
      /\bscorepercent\b/i,
      /\brankingprediction\b/i,
      /\brankprediction\b/i,
      /\bpredictedrank\b/i,
      /\brankingscore\b/i,
    ];
    const offenders: string[] = [];
    for (const file of files) {
      const text = readFileSync(file, "utf8");
      for (const re of forbidden) {
        if (re.test(text)) offenders.push(`${file} -> ${re}`);
      }
    }
    expect(offenders, `Fabricated SEO scoring found:\n${offenders.join("\n")}`).toHaveLength(0);
  });

  it("never imports an AI provider or external SEO SDK", () => {
    const forbidden = [
      /openai/i,
      /anthropic/i,
      /\bllm\b/i,
      /gpt-/i,
      /yoast/i,
      /rankmath/i,
      /moz\.com/i,
      /semrush/i,
      /ahrefs/i,
    ];
    const offenders: string[] = [];
    for (const file of files) {
      const text = readFileSync(file, "utf8");
      for (const re of forbidden) {
        if (re.test(text)) offenders.push(`${file} -> ${re}`);
      }
    }
    expect(offenders, `AI/external SEO SDK reference found:\n${offenders.join("\n")}`).toHaveLength(
      0,
    );
  });

  it("targets the canonical POST /admin/content/{id}/seo-analysis route only", () => {
    const resourceSource = readFileSync(
      join(process.cwd(), "src/lib/api/content.ts"),
      "utf8",
    );
    expect(resourceSource).toContain("/admin/content/${encodeURIComponent(id)}/seo-analysis`");
    expect(resourceSource).not.toMatch(/["'`]\/api\/(?!v1)/);
  });
});
