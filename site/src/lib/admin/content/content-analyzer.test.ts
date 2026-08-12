import { describe, expect, it } from "vitest";
import { analyzeContent, computeStatistics } from "./content-analyzer";

describe("computeStatistics", () => {
  it("counts words and characters", () => {
    const stats = computeStatistics("one two three four");
    expect(stats.words).toBe(4);
    expect(stats.characters).toBe("one two three four".length);
  });

  it("counts headings, code blocks and links", () => {
    const body = [
      "# Title",
      "## Section",
      "Some text with a [link](https://x.dev) here.",
      "```",
      "const a = 1;",
      "```",
      "- item [second](https://y.dev)",
    ].join("\n");
    const stats = computeStatistics(body);
    expect(stats.headings).toBe(2);
    expect(stats.codeBlocks).toBe(1);
    expect(stats.links).toBe(2);
  });

  it("excludes unsafe links from the count (matches the safe preview)", () => {
    // Unsafe protocols are never counted (the preview renders them as text).
    expect(computeStatistics("[x](javascript:alert(1))").links).toBe(0);
    // Safe links across separate lines are counted.
    expect(
      computeStatistics("a [one](https://a.dev)\nb [two](https://b.dev)").links,
    ).toBe(2);
  });

  it("estimates reading time as ceil(words / 200), min 1 for non-empty", () => {
    const body = Array.from({ length: 400 }, () => "word").join(" ");
    expect(computeStatistics(body).readingMinutes).toBe(2);
    expect(computeStatistics("just a few words").readingMinutes).toBe(1);
    expect(computeStatistics("").readingMinutes).toBe(0);
  });

  it("returns zeros for empty content", () => {
    expect(computeStatistics("")).toEqual({
      words: 0,
      characters: 0,
      readingMinutes: 0,
      headings: 0,
      codeBlocks: 0,
      links: 0,
    });
  });
});

describe("analyzeContent", () => {
  it("reports factual presence and counts (no aggregate score)", () => {
    const report = analyzeContent({
      title: "My title",
      description: "",
      body: "# Heading\nsome body words",
    });
    expect(report.title).toBe(true);
    expect(report.description).toBe(false);
    expect(report.headings).toBe(1);
    expect(report.bodyWords).toBeGreaterThan(0);
    // Deliberately no score/grade field exists.
    expect(report).not.toHaveProperty("score");
    expect(report).not.toHaveProperty("grade");
  });

  it("measures title and description lengths by code point", () => {
    const report = analyzeContent({ title: "abc", description: "خلاصه", body: "" });
    expect(report.titleLength).toBe(3);
    expect(report.descriptionLength).toBe(5);
  });
});
