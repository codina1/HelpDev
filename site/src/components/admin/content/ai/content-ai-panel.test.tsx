import { readFileSync } from "node:fs";
import { join } from "node:path";
import { renderToStaticMarkup } from "react-dom/server";
import { describe, expect, it } from "vitest";
import { ApiClientError } from "@/lib/api/errors";
import { ContentAiPanel } from "./content-ai-panel";

const noop = () => {};

describe("ContentAiPanel", () => {
  it("renders controlled action buttons without fabricated results", () => {
    const html = renderToStaticMarkup(
      <ContentAiPanel
        status="idle"
        result={null}
        activeAction={null}
        onRun={noop}
      />,
    );

    expect(html).toContain("تحلیل محتوا");
    expect(html).toContain("پیشنهاد عنوان");
    expect(html).toContain("ساخت توضیحات SEO");
    expect(html).toContain("ساخت FAQ");
    expect(html).toContain("ساخت ساختار مقاله");
    expect(html).not.toContain("نتیجه پیشنهاد");
    expect(html).not.toContain("[Fake]");
  });

  it("shows loading state without raw progress percentages", () => {
    const html = renderToStaticMarkup(
      <ContentAiPanel
        status="loading"
        result={null}
        activeAction="analyze"
        onRun={noop}
      />,
    );

    expect(html).toContain("در حال تولید پیشنهاد");
    expect(html).toMatch(/role="status"/);
    expect(html).not.toContain("%");
    expect(html).not.toMatch(/امتیاز\s*\d/);
  });

  it("shows result for human copy and never offers auto-apply", () => {
    const html = renderToStaticMarkup(
      <ContentAiPanel
        status="success"
        activeAction="outline"
        onRun={noop}
        result={{
          taskType: "OutlineGeneration",
          generatedText: "1. مقدمه\n2. بدنه",
          createdAtUtc: "2026-07-22T12:00:00Z",
          model: "fake-v1",
          provider: "Fake",
        }}
      />,
    );

    expect(html).toContain("نتیجه پیشنهاد");
    expect(html).toContain("1. مقدمه");
    expect(html).toContain("به‌صورت خودکار اعمال یا ذخیره نمی‌شود");
    expect(html).not.toContain("اعمال خودکار");
    expect(html).not.toContain("جایگزینی");
  });

  it("surfaces safe errors without leaking raw payloads", () => {
    const html = renderToStaticMarkup(
      <ContentAiPanel
        status="error"
        result={null}
        activeAction="faq"
        onRun={noop}
        error={
          new ApiClientError({
            message: "تولید پیشنهاد ناموفق بود.",
            status: 502,
            code: "content_ai_provider_failed",
            correlationId: "corr-ai-1",
          })
        }
      />,
    );

    expect(html).toContain("تولید پیشنهاد ناموفق بود");
    expect(html).toContain("corr-ai-1");
    expect(html).not.toContain("stack");
    expect(html).not.toContain("ApiKey");
    expect(html).not.toContain("systemInstruction");
  });

  it("source never auto-saves or fabricates offline AI text", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/admin/content/ai/content-ai-panel.tsx"),
      "utf8",
    );
    expect(source).not.toContain("updateContent");
    expect(source).not.toContain("publishContent");
    expect(source).not.toContain("[Fake]");
    expect(source).not.toContain("lorem ipsum");
  });
});
