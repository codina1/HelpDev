import type { NewsItem } from "@/types";

export const NEWS_FEED: NewsItem[] = [
  {
    id: "1",
    title: "Next.js 15 App Router patterns that scale",
    description:
      "Practical layout, caching, and data-fetching patterns teams use once apps leave the prototype stage.",
    tag: "Next.js",
    time: "2h ago",
  },
  {
    id: "2",
    title: "TypeScript 5.8: what actually matters for app code",
    description:
      "A focused look at type improvements that reduce friction in large React and Node codebases.",
    tag: "TypeScript",
    time: "4h ago",
  },
  {
    id: "3",
    title: "Tailwind CSS v4 migration notes",
    description:
      "How to move from config-heavy setups to CSS-first tokens without breaking your design system.",
    tag: "CSS",
    time: "6h ago",
  },
  {
    id: "4",
    title: "Edge runtimes: when they help and when they hurt",
    description:
      "Latency wins, cold starts, and cost trade-offs for APIs that sit close to users.",
    tag: "Infrastructure",
    time: "Yesterday",
  },
  {
    id: "5",
    title: "Prompt engineering for code review",
    description:
      "Reusable prompt structures that catch correctness issues without drowning you in noise.",
    tag: "AI",
    time: "Yesterday",
  },
  {
    id: "6",
    title: "DX tooling worth adopting in 2026",
    description:
      "A shortlist of CLI and editor tools that improve feedback loops without adding process overhead.",
    tag: "Tooling",
    time: "2d ago",
  },
  {
    id: "7",
    title: "Design systems developers actually use",
    description:
      "Minimal tokens, clear component APIs, and fewer one-off styles in product teams.",
    tag: "Design",
    time: "3d ago",
  },
  {
    id: "8",
    title: "Shipping monorepos without the pain",
    description:
      "Task graphs, package boundaries, and CI strategies that keep shared libraries maintainable.",
    tag: "Architecture",
    time: "4d ago",
  },
];
