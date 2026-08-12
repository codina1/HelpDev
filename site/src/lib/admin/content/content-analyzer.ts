/**
 * Pure, frontend-only content analysis. It inspects REAL authored content and
 * reports factual measurements — never a fabricated "SEO score", grade, or
 * AI-derived judgement. All counting reuses the safe Markdown parser so link
 * counts match what the preview will actually render (unsafe links excluded).
 */

import { parseInline, parseMarkdown, type MarkdownBlock } from "@/lib/admin/content/markdown";

const WORDS_PER_MINUTE = 200;

export type ContentStatistics = {
  words: number;
  characters: number;
  /** Estimate only: words / 200, rounded up (min 1 for non-empty content). */
  readingMinutes: number;
  headings: number;
  codeBlocks: number;
  links: number;
};

export type ContentQualityReport = {
  title: boolean;
  titleLength: number;
  description: boolean;
  descriptionLength: number;
  bodyCharacters: number;
  bodyWords: number;
  headings: number;
  codeBlocks: number;
  links: number;
};

export type ContentAnalyzerInput = {
  title: string;
  description: string;
  body: string;
};

function countWords(text: string): number {
  const trimmed = text.trim();
  if (trimmed.length === 0) return 0;
  return trimmed.split(/\s+/u).filter(Boolean).length;
}

function countCharacters(text: string): number {
  // Code-point aware so multi-byte characters count as one.
  return Array.from(text).length;
}

function countLinksInText(text: string): number {
  return parseInline(text).filter((segment) => segment.kind === "link").length;
}

function countStructure(blocks: MarkdownBlock[]): {
  headings: number;
  codeBlocks: number;
  links: number;
} {
  let headings = 0;
  let codeBlocks = 0;
  let links = 0;

  for (const block of blocks) {
    switch (block.kind) {
      case "heading":
        headings += 1;
        links += countLinksInText(block.text);
        break;
      case "code":
        codeBlocks += 1;
        break;
      case "paragraph":
        links += countLinksInText(block.text);
        break;
      case "list":
        for (const item of block.items) {
          links += countLinksInText(item);
        }
        break;
    }
  }

  return { headings, codeBlocks, links };
}

/** Computes factual statistics for the content body. */
export function computeStatistics(body: string): ContentStatistics {
  const blocks = parseMarkdown(body);
  const words = countWords(body);
  const { headings, codeBlocks, links } = countStructure(blocks);
  const readingMinutes = words === 0 ? 0 : Math.max(1, Math.ceil(words / WORDS_PER_MINUTE));

  return {
    words,
    characters: countCharacters(body),
    readingMinutes,
    headings,
    codeBlocks,
    links,
  };
}

/**
 * Produces a factual quality report (presence + real counts). Callers render
 * these as neutral checklist items; there is deliberately no aggregate score.
 */
export function analyzeContent(input: ContentAnalyzerInput): ContentQualityReport {
  const stats = computeStatistics(input.body);
  const trimmedTitle = input.title.trim();
  const trimmedDescription = input.description.trim();

  return {
    title: trimmedTitle.length > 0,
    titleLength: Array.from(trimmedTitle).length,
    description: trimmedDescription.length > 0,
    descriptionLength: Array.from(trimmedDescription).length,
    bodyCharacters: stats.characters,
    bodyWords: stats.words,
    headings: stats.headings,
    codeBlocks: stats.codeBlocks,
    links: stats.links,
  };
}
