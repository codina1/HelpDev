import type { NewsArticle, NewsTag } from "@/types";

export const NEWS_TAGS: NewsTag[] = ["React", ".NET", "AI", "DevOps"];

export const NEWS_ARTICLES: NewsArticle[] = [
  {
    id: "1",
    title: "React 19 Compiler moves closer to stable",
    tag: "React",
    summary:
      "Automatic memoization lands in more production apps, with clearer guidance on when to keep manual optimizations.",
    time: "1h ago",
  },
  {
    id: "2",
    title: ".NET 10 preview focuses on cloud-native APIs",
    tag: ".NET",
    summary:
      "Faster minimal APIs, improved OpenAPI generation, and leaner container images for ASP.NET Core services.",
    time: "2h ago",
  },
  {
    id: "3",
    title: "AI coding agents get better at multi-file refactors",
    tag: "AI",
    summary:
      "New evaluation suites show stronger results on repository-wide changes, not just single-function edits.",
    time: "3h ago",
  },
  {
    id: "4",
    title: "Kubernetes 1.33 tightens supply-chain defaults",
    tag: "DevOps",
    summary:
      "Signed artifacts and stricter admission policies become easier to enable without custom controllers.",
    time: "5h ago",
  },
  {
    id: "5",
    title: "Server Components patterns that survive scale",
    tag: "React",
    summary:
      "Teams share caching boundaries, streaming layouts, and data-loading rules that hold up under real traffic.",
    time: "Yesterday",
  },
  {
    id: "6",
    title: "C# 14 proposals aim at everyday productivity",
    tag: ".NET",
    summary:
      "Smaller language improvements target null handling, collection expressions, and clearer diagnostics.",
    time: "Yesterday",
  },
  {
    id: "7",
    title: "Prompt evaluation becomes standard CI practice",
    tag: "AI",
    summary:
      "Engineering teams treat prompt regressions like unit tests, with fixtures and score thresholds in pipelines.",
    time: "Yesterday",
  },
  {
    id: "8",
    title: "Platform teams standardize on OpenTelemetry",
    tag: "DevOps",
    summary:
      "Traces, metrics, and logs converge on one collector path across services written in different languages.",
    time: "2d ago",
  },
  {
    id: "9",
    title: "React Native’s New Architecture adoption accelerates",
    tag: "React",
    summary:
      "Fabric and TurboModules become the default path for new apps, with migration guides for legacy modules.",
    time: "2d ago",
  },
  {
    id: "10",
    title: "Blazor United simplifies full-stack .NET UI",
    tag: ".NET",
    summary:
      "A single project model blends server and client rendering, reducing ceremony for internal business apps.",
    time: "3d ago",
  },
  {
    id: "11",
    title: "Local LLMs get practical for private codebases",
    tag: "AI",
    summary:
      "Smaller open models and better tooling make on-device assistants viable for teams with strict data policies.",
    time: "3d ago",
  },
  {
    id: "12",
    title: "GitHub Actions cost controls get more granular",
    tag: "DevOps",
    summary:
      "Per-workflow budgets and idle runner limits help teams cut CI spend without slowing critical pipelines.",
    time: "4d ago",
  },
];
