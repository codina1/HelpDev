import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { AIEntryExperience } from "@/components/experience/ai-entry-experience";
import { CommandSearchBox } from "@/components/experience/command-search-box";

describe("Sprint 50E — AI entry", () => {
  it("renders Ask HelpDev AI entry with SaaS prompt", () => {
    const html = renderToStaticMarkup(<AIEntryExperience />);
    expect(html).toContain("Ask HelpDev AI");
    expect(html).toContain("چطور معماری یک سیستم SaaS را طراحی کنم؟");
    expect(html).toContain("ASP.NET Core");
  });

  it("CommandSearchBox is keyboard-affordance ready", () => {
    expect(renderToStaticMarkup(<CommandSearchBox />)).toContain('aria-label="باز کردن پالت فرمان با Ctrl+K"');
  });
});
