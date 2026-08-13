import { readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  HOME_WORKFLOW_QUESTION,
  HOME_WORKFLOW_STEPS,
  HomeWorkflowSection,
} from "@/components/public/home/home-workflow-section";

describe("homepage workflow section", () => {
  it("renders the title, AI input mockup, and five numbered steps", () => {
    const html = renderToStaticMarkup(<HomeWorkflowSection />);
    expect(html).toContain("از سؤال تا راهکار با هوش HelpDev");
    expect(html).toContain("text-start");
    expect(html).not.toContain("text-center");
    expect(html).toContain(HOME_WORKFLOW_QUESTION);
    expect(html).toContain("/learning/assistant");
    expect(html).toContain("home-workflow-line");
    expect(html).toContain("home-workflow-node-active");
    expect(html).toContain("تصمیم فناوری");
    for (const step of HOME_WORKFLOW_STEPS) {
      expect(html).toContain(step.title);
      expect(html).toContain(step.caption);
    }
    expect(html).toContain("۱");
    expect(html).toContain("۵");
  });

  it("keeps the connector between numbered nodes instead of over them", () => {
    const css = readFileSync(join(process.cwd(), "src/app/globals.css"), "utf8");
    expect(css).toContain("inset-inline: calc(10% + 1.5rem)");
    expect(css).toContain("background: var(--home-bg-elevated)");
  });
});
